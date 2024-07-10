namespace QueryFilter

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Xml.Linq
open System.Linq

open QueryFilterLib

module QueryFilterService =

    let atozlSeq = set {'a'..'z'}
    let atozuSeq = set {'A'..'Z'}
    let numeric  = set {'0'..'9';}
    let ops      = set [|'~';'(';')';'[';']';' ';'"';|]
    let minus    = set [|'-';|]
    let nbhyph   = set [|'‑';|]
    let alphanumeric = atozlSeq + atozuSeq + numeric
    let allVaidCharsSet = alphanumeric + ops + nbhyph

    // failwith strings
    let invalidStringsSet = 
        Set<string> 
            ["and and"; "or or"; "and filterby"; "or filterby";]

    // TODO: Create Regex and get match and capture
    let invalidCharStrings = set [| "/"; "\\"; "?"; "~ "; "/"; "\\"; "?"; "-";|]
        

    let containsInvalidChars (queryString : string) : bool =
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            true
        else
            invalidCharStrings
            |> Seq.map(fun c -> queryString.Contains(c))
            |> Seq.contains(true)

    let validQuery (queryString : string): bool = 
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            false
        else
            let queryStringSet = queryString.ToCharArray() |> Set.ofArray
            let retval = queryStringSet
                         |> Seq.map(fun e -> allVaidCharsSet.Contains e)
                         |> Seq.contains(false)
            if retval then false
            else true

    let invalidChars (queryString : string) : char list = 
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            []
        else
            let queryStringSet = queryString.ToCharArray() |> Set.ofArray
            let charList = queryStringSet
                           |> Seq.filter(fun e -> not (allVaidCharsSet.Contains e))
                           |> Seq.toList
            charList

    let checkForInvalidChars (queryString : string) : bool * string list =
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            (false, [])
        else
            let v1 = invalidCharStrings
                     |> Seq.map(fun c -> queryString.Contains(c))
                     |> Seq.contains(true)
            let v2 = invalidCharStrings
                     |> Seq.filter(fun c -> queryString.Contains(c))
                     |> Seq.toList
            (v1, v2)

    let checkForValidChars (queryString : string) : bool * char list =
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            (false, [])
        else
            let queryStringSet = queryString.ToCharArray() |> Set.ofArray
            let v1 = queryStringSet
                     |> Seq.map(fun e -> allVaidCharsSet.Contains e)
                     |> Seq.contains(false)
            let v2 = queryStringSet
                     |> Seq.filter(fun e -> not (allVaidCharsSet.Contains e))
                     |> Seq.toList
            (not v1,v2)

    let checkForValidForm (queryString : string) : bool * string list =
        let isNullOrEmptyQueryString = String.IsNullOrEmpty(queryString)
        if (isNullOrEmptyQueryString) then
            (false, [])
        else
            let v1 = invalidStringsSet
                     |> Seq.map(fun s -> queryString.Contains(s))
                     |> Seq.contains(true)
            let v2 = invalidStringsSet
                     |> Seq.filter(fun s -> queryString.Contains(s))
                     |> Seq.toList
            (not v1,v2)

    let checkQuery (queryString : string) =
        let lst = list.Empty
        let t1, t2 = checkForValidChars queryString
        let t3, t4 = checkForValidForm queryString
        [(t1, t2), (t3, t4)]

module QueryRecordsetService =

    open RecordTypes
    open SpecialTermSetEx
    open HyphenTermSetEx
    open CompoundTermRecordSetEx
    open QueryFilterRegex
    open DataTypes

    let openOrderedSet (setname:string) =
        let mutable ctset = Set.empty
        let strc = SpecialTermRecordSetEx
        match setname with
        | "SpecialTermRecordSetEx" ->
            ctset <- SpecialTermRecordSetEx
        | "HyphenTermSetEx" ->
            ctset <- HyphenTermRecordSetEx
        | "CompoundTermRecordSetEx" ->
            ctset <- CompoundTermRecordSetEx
        | _ -> failwith "no set by that name"

        let orderedSet = ctset.OrderBy(fun e -> e).ToArray()
        let os = Set.ofArray orderedSet
        os

