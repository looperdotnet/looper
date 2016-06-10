namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    module QueryTransformer = 

        let producerMatch (node : SyntaxNode) : QueryExpr option =
            match node with 
            | IdentifierName _ -> Some (SourceIdentifierName (node :?> IdentifierNameSyntax))
            | _ -> None

        let rec intermediateMatch (node : SyntaxNode) : QueryExpr option = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Select", expr), [SimpleLambdaExpression (param, body)]) ->
                let lambda = SyntaxFactory.SimpleLambdaExpression(param, body)
                intermediateMatch expr |> Option.map (fun expr -> Select (lambda, expr))
            | InvocationExpression (MemberAccessExpression (IdentifierName "Where", expr), [SimpleLambdaExpression (param, body)]) ->
                let lambda = SyntaxFactory.SimpleLambdaExpression(param, body)
                intermediateMatch expr |> Option.map (fun expr -> Select (lambda, expr))
            | _ -> producerMatch node 

        let consumerMatch (node : SyntaxNode) : QueryExpr option = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Sum", expr), args) ->
                intermediateMatch expr |> Option.map Sum 
            | InvocationExpression (MemberAccessExpression (IdentifierName "First", expr), args) ->
                intermediateMatch expr |> Option.map First 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Any", expr), args) ->
                intermediateMatch expr |> Option.map Any
            | _ -> None

        let toQueryExpr (node : SyntaxNode) : QueryExpr option =
            consumerMatch node

        let (|QueryExpr|_|) (node : SyntaxNode) =
            toQueryExpr node

        let toStmtQueryExpr (node : SyntaxNode) : StmtQueryExpr option =
            match node with
            | LocalDeclarationStatement (modifiers, VariableDeclaration (t, [VariableDeclarator (identifier, _, EqualsValueClause (QueryExpr expr))])) -> 
                let identifierNameSyntax = SyntaxFactory.IdentifierName(identifier)
                Some (Assign (identifierNameSyntax, expr))
            | _ -> None

        let (|StmtQueryExpr|_|) (node : SyntaxNode) =
            toStmtQueryExpr node

        let (|NoConsumerQuery|_|) (node : SyntaxNode) =
            match intermediateMatch node with
            | Some _ as m when producerMatch node = None -> m
            | _ -> None

        let (|StmtNoConsumerQuery|_|) (node : SyntaxNode) =
            match node with
            | LocalDeclarationStatement (_, VariableDeclaration (_, [VariableDeclarator (identifier, _, EqualsValueClause (NoConsumerQuery expr))])) -> 
                let identifierNameSyntax = SyntaxFactory.IdentifierName(identifier)
                Some (Assign (identifierNameSyntax, expr))
            | _ -> None