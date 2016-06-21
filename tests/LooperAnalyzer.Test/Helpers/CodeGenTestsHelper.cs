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
using Xunit.Abstractions;

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

        public override string ToString() => Name;

        public string Format(string[] inits, string expr) => string.Format(Template, string.Join(Environment.NewLine, inits), expr);

        public void Deserialize(IXunitSerializationInfo info)
        {
            Name = info.GetValue<string>(nameof(Name));
            ResultExpression = info.GetValue<string>(nameof(ResultExpression));
            Template = info.GetValue<string>(nameof(Template));
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(Name), Name);
            info.AddValue(nameof(ResultExpression), ResultExpression);
            info.AddValue(nameof(Template), Template);
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
                        object Test() {{
                            {0};
                            var test = {1};
                            return test;
                        }}"
                },
            };
    }

    public abstract class CodeGenTestsBase : IClassFixture<CodeGenFixture>
    {
        protected readonly CodeGenFixture fixture;
        protected readonly string[] emptyInits = new string[0];

        public CodeGenTestsBase(CodeGenFixture fixture)
        {
            this.fixture = fixture;
        }

        public static readonly CodeGenTemplate[][] Templates = CodeGenTemplate.Templates.Select(t => new[] { t }).ToArray();

        protected async Task VerifyCodeGen(CodeGenTemplate template, string[] inits, string linqExpr)
        {
            var code = template.Format(inits, linqExpr);

            var script = fixture.DefaultScript.ContinueWith(code);
            var compilation = script.GetCompilation();
            var tree = compilation.SyntaxTrees.Single();
            var model = compilation.GetSemanticModel(tree);

            var stmtQuery = tree.GetRoot()
                .DescendantNodes()
                .OfType<LocalDeclarationStatementSyntax>()
                .Select(e => new { Node = e, LooperStmt = QueryTransformer.toStmtQueryExpr(e, model)?.Value })
                .SingleOrDefault(e => e.LooperStmt != null);

            Assert.NotNull(stmtQuery);

            var codegen = tree.GetRoot()
                .ReplaceNode(stmtQuery.Node, Compiler.compile(stmtQuery.LooperStmt, model))
                .ToFullString();

            var expected = await script.ContinueWith(template.ResultExpression).RunAsync();
            var actual = await script.ContinueWith(codegen).ContinueWith(template.ResultExpression).RunAsync();

            Assert.Equal(expected.ReturnValue, actual.ReturnValue);
        }
    }
}
