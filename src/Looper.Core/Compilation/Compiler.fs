module Looper.Core.Compiler

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SymbolUtils
    open Looper.Core.SyntaxPatterns

    let toStr (node : SyntaxNode) = node.ToFullString()

    let block (stmts : seq<StatementSyntax>) =
        SyntaxFactory.Block(stmts)

    let parseExpr (expr : string) : ExpressionSyntax = 
        SyntaxFactory.ParseExpression(expr)

    let parseStmt (stmt : string) : StatementSyntax = 
        SyntaxFactory.ParseStatement(stmt)

    let parseFor (identifier : string) : ForStatementSyntax = 
        let stmt = parseStmt(sprintf "for (int __i__ = 0; __i__ < %s.Length; __i__++);" identifier)
        stmt :?> ForStatementSyntax
    
    let parseIndexer (identifier : string) : ExpressionSyntax =
        parseExpr(sprintf "%s[__i__]" identifier)

    let  throwNotImplemented =
        parseStmt "throw new System.NotImplementedException();" 

    let rec compileQuery (query : QueryExpr) (model : SemanticModel) (k : ExpressionSyntax -> StatementSyntax) : StatementSyntax =
        
        match query with
        | SourceIdentifierName (ArrayType arrayTypeSymbol, identifier) ->    
            let identifier = identifier.Identifier.ValueText
            let forStmt = parseFor identifier
            let stmt = k (parseIndexer identifier)
            forStmt.WithStatement(block [stmt]) :> _
        | Select (Lambda (param, Expression body), query) ->
            let identifier = param.Identifier.ValueText
            let k expr = block [parseStmt (sprintf "var %s = %s;" identifier (toStr expr));
                                k body] :> StatementSyntax
            compileQuery query model k
        | Sum query -> 
            let varDeclStmt = parseStmt "var __sum__ = 0;"
            let assignStmt value = parseStmt (sprintf "__sum__ += %s;" value)
            let k expr = block [assignStmt (toStr expr); k (parseExpr "__sum__")] :> StatementSyntax
            let loopStmt = compileQuery query model k
            block [varDeclStmt; loopStmt] :> _
        | _ -> throwNotImplemented

    let compile (query : StmtQueryExpr) (model : SemanticModel) : StatementSyntax =
        match query with
        | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
            let varDeclStmt = parseStmt (sprintf "var %s = default(%s);" identifier.ValueText typeSymbol.Name)
            let assignStmt value = parseStmt (sprintf "%s = %s;" identifier.ValueText value)
            let loopStmt = compileQuery queryExpr model (fun expr -> assignStmt (toStr expr)) 
            block [varDeclStmt; loopStmt] :> _
        | _ -> throwNotImplemented
