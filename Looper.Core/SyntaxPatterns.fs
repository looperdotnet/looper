namespace Looper.Core

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    module SyntaxPatterns = 
        
        let (|SimpleLambdaExpression|_|) (node : SyntaxNode) =
            match node with
            | :? SimpleLambdaExpressionSyntax as node -> Some (node.Parameter, node.Body)
            | _ -> None

        let (|MemberAccessExpression|_|) (node : SyntaxNode) =
            match node with
            | :? MemberAccessExpressionSyntax as node -> Some (node.Name, node.Expression)
            | _ -> None

        let (|InvocationExpression|_|) (node : SyntaxNode) =
            match node with
            | :? InvocationExpressionSyntax as node -> 
                Some (node.Expression, node.ArgumentList.Arguments |> Seq.map (fun argSyntax -> argSyntax.Expression) |> Seq.toList)
            | _ -> None
            
        let (|IdentifierName|_|) (node : SyntaxNode) =
            match node with
            | :? IdentifierNameSyntax as node -> Some (node.Identifier.ValueText)
            | _ -> None

    