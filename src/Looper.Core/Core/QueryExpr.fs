namespace Looper.Core
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    type Lambda = Lambda of ParameterSyntax * SyntaxNode
    type QueryExpr =
        // Producer
        | SourceIdentifierName of ITypeSymbol * IdentifierNameSyntax
        | SourceExpression of ExpressionSyntax
        // Intermediate 
        | Select of Lambda * QueryExpr
        | SelectMany of (ParameterSyntax * QueryExpr) * QueryExpr
        | Where of Lambda * QueryExpr
        // Consumer
        | ToArray of QueryExpr
        | ToList of QueryExpr
        | Sum of QueryExpr
        | Count of QueryExpr
        | Any of QueryExpr
        | First of QueryExpr

    and StmtQueryExpr = 
        | Assign of TypeSyntax * ITypeSymbol * SyntaxToken * QueryExpr
