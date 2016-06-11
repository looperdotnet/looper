namespace Looper.Core

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System.Collections.Generic
open Looper.Core.SyntaxPatterns
open Looper.Core.QueryTransformer
open System.Linq

module Analyzer =
    
    type AnalysisResult = {
        OptimizationCandidates: seq<OptimizationCandidate>
        InvalidMarkedNodes: seq<InvalidNode>
        OptimizedNodes: seq<OptimizedNode>
    }

    type private Walker (model: SemanticModel) =
        inherit CSharpSyntaxWalker ()

        let candidates = new List<OptimizationCandidate>()
        let invalidNodes = new List<InvalidNode>()
        let optimizedNodes = new List<OptimizedNode>() // TODO
        let invalidTrivia = new HashSet<SyntaxTrivia>()

        member __.Results = { OptimizationCandidates = candidates; InvalidMarkedNodes = invalidNodes; OptimizedNodes = optimizedNodes}

        override __.Visit(node) =
            let triviaMark = 
                match node with
                | MarkedForOptimization trivia -> Some trivia
                | _ -> None
            
            match node with 
            | StmtQueryExpr model _ 
            | QueryExpr model _ ->
                if triviaMark.IsNone then
                    let inv = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First()
                    candidates.Add(OptimizationCandidate.FromInvocation(inv)) // TODO
            | StmtNoConsumerQuery model _ 
            | NoConsumerQuery model _ when triviaMark.IsSome ->
                let inv = node.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>().First()
                invalidNodes.Add(NoConsumer(inv))
            | _ ->
                match triviaMark with
                | Some t ->
                    if invalidTrivia.Add(t) then
                        invalidNodes.Add(InvalidExpression(node, t))
                | _ -> ()
                base.Visit(node)

    let analyze (model : SemanticModel) =
        let walker = Walker(model)
        walker.Visit(model.SyntaxTree.GetRoot())
        walker.Results