using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LooperAnalyzer.Test.Scripts
{
    public class Program
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

            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                .AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);

            // In the analyzer we get the model directly; get back the syntax tree and move on

            var syntaxTree = model.SyntaxTree;

            var root = syntaxTree.GetRoot();

            var methodDeclarations = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>();

            var methodInvocations = methodDeclarations
                .SelectMany(md => md.DescendantNodes().OfType<InvocationExpressionSyntax>());

            
        }
    }
}
