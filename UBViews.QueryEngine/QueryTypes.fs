[<AutoOpen>]
module UBViews.LexParser.Ast

type value =
    | Int of int
    | Float of float
    | String of string

type FilterValue = 
    | TOPID
    | DOCID
    | SEQID
    | PARID
    | SECID

type FilterID =
    | BookID of string
    | DocuID of string
    | SequID of string
    | ParaID of string
    | SectID of string

type Query =
    | Term of string
    | STerm of string
    | CTerm of string list
    | Phrase of string list
    | And of Query * Query
    | Or of Query * Query
    | SubQuery of Query
    | FilterBy of Query * FilterValue
    | NoOpQuery