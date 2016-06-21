using System.Threading.Tasks;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var fixture = new CodeGenFixture();
            var tests = new CodeGenTests(fixture);
            Task.Run(() => tests.ArrayExprSum()).Wait();
        }
    }
}
