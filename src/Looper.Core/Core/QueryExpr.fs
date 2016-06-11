namespace Looper.Core
    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax

    type QueryExpr =
        // Producer
        | SourceIdentifierName of IdentifierNameSyntax
        // Intermediate 
        | Select of SimpleLambdaExpressionSyntax * QueryExpr
        | SelectMany of (ParameterSyntax * QueryExpr) * QueryExpr
        | Where of SimpleLambdaExpressionSyntax * QueryExpr
        // Consumer
        | ToArray of QueryExpr
        | ToList of QueryExpr
        | Sum of QueryExpr
        | Count of QueryExpr
        | Any of QueryExpr
        | First of QueryExpr

    and StmtQueryExpr = 
        | Assign of TypeSyntax * INamedTypeSymbol * SyntaxToken * QueryExpr
