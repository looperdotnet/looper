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

    sealed class OptimizationCandidateAnalysis : CSharpSyntaxWalker
    {
        SemanticModel _model;
        List<InvocationExpressionSyntax> _nodes;

        private IEnumerable<InvocationExpressionSyntax> Candidates => _nodes;

        private OptimizationCandidateAnalysis(SemanticModel model)
        {
            _model = model;
            _nodes = new List<InvocationExpressionSyntax>();
        }

        public static IEnumerable<InvocationExpressionSyntax> GetCandidates(SemanticModel model)
        {
            var v = new OptimizationCandidateAnalysis(model);
            v.Visit(model.SyntaxTree.GetRoot());
            var candidates = v.Candidates
                .Where(c => !c.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>().IsMarkedWithOptimizationTrivia());

            return candidates;
        }

        private bool IsValidPattern(MemberAccessExpressionSyntax node)
        {
            var method = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;
            if (method == null) return false;

            var typ = _model.GetTypeInfo(node.Expression).Type;

            return typ.IsOptimizableSourceType() && method.IsOptimizableConsumerMethod();
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (IsValidPattern(node) && node.Parent is InvocationExpressionSyntax)
                _nodes.Add(node.Parent as InvocationExpressionSyntax);
            base.VisitMemberAccessExpression(node);
        }
    }
}
