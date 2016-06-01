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

    class OptimizationCandidateVisitor : CSharpSyntaxWalker
    {
        SemanticModel _model;
        List<InvocationExpressionSyntax> _nodes;

        public IEnumerable<InvocationExpressionSyntax> Candidates => _nodes;

        public OptimizationCandidateVisitor(SemanticModel model)
        {
            _model = model;
            _nodes = new List<InvocationExpressionSyntax>();
        }

        private bool IsValidPattern(MemberAccessExpressionSyntax node)
        {
            var typ = _model.GetTypeInfo(node.Expression).Type;
            if (!typ.IsOptimizableSourceType())
                return false;

            var members = _model
                .Compilation.GetTypeByMetadataName("System.Linq.Enumerable")
                .GetMembers().Where(s => s.Name == "Sum" || s.Name == "First");

            var method = _model.GetSymbolInfo(node).Symbol as IMethodSymbol;

            return members.Contains(method?.ReducedFrom);
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (IsValidPattern(node) && node.Parent is InvocationExpressionSyntax)
                _nodes.Add(node.Parent as InvocationExpressionSyntax);
            base.VisitMemberAccessExpression(node);
        }
    }
}
