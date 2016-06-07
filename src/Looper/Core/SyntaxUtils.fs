[<AutoOpen>]
module Looper.Core.SyntaxUtils

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp.Syntax

type SyntaxNode with
    member this.GetParentStatement () =
        this.AncestorsAndSelf()
        |> Seq.choose(function :? StatementSyntax as s -> Some s | _ -> None)
        |> Seq.tryHead