using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer
{
    public static class ApplicationDiagnostics
    {
        public const string OptimizableDiagnosticId       = "LO001";
        public const string NeedsRefactoringDiagnosticId  = "LO002";
        public const string NoConsumerDiagnosticId        = "LO003";
        public const string InvalidExpressionDiagnosticId = "LO004";

        public static readonly LocalizableString OptimizableTitle               = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString OptimizableMessageFormat       = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString OptimizableDescription         = new LocalizableResourceString(nameof(Resources.OptimizableNodeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        public static readonly LocalizableString NeedsRefactoringTitle          = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString NeedsRefactoringMessageFormat  = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString NeedsRefactoringDescription    = new LocalizableResourceString(nameof(Resources.NeedsRefactoringAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        public static readonly LocalizableString NoConsumerTitle                = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString NoConsumerMessageFormat        = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString NoConsumerDescription          = new LocalizableResourceString(nameof(Resources.NoConsumerAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        public static readonly LocalizableString InvalidExpressionTitle         = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString InvalidExpressionMessageFormat = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        public static readonly LocalizableString InvalidExpressionDescription   = new LocalizableResourceString(nameof(Resources.InvalidNodeAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        public const string Category = "Optimization";

        public static DiagnosticDescriptor OptimizableRule       = new DiagnosticDescriptor(OptimizableDiagnosticId, OptimizableTitle, OptimizableMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: OptimizableDescription);
        public static DiagnosticDescriptor NeedsRefactoringRule  = new DiagnosticDescriptor(NeedsRefactoringDiagnosticId, NeedsRefactoringTitle, NeedsRefactoringMessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: NeedsRefactoringDescription);
        public static DiagnosticDescriptor NoConsumerRule        = new DiagnosticDescriptor(NoConsumerDiagnosticId, NoConsumerTitle, NoConsumerMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NoConsumerDescription);
        public static DiagnosticDescriptor InvalidExpressionRule = new DiagnosticDescriptor(InvalidExpressionDiagnosticId, InvalidExpressionTitle, InvalidExpressionMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: InvalidExpressionDescription);
    }
}
