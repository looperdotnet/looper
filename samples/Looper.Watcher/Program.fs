open System.IO
open System
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.Text
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.MSBuild
open Microsoft.CodeAnalysis
open System.Linq
open Looper.Core

[<EntryPoint>]
let main argv = 
    let path =
        match argv with
        | [|path|] -> Path.Combine(Directory.GetCurrentDirectory(), path)
        | _ -> @"c:\Users\kostas\Desktop" //failwith "Pleace specify a directory"

    let watcher = new FileSystemWatcher(Path = path, NotifyFilter = NotifyFilters.LastWrite, Filter = "*.cs")
    
    let mscorlib = MetadataReference.CreateFromFile(typeof<obj>.Assembly.Location)
    let systemCore = MetadataReference.CreateFromFile(typeof<Enumerable>.Assembly.Location)
    let compilation = CSharpCompilation.Create("Watcher").AddReferences(mscorlib, systemCore)
    SymbolUtils.initializeFromCompilation(compilation)

    watcher.Changed.Add(fun e ->
        printfn "Changed %s" e.FullPath
        let tree = CSharpSyntaxTree.ParseText(File.ReadAllText(e.FullPath))
        let compilation = compilation.AddSyntaxTrees(tree)
        let model = compilation.GetSemanticModel(tree, false)
        let syntaxTree = model.SyntaxTree
        let root = syntaxTree.GetRoot()
        printfn "%A" (root.ToFullString())
    )

    watcher.EnableRaisingEvents <- true
    
    let _ = Console.ReadKey()

    0 // return an integer exit code
