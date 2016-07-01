using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public partial class CodeGenTests : CodeGenTestsBase
    {
        [Fact(DisplayName = "For all input.Select.Sum")]
        public void ForAllSelectSum() => 
            VerifyCodeGenForAll<int[], int>(input => 
                $"{input}.Select(x => x + 1).Sum()");

        [Fact(DisplayName = "For all input.Count")]
        public void ForAllCount() =>
            VerifyCodeGenForAll<int[], int>(input =>
                $"{input}.Where(x => x % 2 == 0).Count()");

        [Fact(DisplayName = "For all input.Any")]
        public void ForAllAny() =>
            VerifyCodeGenForAll<int[], bool>(input =>
                $"{input}.Any()");

    }
}
