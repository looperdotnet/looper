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

let parseFor (identifier : string) : ForStatementSyntax = 
    parseStmtf "for (int __i__ = 0; __i__ < %s.Length; __i__++);" identifier

let parseForeach (item : string) (source : string) : ForEachStatementSyntax =
    parseStmtf "foreach(var %s in %s);" item source
    
let parseIndexer (identifier : string) : ExpressionSyntax =
    parseExprf "%s[__i__]" identifier

let throwNotImplemented : ThrowStatementSyntax =
    parseStmt "throw new System.NotImplementedException();" 
