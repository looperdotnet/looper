using System;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //var tests = new CodeGenTests(new ConsoleOutputHelper());
            //tests.SelectMany().Wait();

            //
            var tests = new MarkWithDirectiveTests(new ConsoleOutputHelper());
            tests.Formatting();
        }

        class ConsoleOutputHelper : ITestOutputHelper
        {
            public void WriteLine(string message) => Console.WriteLine(message);

            public void WriteLine(string format, params object[] args) => Console.WriteLine(format, arg: args);
        }
    }
}
