using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Looper.Core;

namespace LooperAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MarkWithComment)), Shared]
    public class MarkWithComment : CodeFixProvider
    {
        private const string title = "Mark with comment for optimization";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(LooperDiagnosticAnalyzer.InvariantOptimizationDiagnosticId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            
            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;

            if (declaration == null) return;

            context.RegisterCodeFix(
                            CodeAction.Create(
                                title: title,
                                createChangedDocument: c => MarkWithCommentAction(context.Document, declaration, c),
                                equivalenceKey: title),
                            diagnostic);
        }

        private async Task<Document> MarkWithCommentAction(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken c)
        {
            var semanticModel = await document.GetSemanticModelAsync(c);

            var candidate = OptimizationCandidate.FromInvocation(invocationExpr);
            var newBlock = CodeTransformer.markWithComment(candidate);

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(c);
            var newRoot = oldRoot.ReplaceNode(candidate.ContainingBlock.Value, newBlock);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);
        }
    }
}