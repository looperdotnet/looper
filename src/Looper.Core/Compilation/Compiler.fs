module Looper.Core.Compiler

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SymbolUtils


    let private throwNotImplemented =
        SyntaxFactory.ThrowStatement(
            SyntaxFactory.ObjectCreationExpression(
                SyntaxFactory.IdentifierName("System.NotImplementedException"))
                .WithArgumentList(SyntaxFactory.ArgumentList())) :> SyntaxNode

    let syntaxList<'Node when 'Node :> SyntaxNode> (elements : seq<'Node>) = 
        SyntaxFactory.SeparatedList(elements)
    let varStmt = 
        SyntaxFactory.VariableDeclaration(
            SyntaxFactory.IdentifierName("var")).
            WithVariables(
            SyntaxFactory.SingletonSeparatedList(
                SyntaxFactory.VariableDeclarator(
                    SyntaxFactory.Identifier("__i__")). // TODO: replace with fresh names
                    WithInitializer(
                    SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)))))).NormalizeWhitespace()
    let forStmt = 
        let lessThenExpr = SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression, SyntaxFactory.IdentifierName("__i__"), SyntaxFactory.IdentifierName("__i__"))
        SyntaxFactory.ForStatement(varStmt, syntaxList [||], lessThenExpr, syntaxList [||], SyntaxFactory.EmptyStatement())

    let rec compileQuery (query : QueryExpr) (model : SemanticModel) (k : SyntaxNode -> SyntaxNode) : SyntaxNode =
        
        match query with
        | SourceIdentifierName (ArrayType arrayTypeSymbol, identifier) ->    
            varStmt :> _
        | Select (Lambda (param, body), query) ->
             compileQuery query model k
        | Sum query -> 
            compileQuery query model k
        | _ -> throwNotImplemented

    let compile (query : StmtQueryExpr) (model : SemanticModel) : SyntaxNode =
        match query with
        | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
            compileQuery queryExpr model id 
        | _ -> throwNotImplemented
