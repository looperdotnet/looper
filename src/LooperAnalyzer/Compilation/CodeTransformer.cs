using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using LooperAnalyzer.Analysis;

namespace LooperAnalyzer.Compilation
{
    static class CodeTransformer
    {
        private static readonly ThrowStatementSyntax _throwNotImplemented =
            ThrowStatement(
                    ObjectCreationExpression(
                        QualifiedName(
                            IdentifierName("System"),
                            IdentifierName("NotImplementedException")))
                    .WithArgumentList(
                        ArgumentList()));

        public static BlockSyntax ReplaceWithIfDirective(BlockSyntax block, StatementSyntax declaration)
        {
            var ifDef = declaration.GetLeadingIfDirective();
            var elseDef = declaration.GetLeadingElseDirective();
            var endDef = declaration.GetTrailingEndDirective();

            var newDeclaration = declaration.WithLeadingTrivia(ifDef);
            var throwSyntax = _throwNotImplemented
                .WithLeadingTrivia(elseDef)
                .WithTrailingTrivia(endDef);

            var newBlock = block.ReplaceNode(declaration, new SyntaxNode[] {
                newDeclaration,
                throwSyntax
            });

            return newBlock;
        }

        public static BlockSyntax MarkWithComment(BlockSyntax block, StatementSyntax declaration)
        {
            var leadingTrivia = declaration.GetLeadingMarkComment();
            var newDeclaration = declaration.WithLeadingTrivia(leadingTrivia);
            return block.ReplaceNode(declaration, newDeclaration);
        }
    }
}
