namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    module QueryTransformer = 

        let producerMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option =
            match node with 
            | IdentifierName _ -> Some (SourceIdentifierName (node :?> IdentifierNameSyntax))
            | _ -> None

        let rec intermediateMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Select", expr), [SimpleLambdaExpression (param, body)]) ->
                let lambda = SyntaxFactory.SimpleLambdaExpression(param, body)
                intermediateMatch expr model |> Option.map (fun expr -> Select (lambda, expr))
            | InvocationExpression (MemberAccessExpression (IdentifierName "Where", expr), [SimpleLambdaExpression (param, body)]) ->
                let lambda = SyntaxFactory.SimpleLambdaExpression(param, body)
                intermediateMatch expr model |> Option.map (fun expr -> Select (lambda, expr))
            | _ -> producerMatch node model

        let consumerMatch (node : SyntaxNode) (model : SemanticModel) : QueryExpr option = 
            match node with 
            | InvocationExpression (MemberAccessExpression ((IdentifierName "Sum" as ident), expr), args) ->
                intermediateMatch expr model |> Option.map Sum 
            | InvocationExpression (MemberAccessExpression (IdentifierName "First", expr), args) ->
                intermediateMatch expr model |> Option.map First 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Any", expr), args) ->
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