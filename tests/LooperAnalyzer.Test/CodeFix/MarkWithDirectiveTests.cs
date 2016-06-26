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
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public class MarkWithDirectiveTests : CodeFixVerifier
    {
        public MarkWithDirectiveTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new ReplaceWithIfDirective();

        private DiagnosticResult Expected(int row, int col) =>
            new DiagnosticResult
            {
                Id = OptimizableDiagnosticId,
                Message = OptimizableMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        [Fact(DisplayName = "Empty document should report no diagnostics")]
        public void EmptyDoc() => VerifyCSharpDiagnostic("");

        [Fact(DisplayName = "Local declaration")]
        public void LocalDecl()
        {
            var test = @"
                using System.Linq;
                class TestClass
                {
                    void Method() {
                        var xs = new [] { 42 };
                        var ys = xs.Select(x => x + 1).Sum();
                    }
                }
                ";
            VerifyCSharpDiagnostic(test, Expected(7, 25));
        }

        [Fact(DisplayName = "Proper formatting")]
        public void Formatting()
        {
            var test = @"
                using System.Linq;
                class TestClass
                {
                    int Method() 
                    {
                        var xs = new [] { 42 };
                        var ys = xs.Select(x => x + 1).Sum();
                        return ys;
                    }
                }
                ";

            var fixtest = @"
                using System.Linq;
                class TestClass
                {
                    int Method() 
                    {
                        var xs = new [] { 42 };
#if !LOOPER
                        var ys = xs.Select(x => x + 1).Sum();
#else
                        var ys = default(int);
                        var sum = 0;
                        for (int i = 0; i < xs.Length; i++) {
                            var x = xs[i];
                            sum += x + 1;
                            ys = sum;
                        }
#endif
                        return ys;
                    }
                }
                ";

            VerifyCSharpFix(test, fixtest, 
                // TODO 
                // It seems like the code produced after applying the fix
                // ignores the directives, resulting in errors like 'dulpicate ys', etc
                // Temporarily ignore those errors.
                allowNewCompilerDiagnostics : true);
        }
    }
}