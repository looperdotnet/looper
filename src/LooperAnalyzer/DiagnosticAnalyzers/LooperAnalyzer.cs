using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Looper.Core;
using static Looper.Core.Analyzer.AnalyzedNode;

namespace LooperAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LooperDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string OptimizableDiagnosticId       = "LO001";
        public const string NeedsRefactoringDiagnosticId  = "LO002";
        public const string NoConsumerDiagnosticId        = "LO003";
        public const string InvalidExpressionDiagnosticId = "LO004";

        private static readonly LocalizableString OptimizableTitle = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString OptimizableMessageFormat = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString OptimizableDescription = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString NeedsRefactoringTitle = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NeedsRefactoringMessageFormat = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NeedsRefactoringDescription = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString NoConsumerTitle = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NoConsumerMessageFormat = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NoConsumerDescription = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString InvalidExpressionTitle = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidExpressionMessageFormat = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidExpressionDescription = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Optimization";

        private static DiagnosticDescriptor OptimizableRule = new DiagnosticDescriptor(OptimizableDiagnosticId, OptimizableTitle, OptimizableMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: OptimizableDescription);
        private static DiagnosticDescriptor NeedsRefactoringRule = new DiagnosticDescriptor(NeedsRefactoringDiagnosticId, NeedsRefactoringTitle, NeedsRefactoringMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: NeedsRefactoringDescription);
        private static DiagnosticDescriptor NoConsumerRule = new DiagnosticDescriptor(NoConsumerDiagnosticId, NoConsumerTitle, NoConsumerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NoConsumerDescription);
        private static DiagnosticDescriptor InvalidExpressionRule = new DiagnosticDescriptor(InvalidExpressionDiagnosticId, InvalidExpressionTitle, InvalidExpressionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: InvalidExpressionDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                OptimizableRule, 
                NeedsRefactoringRule,
                NoConsumerRule,
                InvalidExpressionRule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Should we register to something else?
            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;

            foreach (var node in Analyzer.analyze(model)) {
                Diagnostic diagnostic = null;
                if(node is Invalid) {
                    var n = (Invalid)node;
                    diagnostic = Diagnostic.Create(InvalidExpressionRule, n.trivia.GetLocation());
                }
                else if(node is NoConsumer) {
                    var n = (NoConsumer)node;
                    diagnostic = Diagnostic.Create(NoConsumerRule, n.stmt.GetLocation());
                }
                else if(node is MarkedWithDirective) {
                    var n = (MarkedWithDirective)node;
                    if(n.isStale) {
                        //TODO: report diagnostic
                    }
                }
                else if(node is NeedsRefactoring) {
                    var n = (NeedsRefactoring)node;
                    diagnostic = Diagnostic.Create(NeedsRefactoringRule, n.node.GetLocation());
                }
                else if(node is Optimizable) {
                    var n = (Optimizable)node;
                    diagnostic = Diagnostic.Create(OptimizableRule, n.node.GetLocation());
                }

                if(diagnostic != null)
                    context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
