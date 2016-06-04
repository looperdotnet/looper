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
        | Filter of SimpleLambdaExpressionSyntax * QueryExpr
        // Consumer
        | ToArray of QueryExpr
        | ToList of QueryExpr
        | Sum of QueryExpr
        | Count of QueryExpr