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
using static LooperAnalyzer.ApplicationDiagnostics;

namespace LooperAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class LooperDiagnosticAnalyzer : DiagnosticAnalyzer
    {
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
                    diagnostic = Diagnostic.Create(NoConsumerRule, n.trivia.GetLocation());
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
