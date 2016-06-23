module Looper.Core.QueryTransformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    let private (|LinqInvocation|_|) (checker : SymbolChecker) = 
        function 
        | InvocationExpression(MemberAccessExpression(IdentifierName name, expr) as memberExpr, args) -> 
            let symbol = checker.SemanticModel.GetSymbolInfo(memberExpr).Symbol
            match checker.IsLinqMethodType(symbol) with
            | Some _ -> Some(expr, name, args)
            | _ -> None
        | _ -> None

    let private producerMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option =
        match node with 
        | IdentifierName _ -> 
            match checker.SemanticModel.GetSymbolInfo(node).Symbol with
            | :? ILocalSymbol as symbol -> Some (SourceIdentifierName (symbol.Type, node :?> IdentifierNameSyntax))
            | _ -> None
        | :? ExpressionSyntax as expr -> Some(SourceExpression expr)
        | _ -> None

    let rec private intermediateMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option = 
        match node with 
        | LinqInvocation checker (expr, "Select", [SimpleLambdaExpression (param, body)]) ->
            intermediateMatch expr checker |> Option.map (fun expr -> Select (Lambda (param, body), expr))
        | LinqInvocation checker (expr, "Where", [SimpleLambdaExpression (param, body)]) ->
            intermediateMatch expr checker |> Option.map (fun expr -> Where (Lambda (param, body), expr))
        | _ -> producerMatch node checker

    let private consumerMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option = 
        match node with 
        | LinqInvocation checker (expr, "Sum", []) ->
            intermediateMatch expr checker |> Option.map Sum 
        | LinqInvocation checker (expr, "First", []) ->
            intermediateMatch expr checker |> Option.map First 
        | LinqInvocation checker (expr, "Any", []) ->
            intermediateMatch expr checker |> Option.map Any
        | _ -> None

    let toQueryExpr (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option =
        consumerMatch node checker

    let (|QueryExpr|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        toQueryExpr node checker

    let toStmtQueryExpr (node : SyntaxNode) (checker : SymbolChecker) : StmtQueryExpr option =
        match node with
        | LocalDeclarationStatement (_, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (QueryExpr checker expr))])) -> 
            let typeSymbol = checker.SemanticModel.GetSymbolInfo(typeSyntax).Symbol :?> INamedTypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None

    let (|StmtQueryExpr|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        toStmtQueryExpr node checker

    let (|NoConsumerQuery|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        match intermediateMatch node checker with
        | Some _ as m when producerMatch node checker = None -> m
        | _ -> None

    let (|StmtNoConsumerQuery|_|) (checker : SymbolChecker) (node : SyntaxNode)  =
        match node with
        | LocalDeclarationStatement (_, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (NoConsumerQuery checker expr))])) -> 
            let typeSymbol = checker.SemanticModel.GetSymbolInfo(typeSyntax).Symbol :?> ITypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None