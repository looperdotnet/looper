module Looper.Core.Compiler

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System


    let private throwNotImplemented =
        SyntaxFactory.ThrowStatement(
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("System.NotImplementedException"))
                .WithArgumentList(SyntaxFactory.ArgumentList())) :> SyntaxNode

    let rec compileQuery (query : QueryExpr) (model : SemanticModel) (k : SyntaxNode -> SyntaxNode) : SyntaxNode =
        match query with
        | SourceIdentifierName (typeSymbol, identifier) ->
            throwNotImplemented
        | Select (Lambda (param, body), query) ->
             compileQuery query model k
        | Sum query -> 
            compileQuery query model k
        | _ -> throwNotImplemented

    let compile (query : StmtQueryExpr) (model : SemanticModel) : SyntaxNode =
        match query with
        | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
            throwNotImplemented
        | _ -> throwNotImplemented
