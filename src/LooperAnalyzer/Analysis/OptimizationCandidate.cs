﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

namespace LooperAnalyzer.Analysis
{
    internal sealed class OptimizationCandidate
    {
        public InvocationExpressionSyntax Invocation { get; private set; }
        public string ConsumerMethodName { get; private set; }
        public StatementSyntax ContainingStatement { get; private set; }
        public BlockSyntax ContainingBlock { get; private set; }
        public bool IsInvariantOptimization { get; private set; }
        public bool NeedsRefactoring { get; private set; }
        public bool IsMarkedWithOptimizationTrivia { get; private set; }

        private OptimizationCandidate() { }

        public static OptimizationCandidate FromInvocation(InvocationExpressionSyntax node)
        {
            var memberExpr = node?.Expression as MemberAccessExpressionSyntax;

            Debug.Assert(memberExpr != null);

            var consumerMethod = memberExpr.Name.ToString();

            var parentExpr = node
                .Ancestors()
                .TakeWhile(n => !(n is BlockSyntax))
                .FirstOrDefault(n => n is ExpressionSyntax);
            var parentStmt = node.GetParentStatement();

            var block = node.FirstAncestorOrSelf<BlockSyntax>();

            return new OptimizationCandidate
            {
                Invocation = node,
                ConsumerMethodName = consumerMethod,
                ContainingStatement = parentStmt,
                ContainingBlock = block,
                IsInvariantOptimization = parentExpr == null,
                NeedsRefactoring = !(parentStmt is LocalDeclarationStatementSyntax), // TODO
                IsMarkedWithOptimizationTrivia = parentStmt?.IsMarkedWithOptimizationTrivia() == true
            };
        }
    }
}