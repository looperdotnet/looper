using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using TestHelper;
using LooperAnalyzer;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Looper.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Xunit;
using Xunit.Abstractions;

namespace LooperAnalyzer.Test
{
    public class RefactorAndMarkWithIfDirectiveTests : CodeFixVerifier
    {
        public RefactorAndMarkWithIfDirectiveTests(ITestOutputHelper output) : base(output) { }
    }
}