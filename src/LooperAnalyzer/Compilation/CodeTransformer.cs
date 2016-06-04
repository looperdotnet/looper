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
    internal static class CodeTransformer
    {
        private static readonly ThrowStatementSyntax _throwNotImplemented =
            ThrowStatement(
                    ObjectCreationExpression(
                        QualifiedName(IdentifierName("System"),IdentifierName("NotImplementedException")))
                    .WithArgumentList(ArgumentList()));

        private static readonly SyntaxAnnotation _refactoredExpression = new SyntaxAnnotation("refactored-expression");

        internal static Tuple<BlockSyntax, StatementSyntax> RefactorAndAnnotate(OptimizationCandidate candidate)
        {
            var oldStatement = candidate.ContainingStatement;
            var invocation = candidate.Invocation;

            var freshVarName = char.ToLowerInvariant(candidate.ConsumerMethodName[0]) + candidate.ConsumerMethodName.Substring(1);
            var varStmt = LocalDeclarationStatement(
                                VariableDeclaration(
                                    IdentifierName("var"))
                                .WithVariables(
                                    SingletonSeparatedList(
                                        VariableDeclarator(
                                            Identifier(freshVarName))
                                        .WithInitializer(
                                            EqualsValueClause(invocation)))))
                        .NormalizeWhitespace()
                        .WithLeadingTrivia(
                            oldStatement.GetLeadingTrivia()
                            .AddRange(invocation.GetLeadingTrivia()))
                        .WithAdditionalAnnotations(_refactoredExpression) as StatementSyntax;

            var newStatement = oldStatement.ReplaceNode(invocation, IdentifierName(freshVarName));

            var newBlock = candidate.ContainingBlock.ReplaceNode(oldStatement, new [] { varStmt, newStatement });

            return Tuple.Create(newBlock, newBlock.GetAnnotatedNodes(_refactoredExpression).Single() as StatementSyntax);
        }

        internal static BlockSyntax Refactor(OptimizationCandidate candidate) => RefactorAndAnnotate(candidate).Item1;

        private static BlockSyntax MarkWithComment(StatementSyntax stmt, BlockSyntax block)
        {
            var leadingTrivia = stmt.MakeLeadingMarkComment();
            var newStmt = stmt.WithLeadingTrivia(leadingTrivia);
            return block.ReplaceNode(stmt, newStmt);
        }

        public static BlockSyntax MarkWithComment(OptimizationCandidate candidate) => 
            MarkWithComment(candidate.ContainingStatement, candidate.ContainingBlock);
        
        internal static BlockSyntax RefactorAndMarkWithComment(OptimizationCandidate candidate)
        {
            var refactored = RefactorAndAnnotate(candidate);
            return MarkWithComment(refactored.Item2, refactored.Item1);
        }

        private static BlockSyntax MarkWithIfDirective(StatementSyntax stmt, StatementSyntax elseStmt, BlockSyntax block)
        {
            var ifDef   = stmt.MakeLeadingIfDirective();
            var elseDef = stmt.MakeLeadingElseDirective();
            var endDef  = stmt.MakeTrailingEndDirective();
            var ifStmt  = stmt.WithLeadingTrivia(ifDef);

            elseStmt = elseStmt
                .WithLeadingTrivia(elseDef)
                .WithTrailingTrivia(endDef);

            return block.ReplaceNode(stmt, new [] { ifStmt, elseStmt });
        }

        public static BlockSyntax MarkWithIfDirective(OptimizationCandidate candidate) => 
            MarkWithIfDirective(candidate.ContainingStatement, _throwNotImplemented, candidate.ContainingBlock);
        

        internal static BlockSyntax RefactorAndMarkWithIfDirective(OptimizationCandidate candidate)
        {
            var refactored = RefactorAndAnnotate(candidate);
            return MarkWithIfDirective(refactored.Item2, _throwNotImplemented, refactored.Item1);
        }
    }
}
