using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LooperAnalyzer.Analysis
{
    internal abstract class MarkedNode
    {
        public StatementSyntax Statement { get; }
        public bool IsMarkedWithDirective { get; }
        public bool IsMarkedWithComment { get; }
    }

    internal sealed class InvalidMarkedNode : MarkedNode
    {
        public bool IsValidLinq { get; private set; }
        public bool HasNoConsumer { get; private set; }
        public ExpressionSyntax Expression { get; internal set; }

        private InvalidMarkedNode() {  }

        public static InvalidMarkedNode InvalidLinq(ExpressionSyntax expr) => 
            new InvalidMarkedNode { Expression = expr, IsValidLinq = false };

        public static InvalidMarkedNode NoConsumer(ExpressionSyntax expr) => 
            new InvalidMarkedNode { Expression = expr, IsValidLinq = true };
    }

    internal sealed class OptimizedNode : MarkedNode
    {
        public StatementSyntax GeneratedStatement { get; private set; }
        public bool IsGeneratedStatementStale { get; private set; }
    }
}