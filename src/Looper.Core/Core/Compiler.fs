module Looper.Core.Compiler

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SyntaxPatterns


    let rec compileQuery (query : QueryExpr) (gen : FreshNameGen) (model : SemanticModel) (k : ExpressionSyntax -> StatementSyntax) : StatementSyntax =
        
        match query with
        | SourceExpression(sym, expr) ->
            let item = gen.Generate "item"
            let foreachStmt = parseForeach item (toStr expr)
            let stmt = k (parseExpr item)
            foreachStmt.WithStatement(block [stmt]) :> _
        | SourceIdentifierName (ArrayType arrayTypeSymbol, identifier) ->  
            let index = gen.Generate "i"  
            let identifier = identifier.Identifier.ValueText
            let forStmt = parseFor index identifier
            let stmt = k (parseIndexer identifier index)
            forStmt.WithStatement(block [stmt]) :> _
        | SourceIdentifierName (_, identifier) ->
            let item = gen.Generate "item"
            let foreachStmt = parseForeach item identifier.Identifier.ValueText
            let stmt = k (parseExpr item)
            foreachStmt.WithStatement(block [stmt]) :> _
        | Select (Lambda (param, Expression body), query) ->
            let k expr = 
                let identifier, body = gen.Replace(param, body)
                block [parseStmtf "var %s = %s;" identifier (toStr expr)
                       k body] :> StatementSyntax
            compileQuery query gen model k
        | SelectMany ((param, nestedQuery), query) ->
            let k expr = 
                let identifier = gen.Generate param.Identifier.ValueText
                block [parseStmtf "var %s = %s;" identifier (toStr expr)
                       compileQuery nestedQuery gen model k] :> StatementSyntax
            compileQuery query gen model k
        | Where (Lambda (param, Expression body), query) ->
            let k expr = 
                let identifier, body = gen.Replace(param, body)
                block [parseStmtf "var %s = %s;" identifier (toStr expr)
                       parseIf (toStr body) (toStr (k expr))] :> StatementSyntax 
            compileQuery query gen model k
        | Sum query -> 
            let sum = gen.Generate "sum"
            let varDeclStmt = parseStmtf "var %s = 0;" sum // TODO one needs type info for different overloads
            let assignStmt value = parseStmtf "%s += %s;" sum value
            let k expr = block [assignStmt (toStr expr); k (parseExpr sum)] :> StatementSyntax
            let loopStmt = compileQuery query gen model k
            block [varDeclStmt; loopStmt] :> _
        | Count query -> 
            let count = gen.Generate "count"
            let varDeclStmt = parseStmtf "var %s = 0;" count
            let assignStmt = parseStmtf "++%s;" count
            let k _ = block [assignStmt; k (parseExpr count)] :> StatementSyntax
            let loopStmt = compileQuery query gen model k
            block [varDeclStmt; loopStmt] :> _
        | Any query -> 
            let any = gen.Generate "any"
            let varDeclStmt = parseStmtf "var %s = false;" any
            let k _ = 
                block [ parseStmtf "%s = true;" any
                        k (parseExpr any)
                        breakStmt() ] :> StatementSyntax
            let loopStmt = compileQuery query gen model k
            block [varDeclStmt; loopStmt] :> _
        | _ -> throwNotImplemented :> _

    let compile (query : StmtQueryExpr) (model : SemanticModel) : StatementSyntax =
        match query with
        | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
            let gen = FreshNameGen(model, identifier.SpanStart)
            let varDeclStmt = parseStmtf "var %s = %s;" identifier.ValueText (toStr (defaultOf (typeSymbol.ToDisplayString())))
            let assignStmt value = parseStmtf "%s = %s;" identifier.ValueText value
            let loopStmt = compileQuery queryExpr gen model (assignStmt << toStr) 
            block [varDeclStmt; loopStmt] :> _
