namespace QueryEngine

// see: https://stackoverflow.com/questions/65894470/converting-data-objects-f-fabulous-sql-net-pcl-and-a-web-api-client-library
// and see: https://github.com/TimLariviere/FabulousContacts

open System.Threading.Tasks

open LexParser;
open UBViews.Query;
open UBViews.Query.Ast
open FSharp.Text.Lexing
open Evaluators

open SimpleParse
open DataTypesEx
open LexParser

open QueryRepository
open PostingRepository

type TokenPostingListObject =
     { Query: Query
       QueryString: string 
       TokenSeqList: seq<TokenPositionEx> list } 

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

    member this.ParseQueryString (queryString: string) : Query list = 
        let lexbuf = LexBuffer<char>.FromString queryString
        let queryList = Parser.start Lexer.tokenize lexbuf
        queryList

    member this.ParseQueryStringAsync (queryString: string) : Task<Query list> = 
        async {
            let lexbuf = LexBuffer<char>.FromString queryString
            let queryList = Parser.start Lexer.tokenize lexbuf
            return queryList
        } |> Async.StartAsTask

    member this.ParseQueryStringToTermList (input:string) : string list =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        let termList = Evaluators.queryToTermList(query.Head)
        termList

    member this.ParseQueryStringToTermListAsync (input:string) : Task<string list> =
        async {
            let lexbuff = LexBuffer<char>.FromString(input)
            let query = Parser.start Lexer.tokenize lexbuff
            let termList = Evaluators.queryToTermList(query.Head)
            return termList
        } |> Async.StartAsTask

    member this.QueryToString(qry : Query) : string =
        let retval = queryExpToString qry
        retval

    member this.QueryToStringAsync(qry : Query) : Task<string> =
        async {
            let retval = queryExpToString qry
            return retval
        } |> Async.StartAsTask

type QueryService() = 

    //member this.ConvertTokenPostingListToObject (pl: TokenPostingList) : TokenPostingListObject =
    //    let tokenSeq = pl.BasePostingList
    //    { }

    member this.QueryToString (query: Query) : string =
        let queryString = queryToString query
        queryString

    member this.QueryToStringAsync (query: Query) : Task<string> =
        async {
            let! queryString = queryToStringAsync query |> Async.AwaitTask
            return queryString
        } |> Async.StartAsTask

    member this.ProcessTokenPostingList (input: string) (query: Query) (tpl: seq<TokenPositionEx>) =
        let queryResult = QueryProcessor.processTokenPostingSequence input query tpl
        queryResult

    member this.ProcessTokenPostingListAsync (input: string) (query: Query) (tpl: seq<TokenPositionEx>) =
        async {
            let queryResult = QueryProcessor.processTokenPostingSequence input query tpl
            return queryResult
        } |> Async.StartAsTask

    member this.RunQuery (dbpath: string) (query: Query) : SimpleEnumeratorsEx.TokenPostingList =
        let tpl = getIteratorEx query
        tpl

    member this.RunQueryAsync (dbpath: string) (query: Query) : Task<SimpleEnumeratorsEx.TokenPostingList> =
        async {
            setDatabasePath dbpath
            let tpl = getIteratorEx query
            return tpl
        } |> Async.StartAsTask
