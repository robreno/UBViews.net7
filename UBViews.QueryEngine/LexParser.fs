namespace UBViews.LexParser

open UBViews.LexParser.Lexer;
open UBViews.LexParser.Ast;
open FSharp.Text.Lexing

open Evaluators;

type ParserService() = 

    member this.ParseQuery (input:string) : Query list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        query

    member this.ParseQueryStringToTermList (input:string) : string list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        let tl = Evaluators.queryToTermList(query.Head)
        tl

    member this.QueryExpressionToString (query: Query) : string =
        let queryExpressionStr = queryExpToString query
        queryExpressionStr

module Say =

    let hello name =
        printfn "Hello %s" name

    let parseQueryString qry = 
        // let lexbuff = LexBuffer<char>.FromString(input)
        let lexbuf = LexBuffer<char>.FromString qry
        // UBViews.Query.Parser.start UBViews.Query.Lexer.tokenize lexbuff
        let query = Parser.start Lexer.tokenize lexbuf
        query

    /// Combines a list of query objects into a single one.
    /// e.g., And(Term("foreword"), Term("orvonton")) ->
    /// Or (NoOpQuery, And (Term "foreword", Term "orvonton"))
    let rec joinQueries (queries : Query list) =
        List.fold(fun acc query -> Or(acc, query)) NoOpQuery queries

    let queryToString (qry : Query) : string =
        let retval = queryExpToString qry
        retval


