namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    module QueryTransformer = 

        let producerMatch (node : SyntaxNode) : QueryExpr =
            match node with
            | IdentifierName _ -> SourceIdentifierName (node :?> IdentifierNameSyntax)
            | _ -> failwith "oups" 

        let rec intermediateMatch (node : SyntaxNode) : QueryExpr = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Select", expr), [SimpleLambdaExpression (param, body)]) ->
                let lambda = SyntaxFactory.SimpleLambdaExpression(param, body)
                Select (lambda, intermediateMatch expr)
            | _ -> producerMatch node 

        let consumerMatch (node : SyntaxNode) : QueryExpr = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Sum", expr), args) ->
                Sum (intermediateMatch expr)
            | _ -> failwith "oups" 


        let toQueryExpr (node : SyntaxNode) : QueryExpr =
            consumerMatch node

        let toStmtQueryExpr (node : SyntaxNode) : StmtQueryExpr =
            match node with
            | LocalDeclarationStatement (modifiers, declaration) -> 
                failwith "oups"
            | _ -> failwith "oups" 
