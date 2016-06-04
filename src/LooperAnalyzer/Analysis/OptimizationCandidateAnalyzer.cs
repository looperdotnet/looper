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
    /// Extracts candidates for optimization.
    /// </summary>
    internal sealed class OptimizationCandidateAnalyzer : CSharpSyntaxWalker // TODO: hide walker
    {
        SemanticModel _model;
        List<OptimizationCandidate> _candidates;
        List<InvalidMarkedNode> _falselyMarkedNodes;
        List<OptimizedNode> _optimizedNodes;

        /// <summary>
        /// Expressions or statements that can be possibly optimized.
        /// </summary>
        public IEnumerable<OptimizationCandidate> Candidates => _candidates;

        /// <summary>
        /// Get SyntaxNodes that have been marked for optimization but are not valid candidates.
        /// </summary>
        public IEnumerable<InvalidMarkedNode> FalselyMarkedNodes => _falselyMarkedNodes;

        /// <summary>
        /// Get nodes that are marked for optimization and code has been generated.
        /// </summary>
        public IEnumerable<MarkedNode> OptimizedNodes => _optimizedNodes;

        public OptimizationCandidateAnalyzer(SemanticModel model)
        {
            _model = model;
            _candidates = new List<OptimizationCandidate>();
            _falselyMarkedNodes = new List<InvalidMarkedNode>();
            _optimizedNodes = new List<OptimizedNode>();
        }

        public void Run() => Visit(_model.SyntaxTree.GetRoot());

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            // Get parent stmt to check if it's marked
            var stmt = node.GetParentStatement();
            var isMarkedForOptimization = stmt.IsMarkedWithOptimizationTrivia();

            // If method invoked is a Linq consumer method and is not marked for optimization 
            // add to candidate list, else add to marked nodes.
            // TODO: check if generated code is valid.
            var member = node.Expression as MemberAccessExpressionSyntax;
            var method = _model.GetSymbolInfo(member).Symbol as IMethodSymbol;
            if (method?.IsOptimizableConsumerMethod() == true && _model.GetTypeInfo(member.Expression).Type.IsOptimizableSourceType()) {
                if (!isMarkedForOptimization == true) {
                    _candidates.Add(OptimizationCandidate.FromInvocation(node));
                    return;
                }
                else {
                    //_markedNodes.Add();
                    return;
                }
            }

            // If method is marked but isn't valid
            if (isMarkedForOptimization) {
                var n = method.IsLinqMethod() 
                    ? InvalidMarkedNode.NoConsumer(node) 
                    : InvalidMarkedNode.InvalidLinq(node);
                _falselyMarkedNodes.Add(n);
            }

            base.VisitInvocationExpression(node);
        }
    }
}
