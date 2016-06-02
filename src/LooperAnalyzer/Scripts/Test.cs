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
            
        }
    }
}
