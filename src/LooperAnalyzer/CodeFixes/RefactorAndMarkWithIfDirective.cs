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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RefactorAndReplaceWithIfDirective)), Shared]
    public class RefactorAndReplaceWithIfDirective : CodeFixProvider
    {
        private const string title = "Refactor and replace with conditional optimization";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(LooperDiagnosticAnalyzer.NeedsRefactoringDiagnosticId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindNode(diagnosticSpan) as InvocationExpressionSyntax;

            if (declaration == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithIfDirectiveAction(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);

        }

        private async Task<Document> ReplaceWithIfDirectiveAction(Document document, InvocationExpressionSyntax stmt, CancellationToken c)
        {
            var model = await document.GetSemanticModelAsync(c);
            var oldRoot = await document.GetSyntaxRootAsync(c);
            var refactoring = CodeTransformer.getRefactoring(oldRoot, stmt);
            var newStmt = refactoring.RefactoredStatement;
            var newDoc = document.WithSyntaxRoot(refactoring.NewRoot);
            var newModel = await newDoc.GetSemanticModelAsync(c);
            var finalRoot = CodeTransformer.markWithDirective(newModel, refactoring.NewRoot, refactoring.RefactoredStatement);
            return newDoc.WithSyntaxRoot(finalRoot);
        }
    }
}