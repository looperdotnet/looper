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
    public class NoConsumerDiagnosticTests : CodeFixVerifier
    {
        private DiagnosticResult Expected(int row, int col) =>
            new DiagnosticResult
            {
                Id = NoConsumerDiagnosticId,
                Message = NoConsumerMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Warning,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        [Fact(DisplayName = "Using ifdef without consumer")]
        public void NoConsumer()
        {
            var test = @"
        class TestClass
        {
            void Method() {
#if !LOOPER
            var xs = Enumerable.Range(0,1).Select(x => x + 1);
#else
            ;
#endif   
            }
        }";

            VerifyCSharpDiagnostic(test, Expected(5, 0));
        }
    }
}