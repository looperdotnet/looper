module Looper.Core.RefactoringTransformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SymbolUtils
    open Looper.Core.SyntaxPatterns
    open System.Diagnostics
    open Microsoft.CodeAnalysis.CSharp.CodeGeneration

    type Refactoring = {
        NewRoot : SyntaxNode
        RefactoredStatement : StatementSyntax
    }

    /// Refactors an expr to a variable declaration.
    let refactor (model : SemanticModel) (root : SyntaxNode) (expr : SyntaxNode) : Refactoring option =
        Debug.Assert((QueryTransformer.toQueryExpr expr model).IsSome)

        let variable = "refactored"
        let varDeclStmt = parseStmtf "var %s = %s;" variable (toStr expr) :> StatementSyntax
        let varExpr = parseExpr variable

        // Traverse parents until we find an appropriate node where we can put an assignment
        let rec refactor (parent : SyntaxNode) =
            match parent with
            | WhileStatement _ -> 
                None
            | Assignment _
            | LocalDeclarationStatement _ 
            | IfStatement _ -> 
                let newParent = parent.ReplaceNode(expr, varExpr)
                let newRoot = root.ReplaceNode(parent,
                                [ varDeclStmt.
                                    WithLeadingTrivia(parent.GetLeadingTrivia()).
                                    WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed) :> SyntaxNode
                                  newParent ])
                Some { NewRoot = newRoot; RefactoredStatement = varDeclStmt }
            | DoStatement(cond, Block stmts) ->
                let newCond = cond.ReplaceNode(expr, varExpr)
                let newBlock = 
                    stmts.Add(varDeclStmt.WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed))
                    |> block
                let newParent = 
                    SyntaxFactory.
                        DoStatement(newBlock, newCond).
                        WithTriviaFrom(parent).
                        NormalizeWhitespace()
                let newRoot = root.ReplaceNode(parent, newParent)
                Some { NewRoot = newRoot; RefactoredStatement = varDeclStmt }
            | current when current = root -> 
                None
            | _ ->
                refactor parent.Parent

        match expr.Parent with
        | Assignment _ 
        | LocalDeclarationStatement _ -> None
        | parent -> refactor parent

    let (|CanBeRefactored|_|) model root expr = refactor model root expr 