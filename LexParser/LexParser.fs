namespace LexParser

open Microsoft.FSharp.Collections
open System.Threading.Tasks

open UBViews.Query;
open UBViews.Query.Ast
open FSharp.Text.Lexing
open Evaluators


module LexParser =

    let parseQueryString queryString : Query list = 
        let lexbuf = LexBuffer<char>.FromString queryString
        let queryList = Parser.start Lexer.tokenize lexbuf
        queryList
        

    let parseQueryStringAsync (queryString: string) : Task<Query list> = 
        async {
            let lexbuf = LexBuffer<char>.FromString queryString
            let queryList = Parser.start Lexer.tokenize lexbuf
            return queryList
        } |> Async.StartAsTask

    let parseQueryStringToTermList (input:string) : string list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        let termList = Evaluators.queryToTermList(query.Head)
        termList
       

    let parseQueryStringToTermListAsync (input:string) : Task<string list> =
        async {
            let lexbuff = LexBuffer<char>.FromString(input)
            let query = Parser.start Lexer.tokenize lexbuff
            let termList = Evaluators.queryToTermList(query.Head)
            return termList
        } |> Async.StartAsTask

    /// Combines a list of query objects into a single one.
    /// e.g., And(Term("foreword"), Term("orvonton")) ->
    /// Or (NoOpQuery, And (Term "foreword", Term "orvonton"))
    let rec joinQueries (queries : Query list) =
        List.fold(fun acc query -> Or(acc, query)) NoOpQuery queries

    let rec joinQueriesAsync (queries : Query list) =
        async {
            return List.fold(fun acc query -> Or(acc, query)) NoOpQuery queries
        } |> Async.StartAsTask

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
