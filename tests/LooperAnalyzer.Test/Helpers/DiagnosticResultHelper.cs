using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using TestHelper;
using static LooperAnalyzer.ApplicationDiagnostics;

namespace TestHelper
{
    public abstract partial class CodeFixVerifier
    {
        internal DiagnosticResult RefactoringDiagnostic(int row, int col) =>
            new DiagnosticResult
            {
                Id = NeedsRefactoringDiagnosticId,
                Message = NeedsRefactoringMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        internal DiagnosticResult OptimizeDiagnostic(int row, int col) =>
            new DiagnosticResult
            {
                Id = OptimizableDiagnosticId,
                Message = OptimizableMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        internal DiagnosticResult InvalidExpressionDiagnostic(int row, int col) =>
            new DiagnosticResult
            {
                Id = InvalidExpressionDiagnosticId,
                Message = InvalidExpressionMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        internal DiagnosticResult NoConsumerDiagnostic(int row, int col) =>
            new DiagnosticResult
            {
                Id = NoConsumerDiagnosticId,
                Message = NoConsumerMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };
    }
}
