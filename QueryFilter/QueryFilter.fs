namespace QueryFilterLib

open System
open System.IO
open System.Collections.Generic
open System.Text.RegularExpressions
open System.Xml.Linq
open System.Linq

module QueryFilterService =
    
    open RecordTypes
    open SpecialTermSetEx
    open HyphenTermSetEx
    open CompoundTermRecordSetEx
    open QueryFilterRegex
    open DataTypes

    let atozlSeq = set {'a'..'z'}
    let atozuSeq = set {'A'..'Z'}
    let numeric  = set {'0'..'9';}
    let ops      = set [|'~';'(';')';'[';']';' ';'"';|]
    let dash     = set [|'-';|]
    let alphanumeric = atozlSeq + atozuSeq + numeric
    let allVaidCharsSet = alphanumeric + ops + dash

    // failwith strings
    let invalidStringsSet = 
        Set<string> 
            ["and and"; "or or"; "and filterby"; "or filterby"; "~ ";]

    let validQuery (queryString : string) = 
        let queryStringSet = queryString.ToCharArray() |> Set.ofArray
        let retval = queryStringSet
                     |> Seq.map(fun e -> allVaidCharsSet.Contains e)
                     |> Seq.contains(false)
        if retval then false
        else true

    let invalidChars (queryString : string) = 
        let queryStringSet = queryString.ToCharArray() |> Set.ofArray
        let charList = queryStringSet
                       |> Seq.filter(fun e -> not (allVaidCharsSet.Contains e))
                       |> Seq.toList
        charList

    let checkForValidChars (queryString : string) =
        let queryStringSet = queryString.ToCharArray() |> Set.ofArray
        let v1 = queryStringSet
                 |> Seq.map(fun e -> allVaidCharsSet.Contains e)
                 |> Seq.contains(false)
        let v2 = queryStringSet
                 |> Seq.filter(fun e -> not (allVaidCharsSet.Contains e))
                 |> Seq.toList
        (not v1,v2)

    let checkForValidForm (queryString : string) =
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

