module Looper.Core.SyntaxPatterns

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp.Syntax

    let (|IfStatement|_|) (node : SyntaxNode) = 
        match node with
        | :? IfStatementSyntax as node -> Some(node.Condition, node.Statement)
        | _ -> None

    let (|WhileStatement|_|) (node : SyntaxNode) =
        match node with
        | :? WhileStatementSyntax as node -> Some(node.Condition, node.Statement)
        | _ -> None

    let (|DoStatement|_|) (node : SyntaxNode) =
        match node with
        | :? DoStatementSyntax as node -> Some(node.Condition, node.Statement)
        | _ -> None

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

    let (|List|) (list : SyntaxList<'T>) = Seq.toList list

    let (|Block|_|) (node : SyntaxNode) =
        match node with
        | :? BlockSyntax as node -> Some(node.Statements)
        | _ -> None

    let (|LocalDeclarationStatement|_|) (node : SyntaxNode) =
        match node with
        | :? LocalDeclarationStatementSyntax as node -> Some(node.Modifiers, node.Declaration)
        | _ -> None

    let (|Expression|_|) (node : SyntaxNode) = 
        match node with 
        | :? ExpressionSyntax as node -> Some node
        | _ -> None

    let (|ExpressionStatement|_|) (node : SyntaxNode) =
        match node with
        | :? ExpressionStatementSyntax as node -> Some(node.Expression)
        | _ -> None

    let (|Assignment|_|) (node : SyntaxNode) =
        match node with
        | :? AssignmentExpressionSyntax as node -> Some(node.Left, node.Right)
        | _ -> None

    let (|VariableDeclaration|_|) (node : SyntaxNode) =
        match node with
        | :? VariableDeclarationSyntax as node -> Some(node.Type, node.Variables |>  Seq.toList)
        | _ -> None

    let (|VariableDeclarator|_|) (node : SyntaxNode) =
        match node with
        | :? VariableDeclaratorSyntax as node -> Some(node.Identifier, node.ArgumentList, node.Initializer)
        | _ -> None
    
    let (|EqualsValueClause|_|) (node : SyntaxNode) =
        match node with
        | :? EqualsValueClauseSyntax as node -> Some(node.Value)
        | _ -> None