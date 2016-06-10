[<AutoOpen>]
module Looper.Core.TriviaUtils

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax

let private ifDefIdentifier = "LOOPER"
let private markerCommentText = "// looper"

let private markerComment = SyntaxFactory.Comment(markerCommentText)

let private ifDirective =
    SyntaxFactory.Trivia(
      SyntaxFactory.IfDirectiveTrivia(
        SyntaxFactory.PrefixUnaryExpression(
            SyntaxKind.LogicalNotExpression,
            SyntaxFactory.IdentifierName(ifDefIdentifier)),
            true, true, true))

let private elseDirective = 
    SyntaxFactory.Trivia(SyntaxFactory.ElseDirectiveTrivia(true, false))

let private endDirective =
    SyntaxFactory.Trivia(SyntaxFactory.EndIfDirectiveTrivia(true))

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

type SyntaxNode with
    
    member node.IsMarkedWithOptimizationTrivia = 
        match node with 
        | MarkedForOptimization _ -> true 
        | _ -> false

    member node.MakeLeadingIfDirective () =
        if node.GetLeadingTrivia() |> Seq.forall(fun t -> t.IsKind(SyntaxKind.SingleLineCommentTrivia)) then
            SyntaxFactory.TriviaList().Add(ifDirective).Add(SyntaxFactory.ElasticLineFeed).AddRange(node.GetLeadingTrivia())
        else
            node.GetLeadingTrivia().Add(ifDirective).Add(SyntaxFactory.ElasticCarriageReturnLineFeed)

    member node.MakeLeadingElseDirective() =
        SyntaxFactory.TriviaList(elseDirective, SyntaxFactory.ElasticLineFeed)

    member node.MakeTrailingEndDirective() =
        SyntaxFactory.TriviaList(endDirective).AddRange(node.GetTrailingTrivia())

    member node.MakeLeadingMarkComment() =
        let leading = node.GetLeadingTrivia()
        let whitespace = 
            leading 
            |> Seq.filter(fun t -> t.IsKind(SyntaxKind.WhitespaceTrivia))
            |> Seq.tryLast
        match whitespace with
        | None -> leading.Add(markerComment).Add(SyntaxFactory.ElasticCarriageReturnLineFeed)
        | Some w -> leading.Add(markerComment).Add(SyntaxFactory.ElasticCarriageReturnLineFeed).Add(w)


        