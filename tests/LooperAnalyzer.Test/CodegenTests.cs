using Looper.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;
using Xunit;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LooperAnalyzer.Test
{
    //public class CodegenFixture : IDisposable
    //{
    //    public CSharpCompilation DefaultCompilation { get; private set; }
    //    public ScriptOptions DefaultScriptOptions { get; private set; }

    //    public CodegenFixture()
    //    {
    //        var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
    //        var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
    //        var compilation = CSharpCompilation.Create("UnitTestCompilation").AddReferences(mscorlib, systemCore);
    //        SymbolUtils.initializeFromCompilation(compilation);

    //        DefaultScriptOptions = ScriptOptions.Default
    //            .WithImports("System")
    //            .WithImports("System.Linq")
    //            .WithReferences("mscorlib")
    //            .WithReferences("System.Core");
    //        DefaultCompilation = compilation;
    //    }

    //    public void Dispose()
    //    {

    //    }
    //}


    public class CodegenTests : CodeFixVerifier //, IClassFixture<CodegenFixture>
    {
        //CodegenFixture fixture;

        //public CodegenTests(CodegenFixture fixture)
        //{
        //    this.fixture = fixture;
        //}

        async Task VerifyCodeGen(string linq)
        {
            var code = @"
                using System.Linq;
                object TestF() { 
                    var test = " + linq + @"; 
                    return test; 
                }";

            var script = CSharpScript.Create(code, 
                ScriptOptions.Default
                .WithReferences(typeof(object).Assembly)
                .WithReferences(typeof(Enumerable).Assembly));

            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var model = compilation.GetSemanticModel(tree);
            
            var expr = tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .SingleOrDefault();

            Assert.NotNull(expr);

            SymbolUtils.initializeFromCompilation(compilation);
            var stmtQuery = QueryTransformer.toStmtQueryExpr(expr, model)?.Value;
            
            Assert.NotNull(stmtQuery);

            var codegen = tree.GetRoot()
                .ReplaceNode(expr, Compiler.compile(stmtQuery, model))
                .ToFullString();
                
            var expected = await script.ContinueWith("TestF()").RunAsync();
            var actual = await script.ContinueWith(codegen).ContinueWith("TestF()").RunAsync();
            
            Assert.Equal(expected.ReturnValue, actual.ReturnValue);
        }
        
        [Fact(DisplayName = "Simple LINQ expression")]
        public async Task RangeSelectSum() => await VerifyCodeGen("Enumerable.Range(1, 10).Select(x => x + 1).Sum()");

    }
}
