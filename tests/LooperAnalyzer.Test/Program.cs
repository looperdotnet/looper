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
            LooperAnalyzer.Scripts.Test.Main(new[] {
                typeof(object).Assembly.Location,
                typeof(Enumerable).Assembly.Location });
        }

        static void Test()
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
                            var xs = Enumerable.Range(1,10);
        
                            var zs = xs.Select(x => x + 1);
#if !LOOPER_OPT
                            var ys = xs.Select(x => x * x).Where(x => x % 2 == 0).FirstOrDefault();
#else
                            throw new System.NotImplementedException();
#endif
                        }
                    }
                }");

            var mscorlib = MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
            var systemCore = MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
            var compilation = CSharpCompilation
                .Create("HelloWorld")
                .AddReferences(mscorlib, systemCore)
                .AddSyntaxTrees(tree);

            // TODO : check for successful compilation (without emit)


            // In the analyzer we get the model directly; get back the syntax tree and move on
            var model = compilation.GetSemanticModel(tree);

            var syntaxTree = model.SyntaxTree;

            var root = syntaxTree.GetRoot();

            

            var w = new Walker();
            w.Visit(root);
        }



        class Walker : CSharpSyntaxWalker
        {
            public override void VisitIfDirectiveTrivia(IfDirectiveTriviaSyntax node)
            {
                Console.WriteLine(node.ToFullString());
                base.VisitIfDirectiveTrivia(node);
            }

            public override void VisitElseDirectiveTrivia(ElseDirectiveTriviaSyntax node)
            {
                Console.WriteLine(node.ToFullString());
                base.VisitElseDirectiveTrivia(node);
            }

            public override void VisitEndIfDirectiveTrivia(EndIfDirectiveTriviaSyntax node)
            {
                Console.WriteLine(node.ToFullString());
                base.VisitEndIfDirectiveTrivia(node);
            }

            public override void DefaultVisit(SyntaxNode node)
            {
                Console.WriteLine(node.Kind() + " : " + node.ToFullString());
                base.DefaultVisit(node);
            }

        }
    }
}
