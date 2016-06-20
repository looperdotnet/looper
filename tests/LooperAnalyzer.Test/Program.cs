using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var tests = new CodegenTests();
            Task.Run(() => tests.RangeSelectSum()).Wait();
        }
    }
}
