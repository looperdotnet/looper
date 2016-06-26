[<AutoOpen>]
module Looper.Core.SyntaxUtils

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.CSharp

type SyntaxNode with
    member this.GetParentStatement () =
        this.AncestorsAndSelf()
        |> Seq.choose(function :? StatementSyntax as s -> Some s | _ -> None)
        |> Seq.tryHead

let toStr (node : SyntaxNode) = node.ToFullString()

let parseExpr (expr : string) : ExpressionSyntax = 
    SyntaxFactory.ParseExpression(expr)

let parseExprf fmt = Printf.ksprintf parseExpr fmt

let parseStmt<'T when 'T :> StatementSyntax> (stmt : string) : 'T = 
    SyntaxFactory.ParseStatement(stmt) :?> 'T

let parseStmtf fmt = Printf.ksprintf parseStmt fmt

let block (stmts : seq<StatementSyntax>) =
    SyntaxFactory.Block(stmts)

let parseFor (index : string) (source : string) : ForStatementSyntax = 
    parseStmtf "for (int %s = 0; %s < %s.Length; %s++);" index index source index

let parseForeach (item : string) (source : string) : ForEachStatementSyntax =
    parseStmtf "foreach (var %s in %s);" item source
    
let parseIndexer (identifier : string) (index : string) : ExpressionSyntax =
    parseExprf "%s[%s]" identifier index

let throwNotImplemented : ThrowStatementSyntax =
    parseStmt "throw new System.NotImplementedException();" 

let defaultOf (typ : string) =
    SyntaxFactory.DefaultExpression(SyntaxFactory.IdentifierName(typ))