module Looper.Core.Formatter

    open Looper.Core.SyntaxPatterns
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    /// Remove syntax blocks that are not needed.
    /// Assumes that the first statement produced by the compiler needs
    /// to be kept in scope for the rest of the method body.
    /// Wraps any other declarations in a block to remove them from scope.
    let format (node : StatementSyntax)  =
        let rw = 
            { new CSharpSyntaxRewriter() with
                member __.VisitBlock(block) = 
                    match base.VisitBlock(block) with
                    | Block stmts -> 
                        let newStmts = 
                            seq { 
                                for s in stmts do
                                    match s with
                                    | Block xs -> yield! xs
                                    | _ -> yield s
                            }
                        SyntaxFactory.Block(newStmts) :> _
                    | node -> node }

        match rw.Visit(node).NormalizeWhitespace() with
        | Block(List [])          -> SyntaxList()
        | Block(List [h])         -> SyntaxList().Add(h)
        | Block(List(h :: t))     -> SyntaxList().Add(h).Add(SyntaxFactory.Block(t).NormalizeWhitespace())
        | :? StatementSyntax as s -> SyntaxList().Add(s)
        | _                       -> failwith "Internal error, expected one or more statements"