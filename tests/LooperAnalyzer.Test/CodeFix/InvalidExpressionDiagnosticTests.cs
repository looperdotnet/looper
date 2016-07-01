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
    public class InvalidExpressionDiagnosticTests : CodeFixVerifier
    {
        public InvalidExpressionDiagnosticTests(ITestOutputHelper output) : base(output)
        {
        }

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
            VerifyCSharpDiagnostic(test, InvalidExpressionDiagnostic(5,1));
        }
    }
}