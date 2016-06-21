using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fixture = new CodeGenFixture();
            var tests = new CodeGenTests(fixture, new TestOutputHelper());
            Task.Run(() => tests.SimpleArrayExprSum(CodeGenTemplate.Templates[0])).Wait();
        }
    }
}
