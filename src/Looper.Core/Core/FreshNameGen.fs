namespace Looper.Core

open System
open Microsoft.CodeAnalysis
open System.Collections.Generic
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax


type private Rewriter (model : SemanticModel, oldName : string, newToken : SyntaxToken) =
    inherit CSharpSyntaxRewriter()

    override this.VisitIdentifierName(node) =
        match model.GetSymbolInfo(node).Symbol with
        | null -> node :> _
        | sym when sym.Kind = SymbolKind.Parameter && node.Identifier.ValueText = oldName ->
            node.ReplaceToken(node.Identifier, newToken) :> _
        | _ -> node :> _

module NameUtils =
    let toCameCase (id : string) = (id.[0] |> Char.ToLower |> string) + id.[1..]

type FreshNameGen (model : SemanticModel, position : int) =
    let map = new Dictionary<string, int>()

    let getMax name =
        match map.TryGetValue(name) with
        | true, i -> i + 1
        | false, _ -> -1

    member __.Generate(name : string) =
        let rec fresh name index =
            let n = if index = -1 then name else sprintf "%s%d" name index
            if Seq.isEmpty <| model.LookupSymbols(position, name = n) then
                map.[name] <- index
                n
            else
                fresh name (index + 1)
        fresh name (getMax name)

    member this.Replace<'T when 'T :> SyntaxNode>(param : ParameterSyntax, body : 'T) =
        let oldName = param.Identifier.ValueText
        let newName = this.Generate oldName
        if oldName <> newName then
            let newToken = SyntaxFactory.Identifier newName
            newName, Rewriter(model, oldName, newToken).Visit(body) :?> 'T
        else
            oldName, body 