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
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Editing;
using LooperAnalyzer.Compilation;

namespace LooperAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(LooperAnalyzerCodeFixProvider)), Shared]
    public class LooperAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Optimize Linq expression";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(LooperAnalyzerAnalyzer.DiagnosticId);

        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<InvocationExpressionSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument: c => ConvertToLoopAsync(context.Document, declaration, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> ConvertToLoopAsync(Document document, InvocationExpressionSyntax invocationExpr, CancellationToken cancellationToken)
        {
            // Find a way to pass the fix from the analyzer to code fox provider
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var memberAccessExpr = invocationExpr.Expression as MemberAccessExpressionSyntax;
            var memberSymbol = semanticModel.GetSymbolInfo(memberAccessExpr).Symbol as IMethodSymbol;

            var assign = invocationExpr.FirstAncestorOrSelf<LocalDeclarationStatementSyntax>();
            var newAssign = CodeTransformer.WrapWithIfDirective(assign);

            // Replace the old local declaration with the new local declaration.
            var oldRoot = await document.GetSyntaxRootAsync(cancellationToken);
            var newRoot = oldRoot.ReplaceNode(assign, newAssign);

            // Return document with transformed tree.
            return document.WithSyntaxRoot(newRoot);

        }
    }
}