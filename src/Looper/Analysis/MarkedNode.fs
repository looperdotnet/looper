namespace Looper.Core

open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis

type InvalidNode = 
    | InvalidExpression of stmt: SyntaxNode 
    | NoConsumer of stmt: InvocationExpressionSyntax

type OptimizedNode =
    | MarkedWithDirective of stmt: StatementSyntax * generated: StatementSyntax * isStale: bool
    | MarkedWithComment of stmt: StatementSyntax