namespace UBViews.LexParser

open System.Threading.Tasks

open UBViews.LexParser.Lexer;
open UBViews.LexParser.Ast;
open FSharp.Text.Lexing

open Evaluators;

module QueryMethods =

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

    let queryToStringAsync (qry : Query) : Task<string> =
        async {
            let retval = queryExpToString qry
            return retval
        } |> Async.StartAsTask

type ParserService() = 

    member this.ParseQuery (input:string) : Query list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        query

    member this.ParseQueryAsync (input:string) :Task<Query list> =
        async {
            let lexbuff = LexBuffer<char>.FromString(input)
            let query = Parser.start Lexer.tokenize lexbuff
            return query
        } |> Async.StartAsTask

    member this.ParseQueryStringToTermList (input:string) : string list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        let tl = Evaluators.queryToTermList(query.Head)
        tl

    member this.QueryExpressionToString (query: Query) : string =
        let queryExpressionStr = queryExpToString query
        queryExpressionStr

    member this.QueryToString(qry : Query) : string =
        let retval = queryExpToString qry
        retval

    member this.QueryToStringAsync(qry : Query) : Task<string> =
        async {
            let retval = queryExpToString qry
            return retval
        } |> Async.StartAsTask

type QueryService() =

    member this.QueryToString (query: Query) : string =
        let retval = queryExpToString query
        retval

    member this.QueryToStringAsync (query: Query) : Task<string> =
        async {
            let retval = queryExpToString query
            return retval
        } |> Async.StartAsTask
    
    member this.RunQuery (query: Query) =
        ()


