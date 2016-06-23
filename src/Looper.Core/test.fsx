#I __SOURCE_DIRECTORY__
#r "../../packages/System.IO/lib/netcore50/System.IO.dll"
#r "../../packages/System.Collections/lib/netcore50/System.Collections.dll"
#r "../../packages/System.Threading.Tasks/lib/netcore50/System.Threading.Tasks.dll"
#r "../../packages/System.Text.Encoding/lib/netcore50/System.Text.Encoding.dll"
#r "../../packages/System.Runtime/lib/netcore50/System.Runtime.dll"
#r "../../packages/Microsoft.CodeAnalysis.CSharp/lib/portable-net45+win8/Microsoft.CodeAnalysis.CSharp.dll"
#r "../../packages/Microsoft.CodeAnalysis.CSharp.Workspaces/lib/portable-net45+win8/Microsoft.CodeAnalysis.CSharp.Workspaces.dll"
#r "../../packages/Microsoft.CodeAnalysis.Common/lib/portable-net45+win8/Microsoft.CodeAnalysis.dll"
#r "../../packages/Microsoft.CodeAnalysis.VisualBasic/lib/portable-net45+win8/Microsoft.CodeAnalysis.VisualBasic.dll"
#r "../../packages/Microsoft.CodeAnalysis.VisualBasic.Workspaces/lib/portable-net45+win8/Microsoft.CodeAnalysis.VisualBasic.Workspaces.dll"
#r "../../packages/Microsoft.CodeAnalysis.Workspaces.Common/lib/portable-net45+win8/Microsoft.CodeAnalysis.Workspaces.dll"
#r "../../packages/System.Collections.Immutable/lib/portable-net45+win8+wp8+wpa81/System.Collections.Immutable.dll"
#r "../../packages/Microsoft.Composition/lib/portable-net45+win8+wp8+wpa81/System.Composition.AttributedModel.dll"
#r "../../packages/Microsoft.Composition/lib/portable-net45+win8+wp8+wpa81/System.Composition.Convention.dll"
#r "../../packages/Microsoft.Composition/lib/portable-net45+win8+wp8+wpa81/System.Composition.Hosting.dll"
#r "../../packages/Microsoft.Composition/lib/portable-net45+win8+wp8+wpa81/System.Composition.Runtime.dll"
#r "../../packages/Microsoft.Composition/lib/portable-net45+win8+wp8+wpa81/System.Composition.TypedParts.dll"
#r "../../packages/System.Reflection.Metadata/lib/portable-net45+win8/System.Reflection.Metadata.dll"
#r "../../output/bin/Looper.Core.dll"

open System
open System.Linq
open Looper.Core
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

let test = """
using System;
using System.Linq;
void Foo () {
    var xs = Enumerable.Range(1, 10);
    do {
        ;
    }while(xs.Select(x => x + 1).Any());
}
"""

let syntax = CSharpSyntaxTree.ParseText(test)
let mscorlib = MetadataReference.CreateFromFile(typeof<obj>.Assembly.Location)
let systemCore = MetadataReference.CreateFromFile(typeof<Enumerable>.Assembly.Location)
let compilation = CSharpCompilation.Create("TestCompilation").AddReferences(mscorlib, systemCore).AddSyntaxTrees(syntax)
let model = compilation.GetSemanticModel(syntax, false)
let root = syntax.GetRoot()

//root.DescendantNodes().OfType<InvocationExpressionSyntax>()
//|> Seq.map(fun n -> model.GetSymbolInfo(n.Expression))

SymbolUtils.initializeFromCompilation compilation

let expr =
    root.DescendantNodes().OfType<InvocationExpressionSyntax>()
    |> Seq.filter (fun e -> QueryTransformer.toQueryExpr e model <> None)
    |> Seq.exactlyOne

let newRoot = Transformer.refactor model root expr
newRoot.Value.ToFullString()