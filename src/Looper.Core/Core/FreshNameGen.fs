namespace Looper.Core

open System
open Microsoft.CodeAnalysis
open System.Collections.Generic


module NameUtils =
    let toCameCase (id : string) = (id.[0] |> Char.ToLower |> string) + id.[1..]

type FreshNameGen (context : SyntaxNode, model : SemanticModel) =
    let map = new Dictionary<string, int>()

    let getMax name =
        match map.TryGetValue(name) with
        | true, i -> i + 1
        | false, _ -> -1

    member __.Generate(name : string) =
        let rec fresh name index =
            let n = if index = -1 then name else sprintf "%s%d" name index
            if Seq.isEmpty <| model.LookupSymbols(context.SpanStart, name = n) then
                n
            else
                fresh name (index + 1)
        fresh name (getMax name)

    member this.GenerateAndReplace(name : SyntaxNode, node : SyntaxNode) =
        let name = this.Generate(name.ToFullString())
        failwith "Not implemented"