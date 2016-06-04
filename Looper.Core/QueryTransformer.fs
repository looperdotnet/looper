namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    module QueryTransformer = 

        let intermediateMatch (node : SyntaxNode) : QueryExpr = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Select", expr), args) ->
                failwith "oups" 
            | _ -> failwith "oups" 

        let consumerMatch (node : SyntaxNode) : QueryExpr = 
            match node with 
            | InvocationExpression (MemberAccessExpression (IdentifierName "Sum", expr), args) ->
                intermediateMatch expr
            | _ -> failwith "oups" 


        let toQueryExpr (node : SyntaxNode) : QueryExpr =
            consumerMatch node
