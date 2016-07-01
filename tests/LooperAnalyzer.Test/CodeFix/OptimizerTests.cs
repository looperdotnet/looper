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
    public class OptimizerTests : CodeFixVerifier
    {
        public OptimizerTests(ITestOutputHelper output) : base(output)
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
            VerifyCSharpDiagnostic(test, Expected(7, 9));
        }

        [Fact(DisplayName = "Method argument as source")]
        public void MethodArg()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Method(int [] xs) {
        var ys = xs.Sum();
    }
}
";
            VerifyCSharpDiagnostic(test, Expected(5, 9));
        }

        [Fact(DisplayName = "Class field as source")]
        public void ClassField()
        {
            var test = @"
using System.Linq;
class TestClass
{
    int[] xs = new [] { 42 };
    void Method() {
        var ys = xs.Sum();
    }
}
";
            VerifyCSharpDiagnostic(test, Expected(7, 9));
        }

        [Fact(DisplayName = "Delegate as lambda")]
        public void Delegate()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Method(System.Func<int, int> f) {
        var ys = xs.Select(f).Sum();
    }
}
";
            VerifyCSharpDiagnostic(test, Expected(5, 9));
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
            VerifyCSharpDiagnostic(test, Expected(8, 9));

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
    var ys = default (int);
{
    var sum = 0;
    for (int i = 0; i < xs.Length; i++)
    {
        var x = xs[i];
        sum += x + 1;
        ys = sum;
    }
}
#endif
        return ys;
    }
}
";

            VerifyCSharpFix(test, fixtest);
        }

        [Fact(DisplayName = "Lambda with Block body")]
        public void LambdaBlock()
        {
            var test = @"
using System.Linq;
class TestClass
{
    int Method() 
    {
        var xs = new [] { 42 };
        var ys = xs.Select(x => { var y = x + 1; return y; }).Sum();
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
        var ys = xs.Select(x => { var y = x + 1; return y; }).Sum();
#else
    var ys = default (int);
    var sum = 0;
    for (int i = 0; i < xs.Length; i++)
    {
        var x = xs[i];
        var y = x + 1;
        sum += y;
        ys = sum;
    }
#endif
        return ys;
    }
}
";

            VerifyCSharpFix(test, fixtest);
        }


        [Fact(DisplayName = "Fresh names should not collide with scoped variables")]
        public void FreshNames()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Method() 
    {
        int x0, x1, sum;
        var xs = new [] { 42 };
        var ys = xs.Select(x => x + 1).Select(x => x + 2).Sum();
    }
}
";

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Method() 
    {
        int x0, x1, sum;
        var xs = new [] { 42 };
#if !LOOPER
        var ys = xs.Select(x => x + 1).Select(x => x + 2).Sum();
#else
    var ys = default (int);
{
    var sum0 = 0;
    for (int i = 0; i < xs.Length; i++)
    {
        var x = xs[i];
        var x2 = x + 1;
        sum0 += x2 + 2;
        ys = sum0;
    }
}
#endif
    }
}
";

            VerifyCSharpFix(test, fixtest);
        }
    }
}