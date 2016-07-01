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
    public class RefactoringTests : CodeFixVerifier
    {
        public RefactoringTests(ITestOutputHelper output) : base(output)
        {
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => new RefactorToStatement();

        [Fact(DisplayName = "QueryExprStmt declaration should not produce refactoring diagnostics")]
        public void DeclarationNoDiagnostics()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        var xs = Enumerable.Range(1,10).Count();
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        [Fact(DisplayName = "QueryExprStmt assignment should not produce refactoring diagnostics")]
        public void AssignNoDiagnostics()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        int xs;
        xs = Enumerable.Range(1,10).Count();
    }
}
";
            VerifyCSharpDiagnostic(test);
        }

        [Fact(DisplayName = "Refactoring fresh variable name should no collide with scope")]
        public void FreshVarCollision()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        int count;
        var xs = 1 + Enumerable.Range(1,10).Count();
    }
}
";
            VerifyCSharpDiagnostic(test, RefactoringDiagnostic(7, 22));

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Test() {
        int count;
        var count0 = Enumerable.Range(1,10).Count();
        var xs = 1 + count0;
    }
}
";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact(DisplayName = "Refactor expression from simple if statement")]
        public void IfStatement()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        var xs = new [] { 42 };
        if (xs.Any()) {
            ;
        }
    }
}
";
            VerifyCSharpDiagnostic(test, RefactoringDiagnostic(7, 13));

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Test() {
        var xs = new [] { 42 };
        var any = xs.Any();
        if (any) {
            ;
        }
    }
}
";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact(DisplayName = "Refactor expression from local declaration")]
        public void LocalDeclaration()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        var xs = 1 + Enumerable.Range(1,10).Count();
    }
}
";
            VerifyCSharpDiagnostic(test, RefactoringDiagnostic(6, 22));

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Test() {
        var count = Enumerable.Range(1,10).Count();
        var xs = 1 + count;
    }
}
";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact(DisplayName = "Refactor expression from assignment")]
        public void Assignment()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        int xs;
        xs = 1 + Enumerable.Range(1,10).Count();
    }
}
";
            VerifyCSharpDiagnostic(test, RefactoringDiagnostic(7, 9));

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Test() {
        int xs;
        var count = Enumerable.Range(1,10).Count();
        xs = 1 + count;
    }
}
";
            VerifyCSharpFix(test, fixtest);
        }

        [Fact(DisplayName = "Refactor expression from do-while")]
        public void DoStatement()
        {
            var test = @"
using System.Linq;
class TestClass
{
    void Test() {
        do {
            var x = 42;
        } while(xs = 1 + Enumerable.Range(1,x).Count());
    }
}
";
            VerifyCSharpDiagnostic(test, RefactoringDiagnostic(7, 13));

            var fixtest = @"
using System.Linq;
class TestClass
{
    void Test() {
        do {
            var x = 42;
            var count = Enumerable.Range(1, x).Count();
        } while(xs = 1 + count);
    }
}
";
            VerifyCSharpFix(test, fixtest);
        }

    }
}