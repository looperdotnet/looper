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
        let block = stmt.FirstAncestorOrSelf<BlockSyntax>() // TODO
        
        let eoln = SyntaxFactory.Whitespace(Environment.NewLine)

        let ifStmt = 
            stmt.AppendLeadingTrivia(ifDirective, eoln)
                .AppendTrailingTrivia(eoln)

        let stmts = 
            let first = stmts.First().PrependLeadingTrivia(elseDirective, eoln)
            let stmts = stmts.Replace(stmts.First(), first)
            let last = stmts.Last().AppendTrailingTrivia(eoln, endDirective, eoln)
            stmts.Replace(stmts.Last(), last) :> seq<_>

        let disabled =
            stmts 
            |> Seq.map (fun s -> s.ToFullString())
            |> String.concat ""
            |> SyntaxFactory.DisabledText

        let newNode = ifStmt.AppendTrailingTrivia(disabled)

        block, block.ReplaceNode(stmt, newNode)


    let getRefactoring (root : SyntaxNode, node : InvocationExpressionSyntax) =
        match node with
        | RefactoringTransformer.CanBeRefactored root refactoring ->
            refactoring
        | _ ->
            failwith "Internal error, node cannot be refactored"

    let markWithDirective (model : SemanticModel, root : SyntaxNode, node : StatementSyntax) =
        let checker = SymbolChecker(model) // TODO
        match node with
        | StmtQueryExpr checker query ->
            let optimized = Compiler.compile query model
                            |> Formatter.format
            let oldBlock, newBlock = markStatementWithIfDirective(node, optimized)
            root.ReplaceNode(oldBlock, newBlock)
        | _ -> 
            failwith "Internal error, nodes is not a valid StmtQueryExpr"
