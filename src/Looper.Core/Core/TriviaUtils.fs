[<AutoOpen>]
module Looper.Core.TriviaUtils

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open System

let ifDefIdentifier = "LOOPER"
let private markerCommentText = "// looper"

let private markerComment = SyntaxFactory.Comment(markerCommentText)

let ifDirective =
    SyntaxFactory.Trivia(
      SyntaxFactory.IfDirectiveTrivia(
        SyntaxFactory.PrefixUnaryExpression(
            SyntaxKind.LogicalNotExpression,
            SyntaxFactory.IdentifierName(ifDefIdentifier)),
            true, true, true))

let elseDirective = 
    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(true, false))

let endDirective =
    SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(false))

[<AutoOpen>] 
module TriviaPatterns =
    let (|MarkedForOptimization|_|) (node : SyntaxNode) =
        let rec isMarked (trivia : SyntaxTrivia list) =
            match trivia with
            | [] -> None
            | trivia :: tail ->
                if trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToFullString() = markerCommentText then
                    Some trivia
                else if trivia.IsDirective && trivia.IsKind(SyntaxKind.IfDirectiveTrivia) then
                    match trivia.GetStructure() with
                    | :? ConditionalDirectiveTriviaSyntax as cond ->
                        let exists =
                            cond.DescendantNodesAndSelf()
                            |> Seq.choose(function :? PrefixUnaryExpressionSyntax as s -> Some s | _ -> None)
                            |> Seq.collect(fun n -> n.DescendantNodes())
                            |> Seq.choose(function :? IdentifierNameSyntax as s -> Some s | _ -> None)
                            |> Seq.exists(fun n -> n.Identifier.ToFullString().StartsWith ifDefIdentifier)
                        if exists then Some trivia else isMarked tail
                    | _ -> isMarked tail
                else 
                    isMarked tail

        node.GetLeadingTrivia()
        |> Seq.toList
        |> isMarked

type StatementSyntax with
    
    member node.AppendLeadingTrivia([<ParamArray>]trivia : SyntaxTrivia []) =
        let leading = node.GetLeadingTrivia().AddRange(trivia)
        node.WithLeadingTrivia(leading)

    member node.PrependLeadingTrivia([<ParamArray>]trivia : SyntaxTrivia []) =
        let trivia = node.GetLeadingTrivia().InsertRange(0, trivia)
        node.WithLeadingTrivia(trivia)

    member node.AppendTrailingTrivia([<ParamArray>]trivia : SyntaxTrivia []) =
        let trailing = node.GetTrailingTrivia().AddRange(trivia)
        node.WithTrailingTrivia(trailing)

    member node.PrependTrailingTrivia([<ParamArray>]trivia : SyntaxTrivia []) =
        let trivia = node.GetTrailingTrivia().InsertRange(0, trivia)
        node.WithTrailingTrivia(trivia)

    member node.IsMarkedWithOptimizationTrivia = 
        match node with 
        | MarkedForOptimization _ -> true 
        | _ -> false

    member node.MakeLeadingMarkComment() =
        let leading = node.GetLeadingTrivia()
        let whitespace = 
            leading 
            |> Seq.filter(fun t -> t.IsKind(SyntaxKind.WhitespaceTrivia))
            |> Seq.tryLast
        match whitespace with
        | None -> leading.Add(markerComment).Add(SyntaxFactory.ElasticCarriageReturnLineFeed)
        | Some w -> leading.Add(markerComment).Add(SyntaxFactory.ElasticCarriageReturnLineFeed).Add(w)


        