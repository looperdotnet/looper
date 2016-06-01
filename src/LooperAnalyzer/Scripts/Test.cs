using LooperAnalyzer.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LooperAnalyzer.Scripts
{
    public static class Test
    {
        public static void Main(string[] args)
        {

            var tree = CSharpSyntaxTree.ParseText(
              @"using System;
                using System.Collections.Generic;
                using System.Text;
                using System.Linq;

                namespace HelloWorld
                {
                    class Program
                    {
                        static void Main(string[] args)
                        {
                            var xs = new [] { 1, 2, 3 };
                            var ys = xs.Select(x => x * x).Sum();
                        }
                    }
                }");

            var mscorlib = MetadataReference.CreateFromFile(args[0]);
            var systemCore = MetadataReference.CreateFromFile(args[1]);
            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(mscorlib, systemCore)
                .AddSyntaxTrees(tree);

            // TODO : check for successful compilation (without emit)


            // In the analyzer we get the model directly; get back the syntax tree and move on
            var model = compilation.GetSemanticModel(tree);

            var syntaxTree = model.SyntaxTree;

            var root = syntaxTree.GetRoot();

            var methodDeclarations = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            var invocationNodes = methodDeclarations
                .SelectMany(md => md.DescendantNodes().OfType<InvocationExpressionSyntax>());

            var node = invocationNodes.Last(); // xs.Select(x => x * x)
            var arguments = node.ArgumentList; // x => x * x
            var memberAccess = node.Expression as MemberAccessExpressionSyntax; // xs.Select

            // xs
            var xsExpr = memberAccess.Expression;

            var typ = model.GetTypeInfo(xsExpr).Type;
            typ.OriginalDefinition.AllInterfaces.Contains(compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1"));


            var enumerableTyp = model.Compilation.GetTypeByMetadataName("System.Linq.Enumerable");

            var xsSymbol = model.GetSymbolInfo(xsExpr).Symbol;
            var xsTyp = model.GetTypeInfo(xsExpr).Type; // ArrayTypeSymbol.SZArray(ArrayType System.Int32[])
            var isEnumerable = xsTyp.AllInterfaces.Any(typeSym => typeSym.Name.Contains("IEnumerable")); // ...

            // Select
            var select = memberAccess.Name;
            var selectTyp = model.GetTypeInfo(select).Type;


            var recognizer = new OptimizationCandidateVisitor(model);
            recognizer.Visit(root);
            var cs = recognizer.Candidates;
        }
    }
}
