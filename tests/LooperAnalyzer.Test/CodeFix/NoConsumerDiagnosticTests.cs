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
    public class NoConsumerDiagnosticTests : CodeFixVerifier
    {
        public NoConsumerDiagnosticTests(ITestOutputHelper output) : base(output) { }

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

            VerifyCSharpDiagnostic(test, NoConsumerDiagnostic(5, 0));
        }
    }
}