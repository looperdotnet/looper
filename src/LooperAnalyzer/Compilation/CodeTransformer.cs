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

        public static BlockSyntax MarkWithComment(OptimizationCandidate candidate)
        {
            var block = candidate.ContainingBlock;
            var declaration = candidate.ContainingStatement;
            var leadingTrivia = declaration.GetLeadingMarkComment();
            var newDeclaration = declaration.WithLeadingTrivia(leadingTrivia);
            return block.ReplaceNode(declaration, newDeclaration);
        }

        internal static BlockSyntax RefactorAndMarkWithComment(OptimizationCandidate candidate)
        {
            var invAnn = new SyntaxAnnotation("invocation");
            var annotatedInvocation = candidate.Invocation
                .WithAdditionalAnnotations(invAnn);

            var oldStmtAnn = new SyntaxAnnotation("old-stmt");
            var annotatedStatement = candidate.ContainingStatement
                .ReplaceNode(candidate.Invocation, annotatedInvocation)
                .WithAdditionalAnnotations(oldStmtAnn);

            var block = candidate.ContainingBlock
                .ReplaceNode(candidate.ContainingStatement, annotatedStatement);
            annotatedStatement = block.GetAnnotatedNodes(oldStmtAnn).Single() as StatementSyntax;

            var freshVarName = char.ToLowerInvariant(candidate.ConsumerMethodName[0]) + candidate.ConsumerMethodName.Substring(1);
            var varStmt = LocalDeclarationStatement(
                                VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(freshVarName))
                                        .WithInitializer(
                                            EqualsValueClause(candidate.Invocation)))))
                        .NormalizeWhitespace()
                        .WithLeadingTrivia(
                            annotatedStatement.GetLeadingTrivia()
                            .AddRange(annotatedInvocation.GetLeadingTrivia()));

            varStmt = varStmt.WithLeadingTrivia(varStmt.GetLeadingMarkComment());

            var newBlock = block.InsertNodesBefore(annotatedStatement, new[] { varStmt });

            annotatedInvocation = newBlock.GetAnnotatedNodes(invAnn).Single() as InvocationExpressionSyntax;

            return newBlock.ReplaceNode(annotatedInvocation, IdentifierName(freshVarName));

        }

        public static BlockSyntax ReplaceWithIfDirective(OptimizationCandidate candidate)
        {
            var block = candidate.ContainingBlock;
            var declaration = candidate.ContainingStatement;
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

        internal static BlockSyntax RefactorAndReplaceWithIfDirective(OptimizationCandidate candidate)
        {
            var invAnn = new SyntaxAnnotation("invocation");
            var annotatedInvocation = candidate.Invocation
                .WithAdditionalAnnotations(invAnn);

            var oldStmtAnn = new SyntaxAnnotation("old-stmt");
            var annotatedStatement = candidate.ContainingStatement
                .ReplaceNode(candidate.Invocation, annotatedInvocation)
                .WithAdditionalAnnotations(oldStmtAnn);

            var block = candidate.ContainingBlock
                .ReplaceNode(candidate.ContainingStatement, annotatedStatement);
            annotatedStatement = block.GetAnnotatedNodes(oldStmtAnn).Single() as StatementSyntax;

            var freshVarName = char.ToLowerInvariant(candidate.ConsumerMethodName[0]) + candidate.ConsumerMethodName.Substring(1);
            var varStmt = LocalDeclarationStatement(
                                VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(freshVarName))
                                        .WithInitializer(
                                            EqualsValueClause(candidate.Invocation)))))
                        .NormalizeWhitespace()
                        .WithLeadingTrivia(
                            annotatedStatement.GetLeadingTrivia()
                            .AddRange(annotatedInvocation.GetLeadingTrivia()));

            var ifDef = varStmt.GetLeadingIfDirective();
            var elseDef = varStmt.GetLeadingElseDirective();
            var endDef = varStmt.GetTrailingEndDirective();

            varStmt = varStmt.WithLeadingTrivia(ifDef);
            var throwSyntax = _throwNotImplemented
                .WithLeadingTrivia(elseDef)
                .WithTrailingTrivia(endDef);


            var newBlock = block.InsertNodesBefore(annotatedStatement, new StatementSyntax[] {
                varStmt,
                throwSyntax
            });

            annotatedInvocation = newBlock.GetAnnotatedNodes(invAnn).Single() as InvocationExpressionSyntax;

            return newBlock.ReplaceNode(annotatedInvocation, IdentifierName(freshVarName));
        }
    }
}
