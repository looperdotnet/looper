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

    let private markStatementWithIfDirective(stmt : StatementSyntax, stmts: SyntaxList<StatementSyntax>) =
        let block   = stmt.FirstAncestorOrSelf<BlockSyntax>() // TODO
        let ifDef   = stmt.MakeLeadingIfDirective()
        let elseDef = stmt.MakeLeadingElseDirective()
        let endDef  = stmt.MakeTrailingEndDirective()
        let ifStmt  = stmt.WithLeadingTrivia(ifDef) 

        let stmts = 
            seq { 
                yield ifStmt
                let first = stmts.First().WithLeadingTrivia(elseDef)
                let stmts = stmts.Replace(stmts.First(), first)
                let last = 
                    stmts.Last().WithTrailingTrivia(
                        SyntaxTriviaList.
                            Create(SyntaxFactory.ElasticCarriageReturnLineFeed).
                            AddRange(endDef))
                yield! stmts.Replace(stmts.Last(), last) :> seq<_>
            } |> Seq.cast<SyntaxNode>

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
                            |> Formatter.format
            let oldBlock, newBlock = markStatementWithIfDirective(node, optimized)
            root.ReplaceNode(oldBlock, newBlock)
        | _ -> 
            failwith "Internal error, nodes is not a valid StmtQueryExpr"
