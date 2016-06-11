namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System

    module Compiler = 
        
        let rec compileQuery (query : QueryExpr) (model : SemanticModel) (k : SyntaxNode -> SyntaxNode) : SyntaxNode =
            failwith "oups"

        let compile (query : StmtQueryExpr) (model : SemanticModel) : SyntaxNode =
            match query with
            | Assign (typeSyntax, typeSymbol, identifier, queryExpr) -> 
                //let symbolInfo = model.GetSymbolInfo(identifier)
                //SyntaxFactory.VariableDeclaration()
                failwith "oups"
            | _ -> failwithf "Invalid query %A" query

