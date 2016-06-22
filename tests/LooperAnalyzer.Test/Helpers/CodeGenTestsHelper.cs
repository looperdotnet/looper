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

    public class CodeGenTemplate : IXunitSerializable
    {
        public string Name { get; set; }
        public string ResultExpression { get; set; }
        public string Template { get; set; }
        public string ResultType { get; set; } = "object";

        public override string ToString() => Name;

        public string Format(string[] inits, string expr) => 
            string.Format(Template, ResultType, string.Join(Environment.NewLine, inits), expr);

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            ResultExpression = info.GetValue<string>(nameof(ResultExpression));
            Template = info.GetValue<string>(nameof(Template));
            ResultType = info.GetValue<string>(nameof(ResultType));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(ResultExpression), ResultExpression);
            info.AddValue(nameof(Template), Template);
            info.AddValue(nameof(ResultType), ResultType);
        }

        public static CodeGenTemplate[] Templates =
            new[]
            {
                new CodeGenTemplate
                {
                    Name = "Local declaration in method body",
                    ResultExpression = "Test()",
                    Template = @"
                        using System.Linq;
                        {0} Test() {{
                            {1};
                            var test = {2};
                            return test;
                        }}"
                },
            };
    }

    public abstract class CodeGenTestsBase : IClassFixture<CodeGenFixture>
    {
        protected readonly CodeGenFixture fixture;
        protected readonly string[] emptyInits = new string[0];
        private readonly ITestOutputHelper output;

        public static readonly CodeGenTemplate[][] Templates = CodeGenTemplate.Templates.Select(t => new[] { t }).ToArray();

        public CodeGenTestsBase(CodeGenFixture fixture, ITestOutputHelper output)
        {
            this.fixture = fixture;
            this.output = output;
        }

        protected async Task VerifyCodeGen<T>(string code, string resultExpr)
        {
            var script = fixture.DefaultScript.ContinueWith(code);
            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var model = compilation.GetSemanticModel(tree);
            var root = tree.GetRoot();

            var stmtQuery = root
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .Select(e => new { Node = e, LooperStmt = QueryTransformer.toStmtQueryExpr(e, model)?.Value })
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

            var expected = await script
                .ContinueWith<T>(resultExpr)
                .RunProtectedAsync();

            var actual = await script
                .ContinueWith(codegen)
                .ContinueWith<T>(resultExpr)
                .RunProtectedAsync();

            Assert.Equal(expected, actual);
        }

        protected async Task VerifyCodeGen<T>(CodeGenTemplate template, string[] inits, string linqExpr)
        {
            template.ResultType = typeof(T).FullName;
            var code = template.Format(inits, linqExpr);
            await VerifyCodeGen<T>(code, template.ResultExpression);
        }
    }
}
