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

namespace LooperAnalyzer.Test
{
    public class UnitTest : CodeFixVerifier
    {
        [Fact]
        public void TestTransformer()
        {
            var tree = CSharpSyntaxTree.ParseText(
            @"using System;
                using System.Collections.Generic;
                using System.Text;
                using System.Linq;

                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var xs = new int[] {1, 2, 3};
        
                            var ys = xs.Select(x => x + 1).Sum();
                        }
                    }
                }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(mscorlib, systemCore)
                .AddSyntaxTrees(tree);

            SymbolUtils.initializeFromCompilation(compilation);
            var model = compilation.GetSemanticModel(tree);
            var syntaxTree = model.SyntaxTree;
            var root = syntaxTree.GetRoot();
            var tests = root.DescendantNodes().OfType<BlockSyntax>().ToArray();
            foreach (var stmt in tests[0].Statements)
            {
                var queryExprOption = QueryTransformer.toStmtQueryExpr(stmt, model);
                if (queryExprOption != null)
                {
                    var newStmt = Compiler.compile(queryExprOption.Value, model).NormalizeWhitespace().ToFullString();
                }
            }
            //var queryExpr = QueryTransformer.toQueryExpr(tests[0]);
        }

    //    //No diagnostics expected to show up
    //    [Fact]
    //    public void TestMethod1()
    //    {
    //        var test = @"";

    //        VerifyCSharpDiagnostic(test);
    //    }

    //    //Diagnostic and CodeFix both triggered and checked for
    //    [Fact]
    //    public void TestMethod2()
    //    {
    //        var test = @"
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace ConsoleApplication1
    //{
    //    class TypeName
    //    {   
    //    }
    //}";
    //        var expected = new DiagnosticResult
    //        {
    //            Id = "LooperAnalyzer",
    //            Message = string.Format("Type name '{0}' contains lowercase letters", "TypeName"),
    //            Severity = DiagnosticSeverity.Warning,
    //            Locations =
    //                new[] {
    //                        new DiagnosticResultLocation("Test0.cs", 11, 15)
    //                    }
    //        };

    //        VerifyCSharpDiagnostic(test, expected);

    //        var fixtest = @"
    //using System;
    //using System.Collections.Generic;
    //using System.Linq;
    //using System.Text;
    //using System.Threading.Tasks;
    //using System.Diagnostics;

    //namespace ConsoleApplication1
    //{
    //    class TYPENAME
    //    {   
    //    }
    //}";
    //        VerifyCSharpFix(test, fixtest);
    //    }

    //    protected override CodeFixProvider GetCSharpCodeFixProvider()
    //    {
    //        return new ReplaceWithIfDirective();
    //    }

    //    protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
    //    {
    //        return new LooperDiagnosticAnalyzer();
    //    }
    }
}