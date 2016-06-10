namespace Looper.Core

open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis

type InvalidNode = 
    | InvalidExpression of node: SyntaxNode * trivia: SyntaxTrivia
    | NoConsumer of stmt: InvocationExpressionSyntax

type OptimizedNode =
    | MarkedWithDirective of stmt: StatementSyntax * generated: StatementSyntax * isStale: bool
    | MarkedWithComment of stmt: StatementSyntax