module Looper.Core.CodeTransformer 

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open QueryTransformer
    open Looper.Core.SyntaxPatterns

    let private refactorAndAnnotate(candidate: OptimizationCandidate) =
        match candidate.ContainingStatement, candidate.ContainingBlock with
        | Some oldStatement, Some containingBlock ->
            let refactoredExpression = SyntaxAnnotation("refactored-expression")
            let invocation = candidate.Invocation

            let freshVarName = Char.ToLowerInvariant(candidate.ConsumerMethodName.[0]).ToString() + candidate.ConsumerMethodName.Substring(1)
            let varStmt = SyntaxFactory.LocalDeclarationStatement(
                                SyntaxFactory.VariableDeclaration(
                                    SyntaxFactory.IdentifierName("var")).
                                    WithVariables(
                                    SyntaxFactory.SingletonSeparatedList(
                                        SyntaxFactory.VariableDeclarator(
                                            SyntaxFactory.Identifier(freshVarName)).
                                            WithInitializer(
                                            SyntaxFactory.EqualsValueClause(invocation))))).NormalizeWhitespace().
                            WithLeadingTrivia(oldStatement.GetLeadingTrivia().AddRange(invocation.GetLeadingTrivia())).WithAdditionalAnnotations(refactoredExpression)

            let newStatement = oldStatement.ReplaceNode(invocation, SyntaxFactory.IdentifierName(freshVarName))
            let stmts = [| varStmt :> SyntaxNode; newStatement :> _ |]
            let newBlock = containingBlock.ReplaceNode(oldStatement, stmts)

            let ann = newBlock.GetAnnotatedNodes(refactoredExpression) |> Seq.head
            newBlock, (ann  :?> StatementSyntax)
        | _ -> raise(NotSupportedException())


    let private refactor candidate = refactorAndAnnotate(candidate) |> fst

    let private markStatementWithComment(stmt: StatementSyntax, block: BlockSyntax) =
        let leadingTrivia = stmt.MakeLeadingMarkComment()
        let newStmt = stmt.WithLeadingTrivia(leadingTrivia)
        block.ReplaceNode(stmt, newStmt)

    let private markStatementWithIfDirective(stmt : StatementSyntax, elseStmt: SyntaxNode, block: BlockSyntax) =
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

        block.ReplaceNode(stmt, stmts)

    let markWithComment(candidate: OptimizationCandidate) =
        match candidate.ContainingStatement, candidate.ContainingBlock with
        | Some stmt, Some block -> markStatementWithComment(stmt, block)
        | _ -> raise(NotSupportedException())
        
    let refactorAndMarkWithComment(candidate: OptimizationCandidate) =
        let block, stmt = refactorAndAnnotate(candidate)
        markStatementWithComment(stmt, block)

    let markWithIfDirective(candidate: OptimizationCandidate) =
        let model = candidate.SemanticModel
        match candidate.ContainingStatement, candidate.ContainingBlock with
        | Some((StmtQueryExpr model stmtQuery) as stmt), Some block -> 
            let optimized = Compiler.compile stmtQuery candidate.SemanticModel
            markStatementWithIfDirective(stmt, optimized, block)
        | _ -> raise(NotSupportedException())

    let refactorAndMarkWithIfDirective(candidate: OptimizationCandidate) =
        let model = candidate.SemanticModel
        let block, stmt = refactorAndAnnotate(candidate)
        match stmt with
        | (StmtQueryExpr model stmtQuery) as stmt -> 
            let optimized = Compiler.compile stmtQuery candidate.SemanticModel
            markStatementWithIfDirective(stmt, optimized, block)
        | _ -> raise(NotSupportedException())
