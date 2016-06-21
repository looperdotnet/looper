using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public class CodeGenTests : CodeGenTestsBase
    {
        public CodeGenTests(CodeGenFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

        [Theory(DisplayName = "Simple Range > Sum"), MemberData(nameof(Templates))]
        public async Task SimpleRangeExprSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Sum()");

        [Theory(DisplayName = "Simple array > Sum"), MemberData(nameof(Templates))]
        public async Task SimpleArrayExprSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = Enumerable.Range(1, 10);" },
                linqExpr:      "xs.Sum()");

        [Theory(DisplayName = "Inline array > Sum"), MemberData(nameof(Templates))]
        public async Task InlineArrayExprSum(CodeGenTemplate template) => 
            await VerifyCodeGen<int>(template, emptyInits, 
                "new [] { 1, 2, 3 }.Sum()");

        [Theory(DisplayName = "Inline Range > Select > Sum"), MemberData(nameof(Templates))]
        public async Task InlineRangeExprSelectSum(CodeGenTemplate template) => 
            await VerifyCodeGen<int>(template, emptyInits, 
                "Enumerable.Range(1, 10).Select(x => x + 1).Sum()");
    }
}
