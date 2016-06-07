namespace Looper.Core

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System.Collections.Generic

type OptimizationCandidateAnalyzer private (model: SemanticModel, candidates: List<OptimizationCandidate>, invalidNodes: List<InvalidNode>, optimizedNodes: List<OptimizedNode>) =
    inherit CSharpSyntaxWalker()

    member __.Candidates with get () = candidates :> seq<_>
    member __.FalselyMarkedNodes with get () = invalidNodes :> seq<_>
    member __.OptimizedNode with get () = optimizedNodes :> seq<_> 

    new(model: SemanticModel) = OptimizationCandidateAnalyzer(model, new List<_>(), new List<_>(), new List<_>())

    member __.Run () = __.Visit(model.SyntaxTree.GetRoot())

    override __.VisitInvocationExpression(node) =
        let stmt = node.GetParentStatement()
        let isMarked = stmt |> Option.exists(fun s -> s.IsMarkedWithOptimizationTrivia)

        match node.Expression with
        | :? MemberAccessExpressionSyntax as memberExpr ->
            let methodSym = model.GetSymbolInfo(memberExpr).Symbol :?> IMethodSymbol // TODO
            if methodSym.IsOptimizableConsumerMethod && model.GetTypeInfo(memberExpr.Expression).Type.IsOptimizableSourceType then
                if not isMarked then
                    candidates.Add(OptimizationCandidate.FromInvocation(node))
                    ()
                else
                    ()
            elif isMarked then
                let n = if methodSym.IsLinqMethod then NoConsumer(node) else InvalidExpression(node)
                invalidNodes.Add(n)
        | _ -> base.VisitInvocationExpression(node)