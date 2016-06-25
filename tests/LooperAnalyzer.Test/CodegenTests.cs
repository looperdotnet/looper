using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public class CodeGenTests : CodeGenTestsBase
    {
        public CodeGenTests(CodeGenFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

        [Theory(DisplayName = "Empty array > Sum"), MemberData(nameof(Templates))]
        public async Task EmptyArraySum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = new int [0];" },
                linqExpr: "xs.Sum()");

        [Theory(DisplayName = "Empty sequence > Sum"), MemberData(nameof(Templates))]
        public async Task EmptySequenceSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "System.Collections.Generic.IEnumerable<int> xs = new int [0];" },
                linqExpr: "xs.Sum()");

        [Theory(DisplayName = "Simple array > Sum"), MemberData(nameof(Templates))]
        public async Task SimpleArraySum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Sum()");

        [Theory(DisplayName = "Simple Range > Sum"), MemberData(nameof(Templates))]
        public async Task SimpleRangeSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = Enumerable.Range(1, 10);" },
                linqExpr: "xs.Sum()");

        [Theory(DisplayName = "Inline array > Sum"), MemberData(nameof(Templates))]
        public async Task InlineArraySum(CodeGenTemplate template) => 
            await VerifyCodeGen<int>(template, emptyInits, 
                "new [] { 1, 2, 3 }.Sum()");

        [Theory(DisplayName = "Simple array > Select > Sum"), MemberData(nameof(Templates))]
        public async Task SimpleArraySelectSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template,
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Select(x => x + 1).Sum()");

        [Theory(DisplayName = "Inline array > Select > Sum"), MemberData(nameof(Templates))]
        public async Task InlineArraySelectSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template, emptyInits,
                linqExpr: "new [] { 1, 2, 3 }.Select(x => x + 1).Sum()");

        [Theory(DisplayName = "Inline Range > Select > Sum"), MemberData(nameof(Templates))]
        public async Task InlineRangeSelectSum(CodeGenTemplate template) =>
            await VerifyCodeGen<int>(template, emptyInits,
                "Enumerable.Range(1, 10).Select(x => x + 1).Sum()");

        [Fact(DisplayName = "For all input.Select.Sum")]
        public void ForAllSelectSum() => VerifyCodeGenForAll<int[], int>("input.Select(x => x + 1).Sum()");
    }
}
