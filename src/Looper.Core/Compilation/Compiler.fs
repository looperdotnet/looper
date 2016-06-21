﻿module Looper.Core.Compiler

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SymbolUtils
    open Looper.Core.SyntaxPatterns

    let rec compileQuery (query : QueryExpr) (model : SemanticModel) (k : ExpressionSyntax -> StatementSyntax) : StatementSyntax =
        
        match query with
        | SourceExpression expr ->
            let foreachStmt = parseForeach "__item__" (toStr expr)
            let stmt = k (parseExpr "__item__")
            foreachStmt.WithStatement(block [stmt]) :> _
        | SourceIdentifierName (ArrayType arrayTypeSymbol, identifier) ->    
            let identifier = identifier.Identifier.ValueText
            let forStmt = parseFor identifier
            let stmt = k (parseIndexer identifier)
            forStmt.WithStatement(block [stmt]) :> _
        | Select (Lambda (param, Expression body), query) ->
            let identifier = param.Identifier.ValueText
            let k expr = block [parseStmtf "var %s = %s;" identifier (toStr expr)
                                k body] :> StatementSyntax
            compileQuery query model k
        | Sum query -> 
            let varDeclStmt = parseStmt "var __sum__ = 0;"
            let assignStmt value = parseStmtf "__sum__ += %s;" value
            let k expr = block [assignStmt (toStr expr); k (parseExpr "__sum__")] :> StatementSyntax
            let loopStmt = compileQuery query model k
            block [varDeclStmt; loopStmt] :> _
        | _ -> throwNotImplemented :> _

    let compile (query : StmtQueryExpr) (model : SemanticModel) : StatementSyntax =
        match query with
        | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
            let varDeclStmt = parseStmtf "var %s = default(%s);" identifier.ValueText (typeSymbol.ToDisplayString())
            let assignStmt value = parseStmtf "%s = %s;" identifier.ValueText value
            let loopStmt = compileQuery queryExpr model (fun expr -> assignStmt (toStr expr)) 
            block [varDeclStmt; loopStmt] :> _
