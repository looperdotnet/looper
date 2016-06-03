using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using LooperAnalyzer.Analysis;

namespace LooperAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LooperDiagnosticAnalyzer : DiagnosticAnalyzer
    {
        public const string InvariantOptimizationDiagnosticId = "LO0001";
        public const string UnsafeOptimizationDiagnosticId = "LO0002";

        private static readonly LocalizableString InvariantOptimizationTitle = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvariantOptimizationMessageFormat = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString InvariantOptimizationDescription = new LocalizableResourceString(nameof(Resources.InvariantOptimizationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString UnsafeOptimizationTitle = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString UnsafeOptimizationMessageFormat = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString UnsafeOptimizationDescription = new LocalizableResourceString(nameof(Resources.UnsafeOptimizationAnalyzerDescription), Resources.ResourceManager, typeof(Resources));


        private const string Category = "Optimization";

        private static DiagnosticDescriptor InvariantOptimizationRule = new DiagnosticDescriptor(InvariantOptimizationDiagnosticId, InvariantOptimizationTitle, InvariantOptimizationMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: InvariantOptimizationDescription);
        private static DiagnosticDescriptor UnsafeOptimizationRule = new DiagnosticDescriptor(UnsafeOptimizationDiagnosticId, UnsafeOptimizationTitle, UnsafeOptimizationMessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: UnsafeOptimizationDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(InvariantOptimizationRule, UnsafeOptimizationRule);

        public override void Initialize(AnalysisContext context)
        {
            // TODO: Should we register to something else?
            context.RegisterSemanticModelAction(AnalyzeSemanticModel);
        }

        private static void AnalyzeSemanticModel(SemanticModelAnalysisContext context)
        {
            var model = context.SemanticModel;
            
            SymbolUtils.InitializeFromCompilation(model.Compilation);

            var candidates = OptimizationCandidateAnalysis.GetCandidates(model);

            foreach (var node in candidates) {
                if (node.IsMarkedWithOptimizationTrivia)
                    continue;
                var rule = node.IsInvariantOptimization ? InvariantOptimizationRule : UnsafeOptimizationRule;
                var diagnostic = Diagnostic.Create(rule, node.Invocation.GetLocation(), node.ConsumerMethodName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
