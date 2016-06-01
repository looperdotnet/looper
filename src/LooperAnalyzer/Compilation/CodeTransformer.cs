using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Compilation
{
    static class CodeTransformer
    {
        private const string IFDEF_IDENTIFIER = "LOOPER_OPT";

        public static SyntaxNode WrapWithIfDirective(LocalDeclarationStatementSyntax syntax)
        {
            var leadingTrivia =
                SyntaxFactory.TriviaList(
                    SyntaxFactory.Trivia(
                        SyntaxFactory.IfDirectiveTrivia(
                            SyntaxFactory.PrefixUnaryExpression(
                                SyntaxKind.LogicalNotExpression,
                                SyntaxFactory.IdentifierName(IFDEF_IDENTIFIER)),
                            true,
                            true,
                            true)),
                    SyntaxFactory.ElasticLineFeed)
                .AddRange(syntax.GetLeadingTrivia());

            var trailingTrivia = 
                SyntaxFactory.TriviaList(
                    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(true, false)),
                    SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(true)),
                    SyntaxFactory.ElasticLineFeed);

            return syntax
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia);
        }
    }
}
