using Looper.Core;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit.Abstractions;
using Microsoft.CodeAnalysis.CSharp;
using LooperAnalyzer.Test.Helpers;
using FsCheck;

namespace LooperAnalyzer.Test
{
    public class Globals<TInput>
    {
        public TInput Input { get; set; }

        public static Globals<object> Empty { get; } = new Globals<object>();
    }

    public abstract class CodeGenTestsBase
    {
        private readonly ITestOutputHelper output;

        protected readonly string[] emptyInits = Array.Empty<string>();
        public CodeGenTestsBase(ITestOutputHelper output)
        {
            this.output = output;
        }

        private Script GetDefaultScript<TInput>(string code) =>
            CSharpScript.Create(
                code,
                ScriptOptions.Default
                    .WithReferences(typeof(object).Assembly)
                    .WithReferences(typeof(Enumerable).Assembly),
                typeof(Globals<TInput>));


        private Tuple<ScriptRunner<TOutput>, ScriptRunner<TOutput>> GetScriptRunners<TInput, TOutput>(string code, string testf)
        {
            var script = GetDefaultScript<TInput>(code);

            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var model = compilation.GetSemanticModel(tree);
            var checker = new SymbolChecker(model);

            var root = tree.GetRoot();
            var stmtQuery = root
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .Select(e => new { Node = e, LooperStmt = QueryTransformer.toStmtQueryExpr(e, checker)?.Value })
                .SingleOrDefault(e => e.LooperStmt != null);

            output.WriteLine("original\r\n{0}", root.ToFullString());

            Assert.False(stmtQuery == null,
                $"{nameof(QueryTransformer)} could not find an appropriate expression.");

            var newRoot = CodeTransformer.markWithDirective(model, root, stmtQuery.Node);

            var codegen = // TODO
                $"#define {TriviaUtils.ifDefIdentifier}" + Environment.NewLine +
                $"#if {TriviaUtils.ifDefIdentifier}" + Environment.NewLine +
                newRoot.ToFullString() + Environment.NewLine +
                "#endif";

            output.WriteLine("codegen\r\n{0}", codegen);

            Assert.False(root.IsEquivalentTo(newRoot),
                "Transformed syntax should not be equivalent to original.");

            var expectedF = script
                .ContinueWith<TOutput>(testf)
                .CreateDelegate();

            var actualF = script.ContinueWith(codegen)
                .ContinueWith<TOutput>(testf)
                .CreateDelegate();

            return Tuple.Create(expectedF, actualF);
        }


        protected async Task VerifyCodeGen<TOutput>(string[] inits, string linqExpr)
        {
            var init = string.Join(";\r\n", inits);

            var testExpr = "Test()";
            var code = string.Format(
@"using System.Linq;
{0} Test() {{
    {1}
    var test = {2};
    return test;
}}", typeof(TOutput).FullName, init, linqExpr);

            var tup = GetScriptRunners<object, TOutput>(code, testExpr);

            var expected = await tup.Item1.RunProtectedAsync(Globals<object>.Empty);
            var actual = await tup.Item2.RunProtectedAsync(Globals<object>.Empty);
            Assert.Equal(expected, actual);
        }

        protected void VerifyCodeGenForAll<TInput, TOutput>(Func<string, string> linqExprF)
        {
            // TODO : remove temp assignment
            var testExpr = "Test(Input)";
            var code = string.Format(
@"using System.Linq;
{0} Test({1} temp) {{
    var input = temp;
    var test = {2};
    return test;
}}", typeof(TOutput).FullName, typeof(TInput).FullName, linqExprF("input"));

            var tup = GetScriptRunners<TInput, TOutput>(code, testExpr);

            Prop.ForAll<TInput>(input => {
                var globals = new Globals<TInput> { Input = input };
                var expected = tup.Item1.RunProtected(globals);
                var actual = tup.Item2.RunProtected(globals);
                Assert.Equal(expected, actual);
            }).QuickCheckThrowOnFailure();
        }
    }

}
