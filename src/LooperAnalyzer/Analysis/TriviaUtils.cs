using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace LooperAnalyzer.Analysis
{
    internal static class TriviaUtils
    {
        public const string IFDEF_IDENTIFIER = "LOOPER";
        public const string MARKER_COMMENT = "// looper";
        
        private static readonly SyntaxTrivia _markerComment = Comment(MARKER_COMMENT);

        private static readonly SyntaxTrivia _ifDirective =
                Trivia(IfDirectiveTrivia(
                        PrefixUnaryExpression(
                            SyntaxKind.LogicalNotExpression,
                            IdentifierName(IFDEF_IDENTIFIER)),
                        true, true, true));

        private static readonly SyntaxTrivia _elseDirective = Trivia(ElseDirectiveTrivia(true, false));

        private static readonly SyntaxTrivia _endDirective = Trivia(EndIfDirectiveTrivia(true));

        public static bool IsMarkedWithOptimizationTrivia(this SyntaxNode node)
        {
            foreach (var trivia in node.GetLeadingTrivia()) {
                if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToFullString().StartsWith(MARKER_COMMENT))
                    return true;
                if(trivia.IsDirective && trivia.IsKind(SyntaxKind.IfDirectiveTrivia)) {
                    var cond = (trivia.GetStructure() as ConditionalDirectiveTriviaSyntax).Condition;
                    var nodes = cond? // TODO
                        .DescendantNodesAndSelf()
                        .OfType<PrefixUnaryExpressionSyntax>()
                        .SelectMany(n => n.DescendantNodes())
                        .OfType<IdentifierNameSyntax>();
                    if (nodes.Any(n => n.Identifier.ToFullString().StartsWith(IFDEF_IDENTIFIER)))
                        return true;
                }
            }

            return false;
        }

        public static SyntaxTriviaList MakeLeadingIfDirective(this SyntaxNode declaration)
        {
            if (declaration.GetLeadingTrivia().All(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia)))
                return TriviaList().Add(_ifDirective).Add(ElasticLineFeed).AddRange(declaration.GetLeadingTrivia());
            else
                return declaration.GetLeadingTrivia().Add(_ifDirective).Add(ElasticCarriageReturnLineFeed);
        }

        public static SyntaxTriviaList MakeLeadingElseDirective(this StatementSyntax declaration)
        {
            return TriviaList(_elseDirective, ElasticLineFeed);
        }

        public static SyntaxTriviaList MakeTrailingEndDirective(this SyntaxNode declaration)
        {
            return TriviaList(_endDirective).AddRange(declaration.GetTrailingTrivia());
        }

        public static SyntaxTriviaList MakeLeadingMarkComment(this SyntaxNode declaration)
        {
            var leadingTrivia = declaration.GetLeadingTrivia();
            var lastWhitespace = leadingTrivia.LastOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));
            return leadingTrivia = leadingTrivia
                .Add(_markerComment)
                .Add(ElasticCarriageReturnLineFeed)
                .Add(lastWhitespace);
        }

    }
}
