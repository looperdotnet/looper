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
using LooperAnalyzer.Compilation;
using LooperAnalyzer.Analysis;

namespace LooperAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceWithIfDirective)), Shared]
    public class ReplaceWithIfDirective : CodeFixProvider
    {
        private const string title = "Replace with conditional optimization";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(
                LooperAnalyzerAnalyzer.InvariantOptimizationDiagnosticId,
                LooperAnalyzerAnalyzer.UnsafeOptimizationDiagnosticId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithIfDirectiveAction(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);

        }

        private async Task<Document> ReplaceWithIfDirectiveAction(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken c)
        {
            // Find a way to pass the fix from the analyzer to code fox provider
            var semanticModel = await document.GetSemanticModelAsync(c);

            var candidate = OptimizationCandidate.FromInvocation(invocationExpr);
            var newBlock = CodeTransformer.ReplaceWithIfDirective(candidate.ContainingBlock, candidate.ContainingStatement);

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(c);
            var newRoot = oldRoot.ReplaceNode(candidate.ContainingBlock, newBlock);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);

        }
    }
}