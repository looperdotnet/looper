using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public class CodeGenTests : CodeGenTestsBase
    {
        public CodeGenTests(ITestOutputHelper output) : base(output) { }

        [Fact(DisplayName = "Empty array > Sum")]
        public async Task EmptyArraySum() =>
            await VerifyCodeGen<int>(
                inits: new[] { "var xs = new int [0];" },
                linqExpr: "xs.Sum()");

        [Fact(DisplayName = "Empty sequence > Sum")]
        public async Task EmptySequenceSum() =>
            await VerifyCodeGen<int>(
                inits: new[] { "System.Collections.Generic.IEnumerable<int> xs = new int [0];" },
                linqExpr: "xs.Sum()");

        [Fact(DisplayName = "Simple array > Sum")]
        public async Task SimpleArraySum() =>
            await VerifyCodeGen<int>(
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Sum()");

        [Fact(DisplayName = "Simple Range > Sum")]
        public async Task SimpleRangeSum() =>
            await VerifyCodeGen<int>(
                inits: new[] { "var xs = Enumerable.Range(1, 10);" },
                linqExpr: "xs.Sum()");

        [Fact(DisplayName = "Inline array > Sum")]
        public async Task InlineArraySum() => 
            await VerifyCodeGen<int>(inits: emptyInits,
                linqExpr: "new [] { 1, 2, 3 }.Sum()");

        [Fact(DisplayName = "Simple array > Select > Sum")]
        public async Task SimpleArraySelectSum() =>
            await VerifyCodeGen<int>(
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Select(x => x + 1).Sum()");

        [Fact(DisplayName = "Inline array > Select > Sum")]
        public async Task InlineArraySelectSum() =>
            await VerifyCodeGen<int>(emptyInits,
                linqExpr: "new [] { 1, 2, 3 }.Select(x => x + 1).Sum()");

        [Fact(DisplayName = "Inline Range > Select > Sum")]
        public async Task InlineRangeSelectSum() =>
            await VerifyCodeGen<int>(emptyInits,
                "Enumerable.Range(1, 10).Select(x => x + 1).Sum()");

        [Fact(DisplayName = "Where")]
        public async Task Where() =>
            await VerifyCodeGen<int>(
                inits: new[] { "var xs = new [] { 1, 2, 3 };" },
                linqExpr: "xs.Where(x => x % 2 == 0).Sum()");

        [Fact(DisplayName = "For all input.Select.Sum")]
        public void ForAllSelectSum() => 
            VerifyCodeGenForAll<int[], int>(input => 
                $"{input}.Select(x => x + 1).Sum()");

        [Fact(DisplayName = "SelectMany")]
        public async Task SelectMany() =>
            await VerifyCodeGen<int>(
             inits: new[] { "var xs = new [] { 1, 2, 3 };" },
             linqExpr: "xs.SelectMany(n => Enumerable.Range(1, n).Select(x => x + 1)).Sum()");
    }
}
