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
    public class CodeGenFixture : IDisposable
    {
        public Script DefaultScript { get; private set; }

        public CodeGenFixture()
        {
            DefaultScript = CSharpScript.Create(
                "",
                ScriptOptions.Default
                .WithReferences(typeof(object).Assembly)
                .WithReferences(typeof(Enumerable).Assembly));

            SymbolUtils.initializeFromCompilation(DefaultScript.GetCompilation());
        }

        public void Dispose()
        {

        }
    }

    public class CodeGenTests : IClassFixture<CodeGenFixture>
    {
        CodeGenFixture fixture;

        public CodeGenTests(CodeGenFixture fixture)
        {
            this.fixture = fixture;
        }

        async Task VerifyCodeGen(string linq)
        {
            var code = @"
                using System.Linq;
                object TestF() { 
                    var test = " + linq + @"; 
                    return test; 
                }";

            var script = fixture.DefaultScript.ContinueWith(code);
            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var model = compilation.GetSemanticModel(tree);
            
            var expr = tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .SingleOrDefault();

            Assert.NotNull(expr);

            var stmtQuery = QueryTransformer.toStmtQueryExpr(expr, model)?.Value;
            
            Assert.NotNull(stmtQuery);

            var codegen = tree.GetRoot()
                .ReplaceNode(expr, Compiler.compile(stmtQuery, model))
                .ToFullString();
                
            var expected = await script.ContinueWith("TestF()").RunAsync();
            var actual = await script.ContinueWith(codegen).ContinueWith("TestF()").RunAsync();
            
            Assert.Equal(expected.ReturnValue, actual.ReturnValue);
        }

        [Fact(DisplayName = "Producer array expression > Sum")]
        public async Task ArrayExprSum() => 
            await VerifyCodeGen("new { 1, 2, 3 }.Sum()");

        [Fact(DisplayName = "Producer range expression > Select > Sum")]
        public async Task RangeExprSelectSum() => 
            await VerifyCodeGen("Enumerable.Range(1, 10).Select(x => x + 1).Sum()");

    }
}
