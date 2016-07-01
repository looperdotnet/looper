module Looper.Core.QueryTransformer

    open Microsoft.CodeAnalysis
    open Microsoft.CodeAnalysis.CSharp
    open Microsoft.CodeAnalysis.CSharp.Syntax
    open Looper.Core.SyntaxPatterns

    let rec private (|LinqInvocation|_|) (checker : SymbolChecker) = 
        function 
        | InvocationExpression(MemberAccessExpression(IdentifierName name, expr) as memberExpr, args) -> 
            let symbol = checker.SemanticModel.GetSymbolInfo(memberExpr).Symbol
            match checker.IsLinqMethodType(symbol) with
            | Some _ -> Some(expr, name, args)
            | _ -> None
        | _ -> None

    and private producerMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option =
        match node with 
        | IdentifierName _ -> 
            match checker.SemanticModel.GetSymbolInfo(node).Symbol with
            | :? ILocalSymbol as symbol -> Some (SourceIdentifierName (symbol.Type, node :?> IdentifierNameSyntax))
            | _ -> None
        | Expression expr -> 
            checker.SemanticModel.GetTypeInfo(node).Type
            |> checker.IsIEnumerableType
            |> Option.map(fun sym -> SourceExpression (sym, expr))
        | _ -> None

    and private intermediateMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option = 
        match node with 
        | LinqInvocation checker (expr, "Select", [SimpleLambdaExpression (param, body)]) ->
            intermediateMatch expr checker |> Option.map (fun expr -> Select (Lambda (param, body), expr))
        | LinqInvocation checker (expr, "Where", [SimpleLambdaExpression (param, body)]) ->
            intermediateMatch expr checker |> Option.map (fun expr -> Where (Lambda (param, body), expr))
        | LinqInvocation checker (expr, "SelectMany", [SimpleLambdaExpression (param, NestedQueryExpr checker body)]) ->
            intermediateMatch expr checker |> Option.map (fun expr -> SelectMany ((param, body), expr))
        | _ -> producerMatch node checker

    and private consumerMatch (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option = 
        match node with 
        | LinqInvocation checker (expr, "Sum", []) ->
            intermediateMatch expr checker |> Option.map Sum 
        | LinqInvocation checker (expr, "Count", []) ->
            intermediateMatch expr checker |> Option.map Count 
        | LinqInvocation checker (expr, "First", []) ->
            intermediateMatch expr checker |> Option.map First 
        | LinqInvocation checker (expr, "Any", []) ->
            intermediateMatch expr checker |> Option.map Any
        | _ -> None

    and toQueryExpr (node : SyntaxNode) (checker : SymbolChecker) : QueryExpr option =
        consumerMatch node checker

    and (|QueryExpr|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        toQueryExpr node checker

    and (|NestedQueryExpr|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        intermediateMatch node checker

    and toStmtQueryExpr (node : SyntaxNode) (checker : SymbolChecker) : StmtQueryExpr option =
        match node with
        | LocalDeclarationStatement (_, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (QueryExpr checker expr))])) -> 
            let typeSymbol = checker.SemanticModel.GetSymbolInfo(typeSyntax).Symbol :?> INamedTypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None

    and (|StmtQueryExpr|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        toStmtQueryExpr node checker

    and (|NoConsumerQuery|_|) (checker : SymbolChecker) (node : SyntaxNode) =
        match intermediateMatch node checker with
        | Some _ as m when consumerMatch node checker = None -> m
        | _ -> None

    and (|StmtNoConsumerQuery|_|) (checker : SymbolChecker) (node : SyntaxNode)  =
        match node with
        | LocalDeclarationStatement (_, VariableDeclaration (typeSyntax, [VariableDeclarator (identifier, _, EqualsValueClause (NoConsumerQuery checker expr))])) -> 
            let typeSymbol = checker.SemanticModel.GetSymbolInfo(typeSyntax).Symbol :?> ITypeSymbol
            Some (Assign (typeSyntax, typeSymbol, identifier, expr))
        | _ -> None