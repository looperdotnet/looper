module Looper.Core.QueryTransformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns
    open Looper.Core.SymbolUtils

    let (|LinqInvocation|_|) (model : SemanticModel) = 
        function 
        | InvocationExpression(MemberAccessExpression(IdentifierName name, expr) as memberExpr, args) -> 
            match model.GetSymbolInfo(memberExpr).Symbol with
            | LinqMethod _ -> Some(expr, name, args)
            | _ -> None
        | _ -> None

    let producerMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option =
        match node with 
        | IdentifierName _ -> Some (SourceIdentifierName (node :?> IdentifierNameSyntax))
        | :? ExpressionSyntax as expr -> Some(SourceExpression expr)
        | _ -> None

    let rec intermediateMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option = 
        match node with 
        | LinqInvocation model (expr, "Select", [SimpleLambdaExpression _ as lambda]) ->
            intermediateMatch expr model |> Option.map (fun expr -> Select (lambda :?> SimpleLambdaExpressionSyntax, expr))
        | LinqInvocation model (expr, "Where", [SimpleLambdaExpression _ as lambda]) ->
            intermediateMatch expr model |> Option.map (fun expr -> Where (lambda :?> SimpleLambdaExpressionSyntax, expr))
        | _ -> producerMatch node model

    let consumerMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option = 
        match node with 
        | LinqInvocation model (expr, "Sum", []) ->
            intermediateMatch expr model |> Option.map Sum 
        | LinqInvocation model (expr, "First", []) ->
            intermediateMatch expr model |> Option.map First 
        | LinqInvocation model (expr, "Any", []) ->
            intermediateMatch expr model |> Option.map Any
        | _ -> None

    let toQueryExpr (node : SyntaxNode) (model : SemanticModel) : QueryExpr option =
        consumerMatch node model

    let (|QueryExpr|_|) (model : SemanticModel) (node : SyntaxNode) =
        toQueryExpr node model

    let toStmtQueryExpr (node : SyntaxNode) (model : SemanticModel) : StmtQueryExpr option =
        match node with
        | LocalDeclarationStatement (modifiers, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (QueryExpr model expr))])) -> 
            let typeSymbol = model.GetSymbolInfo(typeSyntax).Symbol :?> INamedTypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None

    let (|StmtQueryExpr|_|) (model : SemanticModel) (node : SyntaxNode) =
        toStmtQueryExpr node model

    let (|NoConsumerQuery|_|) (model : SemanticModel) (node : SyntaxNode) =
        match intermediateMatch node model with
        | Some _ as m when producerMatch node model = None -> m
        | _ -> None

    let (|StmtNoConsumerQuery|_|) (model : SemanticModel) (node : SyntaxNode)  =
        match node with
        | LocalDeclarationStatement (_, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (NoConsumerQuery model expr))])) -> 
            let typeSymbol = model.GetSymbolInfo(typeSyntax).Symbol :?> INamedTypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None