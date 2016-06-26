module Looper.Core.Formatter

    open Looper.Core.SyntaxPatterns
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    type private Rewriter () =
        inherit CSharpSyntaxRewriter()

        override this.VisitBlock(block) =
            match base.VisitBlock(block) with
            | Block stmts ->
                let newStmts = seq {
                    for s in stmts do
                        match s with
                        | Block xs -> yield! xs
                        | _ -> yield s
                }
                SyntaxFactory.Block(newStmts) :> _
            | node -> node 

    let format (node : SyntaxNode)  =
        let rw = new Rewriter()
        match rw.Visit(node).NormalizeWhitespace() with
        | Block stmts -> stmts
        | :? StatementSyntax as s -> SyntaxList().Add(s)
        | _ -> failwith "Internal error, expected one or more statements"