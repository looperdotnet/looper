module Looper.Core.CodeTransformer 

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System

let private throwNotImplemented =
    SyntaxFactory.ThrowStatement(
        SyntaxFactory.ObjectCreationExpression(
            SyntaxFactory.QualifiedName(SyntaxFactory.IdentifierName("System"),SyntaxFactory.IdentifierName("NotImplementedException"))).WithArgumentList(SyntaxFactory.ArgumentList()))

let refactoredExpression = SyntaxAnnotation("refactored-expression")

let private refactorAndAnnotate(candidate: OptimizationCandidate) =
    match candidate.ContainingStatement, candidate.ContainingBlock with
    | Some oldStatement, Some containingBlock ->
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
    | _ -> failwith "Not supported"


let refactor candidate = refactorAndAnnotate(candidate) |> fst

let private markStatementWithComment(stmt: StatementSyntax, block: BlockSyntax) =
    let leadingTrivia = stmt.MakeLeadingMarkComment()
    let newStmt = stmt.WithLeadingTrivia(leadingTrivia)
    block.ReplaceNode(stmt, newStmt)

let markWithComment(candidate: OptimizationCandidate) =
    match candidate.ContainingStatement, candidate.ContainingBlock with
    | Some stmt, Some block -> markStatementWithComment(stmt, block)
    | _ -> failwith "Not supported"
        
let refactorAndMarkWithComment(candidate: OptimizationCandidate) =
    let block, stmt = refactorAndAnnotate(candidate)
    markStatementWithComment(stmt, block)

let markStatementWithIfDirective(stmt : StatementSyntax, elseStmt: StatementSyntax, block: BlockSyntax) =
    let ifDef   = stmt.MakeLeadingIfDirective()
    let elseDef = stmt.MakeLeadingElseDirective()
    let endDef  = stmt.MakeTrailingEndDirective()
    let ifStmt  = stmt.WithLeadingTrivia(ifDef)

    let elseStmt = elseStmt.WithLeadingTrivia(elseDef).WithTrailingTrivia(endDef)

    block.ReplaceNode(stmt, [| ifStmt :> SyntaxNode; elseStmt :> _ |])

let markWithIfDirective(candidate: OptimizationCandidate) =
    match candidate.ContainingStatement, candidate.ContainingBlock with
    | Some stmt, Some block -> markStatementWithIfDirective(stmt, throwNotImplemented, block)
    | _ -> failwith "Not supported"

let refactorAndMarkWithIfDirective(candidate: OptimizationCandidate) =
    let block, stmt = refactorAndAnnotate(candidate)
    markStatementWithIfDirective(stmt, throwNotImplemented, block)
