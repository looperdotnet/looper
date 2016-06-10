[<AutoOpen>]
module Looper.Core.SymbolUtils

open Microsoft.CodeAnalysis
open System.Collections.Generic

let mutable private whitelist = Unchecked.defaultof<_>
let mutable private allLinqMethods = Unchecked.defaultof<_>
let mutable private genericIEnumerableType = Unchecked.defaultof<_>

let initializeFromCompilation(compilation : Compilation) = 
    if isNull whitelist || isNull allLinqMethods || isNull genericIEnumerableType then
        let whitelistNames = 
            set [
                "First"
                "FirstOrDefault"
                "Single"
                "SingleOrDefault"
                "Any"
                "Average"
                "Count"
                "ElementAt"
                "ElementAtOrDefault"
                "Last"
                "LastOrDefault"
                "Max"
                "Min"
                "Single"
                "SingleOrDefault"
                "Sum"
                "ToArray"
                "ToList"
            ]

        let methods = compilation.GetTypeByMetadataName("System.Linq.Enumerable").GetMembers() |> Seq.where(fun m -> m :? IMethodSymbol)
        let filtered = methods |> Seq.filter(fun m -> whitelistNames.Contains(m.Name))

        allLinqMethods <- new HashSet<ISymbol>(methods)
        whitelist <- new HashSet<ISymbol>(filtered)
        genericIEnumerableType <- compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")

type IMethodSymbol with
    member this.IsOptimizableConsumerMethod =
        if this.IsExtensionMethod then whitelist.Contains this.ReducedFrom else whitelist.Contains this

    member this.IsLinqMethod = 
        if this.IsExtensionMethod then allLinqMethods.Contains this.ReducedFrom else allLinqMethods.Contains this


type ITypeSymbol with
    member this.IsArrayType = this :? IArrayTypeSymbol

    member this.IsIEnumerableType = 
        match this with
        | :? INamedTypeSymbol as ita -> 
            ita.OriginalDefinition = genericIEnumerableType || ita.OriginalDefinition.AllInterfaces.Contains(genericIEnumerableType)
        | _ -> false

    member this.IsOptimizableSourceType = this.IsArrayType || this.IsIEnumerableType
