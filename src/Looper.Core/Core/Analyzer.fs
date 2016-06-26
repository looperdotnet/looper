module Looper.Core.Analyzer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open System.Collections.Generic
    open Looper.Core.SyntaxPatterns
    open Looper.Core.QueryTransformer
    open Looper.Core.RefactoringTransformer
    open System.Linq


    type AnalyzedNode =
    // A node marked with Looper trivia but not a valid Linq expression.
    | Invalid of node: SyntaxNode * trivia: SyntaxTrivia
    // A node marked with Looper trivia, that is a valid Linq expression but has no consumer.
    | NoConsumer of stmt: SyntaxNode
    // A valid Looper expr marked with Looper conditional trivia.
    | MarkedWithDirective of stmt: StatementSyntax * generated: SyntaxList<StatementSyntax> * isStale: bool
    // A valid Looper expr that needs refactoring before optimization.
    | NeedsRefactoring of node: InvocationExpressionSyntax
    // A valid Looper expr that can be optimized.
    | Optimizable of node: SyntaxNode

    type private Walker (model: SemanticModel) =
        inherit CSharpSyntaxWalker ()

        let nodes = new List<AnalyzedNode>()
        let invalidTrivia = new HashSet<SyntaxTrivia>()
        let checker = SymbolChecker(model)

        member __.Results = nodes

        override __.Visit(node) =
            let triviaMark = 
                match node with
                | MarkedForOptimization trivia -> Some trivia
                | _ -> None
            
            match node with 
            | StmtQueryExpr checker _  ->
                if triviaMark.IsNone then
                    nodes.Add(Optimizable node)
            | StmtNoConsumerQuery checker _ when triviaMark.IsSome ->
                nodes.Add(NoConsumer node)
            | QueryExpr checker _ when Option.isSome(refactor (model.SyntaxTree.GetRoot()) model node) ->
                nodes.Add(NeedsRefactoring(node :?> InvocationExpressionSyntax))
            | _ ->
                match triviaMark with
                | Some t ->
                    if invalidTrivia.Add(t) then
                        nodes.Add(Invalid(node, t))
                | _ -> ()
                base.Visit(node)

    let analyze (model : SemanticModel) : AnalyzedNode seq =
        let walker = Walker(model)
        walker.Visit(model.SyntaxTree.GetRoot())
        walker.Results :> _