﻿using Microsoft.CodeAnalysis;
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
using static LooperAnalyzer.ApplicationDiagnostics;

namespace LooperAnalyzer.Test
{
    public class RefactorToStatementTests : CodeFixVerifier
    {
        protected override CodeFixProvider GetCSharpCodeFixProvider() => new RefactorToStatement();

        private DiagnosticResult Expected(int row, int col) =>
            new DiagnosticResult
            {
                Id = NeedsRefactoringDiagnosticId,
                Message = NeedsRefactoringMessageFormat.ToString(),
                Severity = DiagnosticSeverity.Info,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", row, col) }
            };

        //Diagnostic and CodeFix both triggered and checked for
        [Fact(DisplayName = "Refactor expression from simple if statement")]
        public void IfStatement()
        {
            var test = @"
                using System.Collections.Generic;
                class TestClass
                {
                    void Test() {
                        var xs = new [] { 42 };
                        if(xs.Any()) {
                            ;
                        }
                    }
                }
                ";
            VerifyCSharpDiagnostic(test, Expected(7, 28));

            var fixtest = @"
                using System.Collections.Generic;
                class TestClass
                {
                    void Test() {
                        var xs = new [] { 42 };
                        var any = xs.Any();
                        if(any) {
                            ;
                        }
                    }
                }
                ";

            VerifyCSharpFix(test, fixtest);
        }
    }
}