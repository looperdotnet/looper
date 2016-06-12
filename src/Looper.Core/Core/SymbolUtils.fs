module Looper.Core.SymbolUtils

open Microsoft.CodeAnalysis
open System.Collections.Generic

let mutable private allLinqMethods = Unchecked.defaultof<_>
let mutable private genericIEnumerableType = Unchecked.defaultof<_>

let initializeFromCompilation (compilation : Compilation) = 
    // TODO : check if System.Linq is referenced in compilation
    if isNull allLinqMethods || isNull genericIEnumerableType then 
        let methods = 
            compilation.GetTypeByMetadataName("System.Linq.Enumerable").GetMembers() 
            |> Seq.where (fun m -> m :? IMethodSymbol)
        allLinqMethods <- new HashSet<ISymbol>(methods)
        genericIEnumerableType <- compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")

let (|LinqMethod|Other|) (symbol : ISymbol) = 
    if isNull allLinqMethods then invalidOp "Not initialized from a compilation"
    match symbol with
    | :? IMethodSymbol as s when s.IsExtensionMethod && allLinqMethods.Contains s.ReducedFrom 
                                 || allLinqMethods.Contains s -> LinqMethod s
    | _ -> Other

let (|ArrayType|_|) (symbol : ISymbol) = 
    match symbol with
    | :? IArrayTypeSymbol as s -> Some s
    | _ -> None

let (|IEnumerableType|_|) (symbol : ISymbol) = 
    match symbol with
    | :? INamedTypeSymbol as s when s.OriginalDefinition = genericIEnumerableType 
                                    || s.OriginalDefinition.AllInterfaces.Contains(genericIEnumerableType) -> Some s
    | _ -> None