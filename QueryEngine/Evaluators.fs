namespace QueryEngine

open UBViews.Query.Ast
open System.Text

module Evaluators =

    let toStringValue fv =
        match fv with
        | FilterValue.TOPID -> "TOPID"
        | FilterValue.DOCID -> "DOCID"
        | FilterValue.SEQID -> "SEQID"
        | FilterValue.PARID -> "PARID"
        | FilterValue.SECID -> "SECID"

    let toFilterValue s =
        match s with
        | _ when s = "topid" || s = "TOPID" 
            -> FilterValue.TOPID
        | _ when s = "docid" || s = "DOCID" 
            -> FilterValue.DOCID
        | _ when s = "seqid" || s = "SEQID" 
            -> FilterValue.SEQID
        | _ when s = "parid" || s = "PARID" 
            -> FilterValue.PARID
        | _ when s = "secid" || s = "SECID" 
            -> FilterValue.SECID
        | _ -> failwith "Unknown FilterType"

    let toProximityValue fv =
       match fv with
       | FilterValue.TOPID -> "book"
       | FilterValue.DOCID -> "document"
       | FilterValue.SECID -> "section"
       | FilterValue.SEQID -> "sequence"
       | FilterValue.PARID -> "paragraph"
    
    let rec eval (q : Query) : string =
        match q with
        | Term(term) -> "Term(\"" + term + "\")"
        | STerm(term) -> "STerm(\"" + term + "\")"
        | CTerm(cterm) -> 
            let newStringList =
                cterm
                |> List.rev
                |> List.map(fun s -> let ns = s
                                     ns)
            "CTerm(\"" + newStringList.ToString() + "\")"
        | Phrase(phrase) -> 
            let newStringList = 
                phrase
                |> List.rev 
                |> List.map (fun s -> let ns = s
                                      ns)
            "Phrase(" + newStringList.ToString() + ")"
        | And(x, y)   -> "And(" + eval(x) + "," + eval(y) + ")"
        | Or(x, y)    -> "Or(" + eval(x) + "," + eval(y) + ")"
        | SubQuery(q) -> "SubQuery(" + eval(q) + ")"
        | FilterBy(q, f) -> 
            let results =
                    match q with
                    | Term(term)     -> eval(q) + " FilderBy " + toStringValue f
                    | STerm(term)    -> eval(q) + " FilderBy " + toStringValue f
                    | CTerm(cterm)   -> eval(q) + " FilderBy " + toStringValue f
                    | Phrase(phrase) -> eval(q) + " FilderBy " + toStringValue f
                    | And(x, y)      -> eval(q) + " FilderBy " + toStringValue f
                    | Or(x, y)       -> eval(q) + " FilderBy " + toStringValue f
                    | _ -> eval(q) + " FilderBy " + toStringValue f
            results
        | NoOpQuery   -> string []

    let rec evalEx (q : Query) : Query list =
        match q with
        | Term(term) -> [Term(term)]
        | STerm(term) -> [STerm(term)]
        | CTerm(cterm) ->
            let result = 
                cterm
                |> List.rev 
                |> List.map (fun s -> Term(s))
            result
        | Phrase(phrase) -> 
            let result = 
                phrase
                |> List.rev 
                |> List.map (fun s -> Term(s))
            result
        | And(x, y)   -> [And(Term(evalEx(x).ToString()), Term(evalEx(y).ToString()))]
        | Or(x, y)    -> [Or(Term(evalEx(x).ToString()), Term(evalEx(y).ToString()))]
        | SubQuery(q) -> evalEx(q)
        | FilterBy(q, f) -> 
            let results = 
                match q with
                | Term(term)     -> [FilterBy(Term(term), f)]
                | STerm(term)    -> [FilterBy(STerm(term), f)]
                | CTerm(cterm)   -> [FilterBy(CTerm(cterm), f)]
                | Phrase(phrase) -> [FilterBy(Phrase(phrase), f)]
                | And(x,y)       -> [FilterBy(And(Term(evalEx(x).ToString()),Term(evalEx(y).ToString())), f)]
                | Or(x,y)        -> [FilterBy(Or(Term(evalEx(x).ToString()),Term(evalEx(y).ToString())), f)]
                | _              -> evalEx(q)
            results
        | NoOpQuery   -> []

    let rec queryToTermList (query: Query) =
       match query with
       | Term(term)     -> term :: []
       | STerm(term)    -> term :: []
       | CTerm(cterm)   -> let mutable sb = new StringBuilder()
                           cterm |> List.rev |> List.iter (fun t -> sb.Append(t + " ") |> ignore)
                           let term = sb.ToString().Trim()
                           term :: []
       | Phrase(phrase) -> let mutable sb = new StringBuilder()
                           phrase |> List.rev |> List.iter (fun t -> sb.Append(t + " ") |> ignore)
                           let phraseStr = sb.ToString().Trim()
                           phraseStr :: []
       | And(x, y)      -> let term1 = queryToTermList(x) 
                           let term2 = queryToTermList(y)
                           let terms = term1 @ term2
                           terms
       | Or(x, y)       -> let term1 = queryToTermList(x) 
                           let term2 = queryToTermList(y)
                           let terms = term1 @ term2
                           terms
       | SubQuery(q)    -> queryToTermList(q)
       | FilterBy(q, f) -> queryToTermList(q)
       | NoOpQuery      -> ["NoOp"]

