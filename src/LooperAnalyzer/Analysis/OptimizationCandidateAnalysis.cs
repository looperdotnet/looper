using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Analysis
{
    /// <summary>
    ///     Extracts candidates for optimization.
    /// </summary>
    internal sealed class OptimizationCandidateAnalysis : CSharpSyntaxWalker
    {
        SemanticModel _model;
        List<OptimizationCandidate> _nodes;

        private IEnumerable<OptimizationCandidate> Candidates => _nodes;

        private OptimizationCandidateAnalysis(SemanticModel model)
        {
            _model = model;
            _nodes = new List<OptimizationCandidate>();
        }

        public static IEnumerable<OptimizationCandidate> GetCandidates(SemanticModel model)
        {
            var v = new OptimizationCandidateAnalysis(model);
            v.Visit(model.SyntaxTree.GetRoot());
            var candidates = v.Candidates;

            return v.Candidates;
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            var method = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (method?.IsOptimizableConsumerMethod() == true) {

                var typ = _model.GetTypeInfo(node.Expression).Type;
                var invocation = node.Parent as InvocationExpressionSyntax;

                if (typ.IsOptimizableSourceType() && invocation != null)
                    _nodes.Add(OptimizationCandidate.FromInvocation(invocation));
            }
            base.VisitMemberAccessExpression(node); // What to do with nested?
        }
    }
}
