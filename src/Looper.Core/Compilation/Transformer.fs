module Looper.Core.Transformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System
    open Looper.Core.SymbolUtils
    open Looper.Core.SyntaxPatterns
    open System.Diagnostics
    open Microsoft.CodeAnalysis.CSharp.CodeGeneration

    /// Refactors an expr to a variable declaration.
    let refactor (model : SemanticModel) (root : SyntaxNode) (expr : InvocationExpressionSyntax) : SyntaxNode option =
        Debug.Assert((QueryTransformer.toQueryExpr expr model).IsSome)
        
        let variable = "refactored"
        let varDeclStmt = parseStmtf "var %s = %s;" variable (toStr expr) :> SyntaxNode
        let varExpr = parseExpr variable

        // Traverse parents until we find an appropriate node where we can put an assignment
        let rec refactor (parent : SyntaxNode) : SyntaxNode option =
            match parent with
            | :? MethodDeclarationSyntax
            | WhileStatement _ -> 
                None
            | Assignment _
            | LocalDeclarationStatement _ 
            | IfStatement _ -> 
                let newParent = parent.ReplaceNode(expr, varExpr)
                root.ReplaceNode(parent,
                    [ varDeclStmt.
                        WithLeadingTrivia(parent.GetLeadingTrivia()).
                        WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed)
                      newParent ])
                |> Some
            | DoStatement(cond, Block stmts) ->
                let newCond = cond.ReplaceNode(expr, varExpr)
                let newBlock = 
                    stmts.Add((varDeclStmt :?> StatementSyntax).WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed))
                    |> block
                let newParent = 
                    SyntaxFactory.
                        DoStatement(newBlock, newCond).
                        WithTriviaFrom(parent).
                        NormalizeWhitespace()
                root.ReplaceNode(parent, newParent)
                |> Some
            | _ -> 
                refactor parent.Parent

        match expr.Parent with
        | Assignment _ 
        | LocalDeclarationStatement _ -> None
        | parent -> refactor expr.Parent