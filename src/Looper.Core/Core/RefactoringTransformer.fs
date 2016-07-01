module Looper.Core.RefactoringTransformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SyntaxPatterns
    open System.Diagnostics
    open Microsoft.CodeAnalysis.CSharp.CodeGeneration

    type Refactoring = {
        NewRoot : SyntaxNode
        RefactoredStatement : StatementSyntax
    }

    /// Refactors an expr to a variable declaration.
    let refactor (root : SyntaxNode) (model : SemanticModel) (expr : SyntaxNode) : Refactoring option =
        let gen = FreshNameGen(model, expr.SpanStart)
        
        let variable = 
            match expr with
            | InvocationExpression(MemberAccessExpression (name, _), _) -> 
                NameUtils.toCameCase(string name)
            | _ -> "temp"
            |> gen.Generate

        // Traverse parents until we find an appropriate node where we can put an assignment
        let rec refactor (parent : SyntaxNode) =
            match parent with
            | WhileStatement _ -> 
                None
            | ExpressionStatement(Assignment  _)
            | LocalDeclarationStatement _ 
            | IfStatement _ -> 
                let varDeclStmt = parseStmtf "var %s = %s;" variable (toStr expr) :> StatementSyntax
                let varExpr = parseExpr variable
                let newParent = parent.ReplaceNode(expr, varExpr)
                let newRoot = root.ReplaceNode(parent,
                                [ varDeclStmt.
                                    WithLeadingTrivia(parent.GetLeadingTrivia()).
                                    WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed) :> SyntaxNode
                                  newParent ])
                Some { NewRoot = newRoot; RefactoredStatement = varDeclStmt }
//            | DoStatement(cond, Block stmts) ->
//                let varInitStmt = parseStmtf "var %s = %s;" variable (toStr (defaultOf ))
//                let varAssignStmt = parseStmtf "%s = %s;" variable (toStr expr) :> StatementSyntax
//                let varExpr = parseExpr variable
//
//                let newCond = cond.ReplaceNode(expr, varExpr)
//                let newBlock = 
//                    stmts.Add(varAssignStmt.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed))
//                    |> block
//                let newParent = 
//                    SyntaxFactory.
//                        DoStatement(newBlock, newCond).
//                        WithTriviaFrom(parent).
//                        NormalizeWhitespace()
//                let newRoot = root.ReplaceNode(parent, newParent)
//                Some { NewRoot = newRoot; RefactoredStatement = varAssignStmt }
            | current when current = root -> 
                None
            | _ ->
                refactor parent.Parent

        match expr.Parent with
        | Assignment _ 
        | LocalDeclarationStatement _ -> None
        | parent -> refactor parent

    let (|CanBeRefactored|_|) root model expr = refactor root model expr 