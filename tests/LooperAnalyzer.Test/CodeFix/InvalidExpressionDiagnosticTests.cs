using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using LooperAnalyzer;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Looper.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using static LooperAnalyzer.ApplicationDiagnostics;

namespace LooperAnalyzer.Test
{
    public class InvalidExpressionDiagnosticTests : CodeFixVerifier
    {
        private DiagnosticResult Expected(int row, int col) =>
            new DiagnosticResult
            {
                Id = InvalidExpressionDiagnosticId,
                Message = InvalidExpressionMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        [Fact(DisplayName = "Using ifdef with invalid declaration")]
        public void InvalidExpr()
        {
            var test = @"
        class TestClass
        {
            void Method() {
#if !LOOPER
            var xs = 42;
#else
            ;
#endif   
            }
        }";
            VerifyCSharpDiagnostic(test, Expected(5,0));
        }
    }
}