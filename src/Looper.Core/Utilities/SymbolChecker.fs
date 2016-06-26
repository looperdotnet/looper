namespace Looper.Core

open Microsoft.CodeAnalysis
open System.Collections.Generic


/// Wrapper around semantic model
type SymbolChecker(model : SemanticModel) = 
    let compilation = model.Compilation

    let allLinqMethods = 
        let methods = 
            let meta = compilation.GetTypeByMetadataName("System.Linq.Enumerable")
            if isNull meta then Seq.empty
            else meta.GetMembers() |> Seq.where (fun m -> m :? IMethodSymbol)
        new HashSet<ISymbol>(methods)
    
    let genericIEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1")
    
    member __.SemanticModel = model

    member __.IsLinqMethodType(symbol : ISymbol) = 
        match symbol with
        | :? IMethodSymbol as s when s.IsExtensionMethod && allLinqMethods.Contains s.ReducedFrom 
                                     || allLinqMethods.Contains s -> Some s
        | _ -> None
    
    member __.IsIEnumerableType(symbol : ISymbol) = 
        match symbol with
        | :? INamedTypeSymbol as s when s.OriginalDefinition = genericIEnumerableType 
                                        || s.OriginalDefinition.AllInterfaces.Contains(genericIEnumerableType) -> Some s
        | _ -> None


[<AutoOpen>]
module SymbolPatterns =
    let (|ArrayType|_|) (symbol : ISymbol) = 
        match symbol with
        | :? IArrayTypeSymbol as s -> Some s
        | _ -> None