﻿using System;
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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceWithIfDirective)), Shared]
    public class ReplaceWithIfDirective : CodeFixProvider
    {
        private const string title = "Replace with conditional optimization";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => 
            ImmutableArray.Create(ApplicationDiagnostics.OptimizableDiagnosticId);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var declaration = root.FindNode(diagnosticSpan) as StatementSyntax;

            if (declaration == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ReplaceWithIfDirectiveAction(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);

        }

        private async Task<Document> ReplaceWithIfDirectiveAction(Document document, StatementSyntax stmt, CancellationToken c)
        {
            var model = await document.GetSemanticModelAsync(c);
            var oldRoot = await document.GetSyntaxRootAsync(c);
            var newRoot = CodeTransformer.markWithDirective(model, oldRoot, stmt);
            return document.WithSyntaxRoot(newRoot);
        }
    }
}