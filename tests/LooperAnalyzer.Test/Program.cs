﻿using System;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var fixture = new CodeGenFixture();
            var tests = new CodeGenTests(fixture, new ConsoleOutputHelper());
            Task.Run(() => tests.ForAllSelectSum()).Wait();
        }

        class ConsoleOutputHelper : ITestOutputHelper
        {
            public void WriteLine(string message) => Console.WriteLine(message);

            public void WriteLine(string format, params object[] args) => Console.WriteLine(format, arg: args);
        }
    }
}
