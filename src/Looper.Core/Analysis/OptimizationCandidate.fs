namespace Looper.Core

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp.Syntax

type OptimizationCandidate = 
    { Invocation                     : InvocationExpressionSyntax
      ConsumerMethodName             : string
      ContainingStatement            : StatementSyntax option
      ContainingBlock                : BlockSyntax option
      IsInvariantOptimization        : bool
      NeedsRefactoring               : bool
      IsMarkedWithOptimizationTrivia : bool
      SemanticModel                  : SemanticModel }

    static member FromInvocation(model : SemanticModel, node : InvocationExpressionSyntax) = 
        let memberExpr = 
            match node.Expression with
            | :? MemberAccessExpressionSyntax as m -> m
            | _ -> invalidArg "node" "Expected member access"
        
        let consumerMethod = string memberExpr.Name

        let parentExpr = 
            node.Ancestors()
            |> Seq.takeWhile(fun n -> not(n :? BlockSyntax))
            |> Seq.filter(fun n -> n :? ExpressionSyntax)
            |> Seq.tryHead

        let parentStmt = node.GetParentStatement();

        let block = Some <| node.FirstAncestorOrSelf<BlockSyntax>()

        let isMarked = parentStmt |> Option.exists (fun s -> s.IsMarkedWithOptimizationTrivia) 
        let needsRefactoring = parentStmt |> Option.exists(fun s -> not(s :? LocalDeclarationStatementSyntax)) // TODO

        { Invocation = node
          ConsumerMethodName = consumerMethod
          ContainingStatement = parentStmt
          IsInvariantOptimization = parentExpr.IsNone
          ContainingBlock = block
          NeedsRefactoring = needsRefactoring
          IsMarkedWithOptimizationTrivia = isMarked
          SemanticModel = model
        }