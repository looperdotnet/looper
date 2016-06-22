module Looper.Core.CodeTransformer 

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open QueryTransformer
    open Looper.Core.SyntaxPatterns

    let private markStatementWithComment(stmt: StatementSyntax) =
        let block   = stmt.FirstAncestorOrSelf<BlockSyntax>() // TODO
        let leadingTrivia = stmt.MakeLeadingMarkComment()
        let newStmt = stmt.WithLeadingTrivia(leadingTrivia)
        block, block.ReplaceNode(stmt, newStmt)

    let private markStatementWithIfDirective(stmt : StatementSyntax, elseStmt: SyntaxNode) =
        let block   = stmt.FirstAncestorOrSelf<BlockSyntax>() // TODO
        let ifDef   = stmt.MakeLeadingIfDirective()
        let elseDef = stmt.MakeLeadingElseDirective()
        let endDef  = stmt.MakeTrailingEndDirective()
        let ifStmt  = stmt.WithLeadingTrivia(ifDef) 

        let stmts = 
            seq { 
                yield ifStmt
                match elseStmt with
                | Block stmts -> 
                    let first = stmts.First().WithLeadingTrivia(elseDef)
                    let stmts = stmts.Replace(stmts.First(), first)
                    let last = stmts.Last().WithTrailingTrivia(endDef)
                    yield! stmts.Replace(stmts.Last(), last) :> seq<_>
                | :? StatementSyntax as s -> yield! Seq.singleton s
                | _ -> failwith "Internal error, expected one or more statements"
            }
            |> Seq.cast<SyntaxNode>

        block, block.ReplaceNode(stmt, stmts)


    let getRefactoring (model : SemanticModel, root : SyntaxNode, node : InvocationExpressionSyntax) =
        match node with
        | RefactoringTransformer.CanBeRefactored model root refactoring ->
            refactoring
        | _ ->
            failwith "Internal error, node cannot be refactored"

    let markWithDirective (model : SemanticModel, root : SyntaxNode, node : StatementSyntax) =
        match node with
        | StmtQueryExpr model query ->
            let optimized = Compiler.compile query model
            let oldBlock, newBlock = markStatementWithIfDirective(node, optimized)
            root.ReplaceNode(oldBlock, newBlock)
        | _ -> 
            failwith "Internal error, nodes is not a valid StmtQueryExpr"
