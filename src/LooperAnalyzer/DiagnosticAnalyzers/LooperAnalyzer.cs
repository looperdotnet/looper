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

namespace LooperAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LooperDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string InvariantOptimizationDiagnosticId = "LO001";
        public const string UnsafeOptimizationDiagnosticId    = "LO002";
        public const string NoConsumerDiagnosticId            = "LO003";
        public const string InvalidExpressionDiagnosticId     = "LO004";

        private static readonly LocalizableString InvariantOptimizationTitle = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvariantOptimizationMessageFormat = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvariantOptimizationDescription = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString UnsafeOptimizationTitle = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString UnsafeOptimizationMessageFormat = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString UnsafeOptimizationDescription = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString NoConsumerTitle = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NoConsumerMessageFormat = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NoConsumerDescription = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString InvalidExpressionTitle = new LocalizableResourceString(nameof(Resources.InvalidExpressionAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidExpressionMessageFormat = new LocalizableResourceString(nameof(Resources.InvalidExpressionAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvalidExpressionDescription = new LocalizableResourceString(nameof(Resources.InvalidExpressionAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Optimization";

        private static DiagnosticDescriptor InvariantOptimizationRule = new DiagnosticDescriptor(InvariantOptimizationDiagnosticId, InvariantOptimizationTitle, InvariantOptimizationMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: InvariantOptimizationDescription);
        private static DiagnosticDescriptor UnsafeOptimizationRule = new DiagnosticDescriptor(UnsafeOptimizationDiagnosticId, UnsafeOptimizationTitle, UnsafeOptimizationMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: UnsafeOptimizationDescription);
        private static DiagnosticDescriptor NoConsumerRule = new DiagnosticDescriptor(NoConsumerDiagnosticId, NoConsumerTitle, NoConsumerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NoConsumerDescription);
        private static DiagnosticDescriptor InvalidExpressionRule = new DiagnosticDescriptor(InvalidExpressionDiagnosticId, InvalidExpressionTitle, InvalidExpressionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: InvalidExpressionDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(
                InvariantOptimizationRule, 
                UnsafeOptimizationRule,
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
            
            SymbolUtils.initializeFromCompilation(model.Compilation);

            var analyzer = Analyzer.analyze(model);
            
            foreach (var node in analyzer.OptimizationCandidates) {
                var rule = node.IsInvariantOptimization ? InvariantOptimizationRule : UnsafeOptimizationRule;
                var diagnostic = Diagnostic.Create(rule, node.Invocation.GetLocation(), node.ConsumerMethodName);
                context.ReportDiagnostic(diagnostic);
            }

            foreach (var node in analyzer.InvalidMarkedNodes) {
                Diagnostic diagnostic;
                if (node.IsInvalidExpression) {
                    var n = node as InvalidNode.InvalidExpression;
                    diagnostic = Diagnostic.Create(InvalidExpressionRule, n.trivia.GetLocation());
                } else {
                    var n = node as InvalidNode.NoConsumer;
                    diagnostic = Diagnostic.Create(NoConsumerRule, n.stmt.GetLocation());

                }
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
