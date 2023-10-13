namespace QueryEngine

open System
open System.Text
open System.Linq
open System.Xml.Linq
open System.Threading.Tasks
open System.Text.RegularExpressions

open UBViews.Query.Ast
open DataTypesEx

//open PostingRepository
//open QueryRepository

module QueryProcessor =

    /// Utility Methods
    let toStringValue fv =
       match fv with
       | FilterValue.TOPID -> "TOPID"
       | FilterValue.DOCID -> "DOCID"
       | FilterValue.SEQID -> "SEQID"
       | FilterValue.PARID -> "PARID"
       | FilterValue.SECID -> "SECID"

    let toFilterId fv =
       match fv with
       | FilterValue.TOPID -> "TOPID"
       | FilterValue.DOCID -> "DOCID"
       | FilterValue.SEQID -> "SEQID"
       | FilterValue.PARID -> "PARID"
       | FilterValue.SECID -> "SECID"

    let toProximityValue fv =
       match fv with
       | FilterValue.TOPID -> "book"
       | FilterValue.DOCID -> "document"
       | FilterValue.SECID -> "section"
       | FilterValue.SEQID -> "sequence"
       | FilterValue.PARID -> "paragraph"

    let joinSymbol = "+"

    let mutable _dbpath = ""
    let setDatabasePath (dbpath: string) =
        _dbpath <- dbpath

    let rec queryExpToString (query: Query) : string =
        let qs = query.ToString()
        match query with
        | Term(term)   -> "Term(\"" + term + "\")"
        | STerm(term)  -> "STerm(\"" + term + "\")"
        | CTerm(cterm) -> 
            let newStringList =
                cterm
                |> List.rev
                |> List.map(fun s -> let ns = s
                                     ns)
            "CTerm(\"" + newStringList.ToString() + "\")"
        | Phrase(phrase) -> 
            let mutable ns: string = String.Empty
            let newStringList = 
                phrase
                |> List.map (fun w -> ns <- ns + "\"" + w + "\"; "
                                      ns)
            ns <- ns.Substring(0, ns.Length-2)
            let qes = "Phrase [" + ns + "]"
            let isExact = qs.Equals(qes)
            qes
        | And(x, y)   -> "And(" + queryExpToString(x) + "," + queryExpToString(y) + ")"
        | Or(x, y)    -> "Or(" + queryExpToString(x) + "," + queryExpToString(y) + ")"
        | SubQuery(q) -> "SubQuery(" + queryExpToString(q) + ")"
        | FilterBy(q, f) ->
            let results =
                    match q with
                    | Term(term)     -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | STerm(term)    -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | CTerm(cterm)   -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | Phrase(phrase) -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | And(x, y)      -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | Or(x, y)       -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
                    | _              -> "FilterBy(" + queryExpToString(q) + "," + toStringValue f + ")"
            results
        | NoOpQuery   -> string []

    let termFromList(tl: string list) =
        String.concat " " <| List.map string (List.rev tl)

    let rec queryToTermListStrings (query: Query) =
        match query with
       | Term(term)     -> term
       | STerm(term)    -> term
       | CTerm(cterm)   -> let s = termFromList cterm 
                           let ct = "[" + s + "]"
                           ct
       | Phrase(phrase) -> let s = termFromList phrase
                           let pt = "{" + s + "}"
                           pt
       | And(x, y)      -> let term1 = queryToTermListStrings(x) 
                           let term2 = queryToTermListStrings(y)
                           let terms = term1 + "|" + term2
                           terms
       | Or(x, y)       -> let term1 = queryToTermListStrings(x) 
                           let term2 = queryToTermListStrings(y)
                           let terms = term1 + "|" + term2
                           terms
       | SubQuery(q)    -> queryToTermListStrings(q)
       | FilterBy(q, f) -> queryToTermListStrings(q)
       | NoOpQuery      -> "NoOp"

    let rec queryToTermList (query: Query) =
       match query with
       | Term(term)     -> term :: []
       | STerm(term)    -> term :: []
       | CTerm(cterm)   -> let mutable sb = new StringBuilder()
                           cterm |> List.rev |> List.iter (fun t -> sb.Append(t + " ") |> ignore)
                           let term = sb.ToString().Trim()
                           term :: []
       | Phrase(phrase) -> let mutable nl: string list = [] 
                           phrase
                           |> List.iter (fun w -> nl <- w :: nl)
                           nl
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

    let rec evaluateQueryType query : string = 
        match query with
        | Term(term)     -> "Term"
        | STerm(sterm)   -> "STerm"
        | CTerm(cterm)   -> "CTerm"
        | Phrase(phrase) -> "Phrase"
        | And(x, y)      -> "And" + joinSymbol 
                                  + evaluateQueryType(x) 
                                  + evaluateQueryType(y)
        | Or(x, y)       -> "Or" + joinSymbol 
                                 + evaluateQueryType(x)
                                 + evaluateQueryType(y)
        | SubQuery(q)    -> "SubQuery" + joinSymbol
        | FilterBy(q, f) -> let results =
                                match q with
                                | Term(term)     -> "FilterBy+Term"
                                | STerm(term)    -> "FilterBy+STerm"
                                | CTerm(cterm)   -> "FilterBy+CTerm"
                                | Phrase(phrase) -> "FilterBy+Phrase"
                                | And(x, y)      -> "FilterBy" + joinSymbol 
                                                               + evaluateQueryType(x) 
                                                               + evaluateQueryType(y)
                                | Or(x, y)       -> "FilterBy" + joinSymbol 
                                                               + evaluateQueryType(q) 
                                                               + evaluateQueryType(y)
                                | _ -> "Unknown_FilterBy" + joinSymbol + evaluateQueryType(q)
                            results
        | NoOpQuery      -> "NoOpQuery"

    let rec evaluateQuery (queryElement: XElement) (query: Query) =
        let joinSymbol = "+"
        match query with
        | Term(term) -> let att = queryElement.Attribute("type")
                        if att = null then
                            queryElement.SetAttributeValue("type", "Term")
                        else
                            let v = queryElement.Attribute("type").Value
                            queryElement.SetAttributeValue("type", v + joinSymbol + "STerm")
                        let termList = queryToTermList(Term(term))
                        queryElement.SetAttributeValue("terms", termList)
                        queryElement.SetAttributeValue("proximity", "book")
                        queryElement
        | STerm(term) -> let att = queryElement.Attribute("type")
                         if att = null then 
                            queryElement.SetAttributeValue("type", "STerm")
                         else 
                            let v = queryElement.Attribute("type").Value
                            queryElement.SetAttributeValue("type", v + joinSymbol + "STerm")
                         let termList = queryToTermList(STerm(term))
                         let opt = async { let! retval = QueryEngine.PostingRepository.getTokenStemAsync _dbpath term |> Async.AwaitTask 
                                           return retval
                                      } |> Async.StartAsTask
                         if (opt.Result.IsNone) then
                            // failwith here and return empty TokenPostingList
                            failwithf "Invalid Token -> [%s] in query." term
                         let sterm = opt.Result.Value.Stemmed
                         queryElement.SetAttributeValue("terms", termList)
                         queryElement.SetAttributeValue("stemmed", sterm)
                         queryElement.SetAttributeValue("proximity", "book")
                         queryElement
        | CTerm(cterm) -> let att = queryElement.Attribute("type")
                          if att = null then 
                            queryElement.SetAttributeValue("type", "CTerm")
                          else 
                            let v = queryElement.Attribute("type").Value
                            queryElement.SetAttributeValue("type", v + joinSymbol + "CTerm" )
                          let termList = queryToTermList(CTerm(cterm))
                          queryElement.SetAttributeValue("terms", termList)
                          queryElement.SetAttributeValue("proximity", "paragraph")
                          queryElement
        | Phrase(phrase) -> let att = queryElement.Attribute("type")
                            if att = null then 
                                queryElement.SetAttributeValue("type", "Phrase")
                            else 
                                let v = queryElement.Attribute("type").Value
                                queryElement.SetAttributeValue("type", v + joinSymbol + "Phrase" )
                            let mutable ns: string = String.Empty
                            phrase
                            |> List.iter (fun w -> ns <- ns + w + "; ")
                            ns <- "[" + ns.Substring(0, ns.Length-2) + "]"
                            queryElement.SetAttributeValue("terms", ns)
                            queryElement.SetAttributeValue("proximity", "paragraph")
                            queryElement
        | And(x, y)      -> let att = queryElement.Attribute("type")
                            if att = null then 
                               queryElement.SetAttributeValue("type", "And")
                            else 
                               let v = queryElement.Attribute("type").Value
                               queryElement.SetAttributeValue("type", v + joinSymbol + "And" )
                            let termList = queryToTermList(And(x, y))
                            queryElement.SetAttributeValue("terms", termList)
                            queryElement.SetAttributeValue("proximity", "paragraph")
                            queryElement
        | Or(x, y)       -> let att = queryElement.Attribute("type")
                            if att = null then 
                               queryElement.SetAttributeValue("type", "Or")
                            else 
                               let v = queryElement.Attribute("type").Value
                               queryElement.SetAttributeValue("type", v + joinSymbol + "Or" )
                            let termList = queryToTermList(Or(x, y))
                            queryElement.SetAttributeValue("terms", termList)
                            queryElement.SetAttributeValue("proximity", "book")
                            queryElement
        | SubQuery(q)    -> evaluateQuery queryElement q 
        | FilterBy(q, f) -> let att = queryElement.Attribute("type")
                            if att = null then 
                               queryElement.SetAttributeValue("type", "FilterBy")
                            else 
                               let v = queryElement.Attribute("type").Value
                               queryElement.SetAttributeValue("type", v + joinSymbol + "FilterBy" )
                            let fb = XElement("FilterBy", [])
                            fb.SetAttributeValue("filterId", toFilterId f)
                            let qrueryval = evaluateQuery queryElement q
                            fb.Add(qrueryval)
                            //queryElement.Add(fb)
                            queryElement.SetAttributeValue("filterId", toFilterId f)
                            queryElement.SetAttributeValue("proximity", toProximityValue f)
                            queryElement
        | NoOpQuery      -> XElement("NoOpQuery", [])

    let getQueryType queryString =    
        let rgxFilterBy = new Regex("filterb")
        let rgxAnd = new Regex(@"\sand\s")
        let rgxOr  = new Regex(@"\sor\s")
        let rgxPhrase = new Regex(@"^\""")

        let mutable queryType = String.Empty

        let filterByOp = rgxFilterBy.Match(queryString).Success
        let andOp = rgxAnd.Match(queryString).Success
        let orOp = rgxOr.Match(queryString).Success
        let phraseOp = rgxPhrase.Match(queryString).Success

        if (andOp && filterByOp) then
            queryType <- "FilterBy+And"
        else if (orOp && filterByOp) then
            queryType <- "Filterby+Or"
        else if (andOp && not filterByOp) then
            queryType <- "And"
        else if (orOp && not filterByOp) then
            queryType <- "Or"
        else if (phraseOp) then
            queryType <- "Phrase"
        else
            queryType <- "Unknown"

        queryType

    let reverseQueryString (queryString : string) (queryType : string) =
        let rgxFilterBy = new Regex("filterby")
        let rgxAnd = new Regex(@"\sand\s")
        let rgxOr  = new Regex(@"\sor\s")
        let mutable reverseQueryString = ""
        let mutable filterByOp = ""
        let mutable baseQuery = ""
        let len = queryString.Length

        if (queryType = "And") then
            let m = rgxAnd.Match(queryString)
            let terms = queryString.Split(' ')
            reverseQueryString <- terms[2] + " and " + terms[0]
        else if (queryType = "FilterBy+And") then
            let m = rgxFilterBy.Match(queryString)
            if (m.Success) then
                filterByOp <- queryString.Substring(m.Index, queryString.Length - m.Index)
                baseQuery <- queryString.Substring(0, m.Index - 1)
                let terms = baseQuery.Split(' ')
                reverseQueryString <- terms[2] + " and " + terms[0] + " " + filterByOp

        reverseQueryString

    let makeTokenPostingList (tokenExLst: List<TokenPositionEx>) : TokenPostingList =
        let mutable tpl = new TokenPostingList([])
        let newSeq = Seq.ofList tokenExLst
        tpl <- SimpleEnumeratorsEx.TokenPostingList(newSeq)
        tpl

    let getPhraseString (terms: string) =
        let mutable phraseStr = String.Empty
        let rgx = new Regex("(?<=\{)[^}]*(?=\})")
        let m = rgx.Match(terms)
        if m.Success then
            phraseStr <- m.Value
        phraseStr

    let getCTermString (terms: string) =
        let mutable phraseStr = String.Empty
        let rgx = new Regex("(?<=\[)[^}]*(?=\])")
        let m = rgx.Match(terms)
        if m.Success then
            phraseStr <- m.Value
        phraseStr

    let processTokenPostingSequence (dbPath: string) (input: string) (query: Query) (tokenPositionSeq: seq<TokenPositionEx>) =
        setDatabasePath dbPath
        let _queryType = evaluateQueryType query
        let _queryExpStr = queryExpToString query
        let _queryStrElm = XElement("QueryString", [XText(input)])
        let _reverseQueryStr = reverseQueryString input _queryType
        let _reverseQueryStrElm = XElement("ReverseQueryString", [XText(_reverseQueryStr)])
        let _queryExprElm = XElement("QueryExpression", [XText(_queryExpStr)])

        let mutable _queryResult = XElement("QueryResult", [])
        _queryResult.SetAttributeValue("id", "0")
        _queryResult.SetAttributeValue("locationCount", 0)
        _queryResult.Add(_queryStrElm)
        if (_reverseQueryStr <> "") then
            _queryResult.Add(_reverseQueryStrElm)
        _queryResult.Add(_queryExprElm)
            
        evaluateQuery _queryResult query |> ignore
        _queryResult.SetAttributeValue("type", _queryType)
        let queryType = _queryResult.Attribute("type").Value
            
            
        let mutable _length = 0
        let mutable _isPhrase = false
        let mutable _phraseString = String.Empty
        if queryType.Contains("Phrase") then
            let trms = _queryResult.Attribute("terms").Value
            _phraseString <- getPhraseString trms
            _length <-  _phraseString.Length
            _isPhrase <- true

        let mutable _isCTerm = false 
        let mutable _ctermString = String.Empty
        if queryType.Contains("CTerm") then
            let trms = _queryResult.Attribute("terms").Value
            _ctermString <- getCTermString trms
            _length <-  _ctermString.Length
            _isCTerm <- true

        let mutable _queryLocs = XElement("QueryLocations", 
                                        [XAttribute("count", 0)])
    
        let groups = tokenPositionSeq 
                        |> Seq.groupBy(fun t -> t.DocumentID, t.SequenceID, t.ParagraphID)

        let grps = groups.DefaultIfEmpty();

        let mutable _qryLocList = []

        grps
        |> Seq.iter(fun g -> let _tpl, _occs = g
                             let _did, _sid, _pid = _tpl
                             let _id = _did.ToString("0") 
                                        + "." + 
                                        _sid.ToString("0")
                             
                             let mutable _occList = XElement("TermOccurrenceList", [XAttribute("count", 0)])
                             let mutable _qryLoc = XElement("QueryLocation", 
                                                                [XAttribute("id", _id)
                                                                 XAttribute("pid", _pid)])
                             _occs
                             |> Seq.sortBy(fun o -> o.TermPosition)
                             |> Seq.iteri(fun i t -> let _pid  = t.ParagraphID
                                                     let _did  = Int32.Parse(t.DocumentID.ToString())
                                                     let _sid  = Int32.Parse(t.SequenceID.ToString())
                                                     let _dpo  = t.DocumentPosition
                                                     let _tpo  = t.TermPosition
                                                     let _term = t.Token

                                                     let _occ = XElement("TermOccurrence", [])

                                                     _occ.SetAttributeValue("term", _term)
                                                     _occ.SetAttributeValue("docId", _did)
                                                     _occ.SetAttributeValue("seqId", _sid)
                                                     _occ.SetAttributeValue("dpoId", _dpo)
                                                     _occ.SetAttributeValue("tpoId", _tpo)
                                                     _occ.SetAttributeValue("len", _term.Length)

                                                     if _isPhrase && _phraseString.Contains(_term) then
                                                        _occ.SetAttributeValue("term", _phraseString)
                                                        _occ.SetAttributeValue("len", _length)

                                                     if _isCTerm && _ctermString.Contains(_term) then
                                                        _occ.SetAttributeValue("term", _ctermString)
                                                        _occ.SetAttributeValue("len", _length)

                                                     _occList.Add(_occ)
                                                     _occList.SetAttributeValue("count", _occList.Elements().Count())
                                        )
                             _qryLoc.Add(_occList)
                             _qryLocList <- _qryLoc :: _qryLocList
                   )
        _qryLocList
        |> Seq.iter(fun l -> _queryLocs.AddFirst(l))
        let _hits = _queryLocs.Descendants("QueryLocation").Count()
        _queryLocs.SetAttributeValue("count", _hits)
        _queryResult.Add(_queryLocs)
        _queryResult.SetAttributeValue("locationCount", _hits)
        _queryResult

    let processTokenPostingSequenceAsync (dbPath: string) (input: string) (query: Query) (tokenPositionSeq: seq<TokenPositionEx>) : Task<XElement> =
        async {
            setDatabasePath dbPath
            let _queryType = evaluateQueryType query
            let _queryExpStr = queryExpToString query
            let _queryStrElm = XElement("QueryString", [XText(input)])
            let _reverseQueryStr = reverseQueryString input _queryType
            let _reverseQueryStrElm = XElement("ReverseQueryString", [XText(_reverseQueryStr)])
            let _queryExprElm = XElement("QueryExpression", [XText(_queryExpStr)])

            let mutable _queryResult = XElement("QueryResult", [])
            _queryResult.SetAttributeValue("id", "0")
            _queryResult.SetAttributeValue("locationCount", 0)
            _queryResult.Add(_queryStrElm)
            if (_reverseQueryStr <> "") then
                _queryResult.Add(_reverseQueryStrElm)
            _queryResult.Add(_queryExprElm)
            
            evaluateQuery _queryResult query |> ignore
            _queryResult.SetAttributeValue("type", _queryType)
            let queryType = _queryResult.Attribute("type").Value
            
            
            let mutable _length = 0
            let mutable _isPhrase = false
            let mutable _phraseString = String.Empty
            if queryType.Contains("Phrase") then
                let trms = _queryResult.Attribute("terms").Value
                _phraseString <- getPhraseString trms
                _length <-  _phraseString.Length
                _isPhrase <- true

            let mutable _isCTerm = false 
            let mutable _ctermString = String.Empty
            if queryType.Contains("CTerm") then
                let trms = _queryResult.Attribute("terms").Value
                _ctermString <- getCTermString trms
                _length <-  _ctermString.Length
                _isCTerm <- true

            let mutable _queryLocs = XElement("QueryLocations", 
                                            [XAttribute("count", 0)])
    
            let groups = tokenPositionSeq 
                         |> Seq.groupBy(fun t -> t.DocumentID, t.SequenceID, t.ParagraphID)

            let grps = groups.DefaultIfEmpty();

            let mutable _qryLocList = []

            grps
            |> Seq.iter(fun g -> let _tpl, _occs = g
                                 let _did, _sid, _pid = _tpl
                                 let _id = _did.ToString("0") 
                                           + "." + 
                                           _sid.ToString("0")
                             
                                 let mutable _occList = XElement("TermOccurrenceList", [XAttribute("count", 0)])
                                 let mutable _qryLoc = XElement("QueryLocation", 
                                                                [XAttribute("id", _id)
                                                                 XAttribute("pid", _pid)])
                                 _occs
                                 |> Seq.sortBy(fun o -> o.TermPosition)
                                 |> Seq.iteri(fun i t -> let _pid  = t.ParagraphID
                                                         let _did  = Int32.Parse(t.DocumentID.ToString())
                                                         let _sid  = Int32.Parse(t.SequenceID.ToString())
                                                         let _dpo  = t.DocumentPosition
                                                         let _tpo  = t.TermPosition
                                                         let _term = t.Token

                                                         let _occ = XElement("TermOccurrence", [])

                                                         _occ.SetAttributeValue("term", _term)
                                                         _occ.SetAttributeValue("docId", _did)
                                                         _occ.SetAttributeValue("seqId", _sid)
                                                         _occ.SetAttributeValue("dpoId", _dpo)
                                                         _occ.SetAttributeValue("tpoId", _tpo)
                                                         _occ.SetAttributeValue("len", _term.Length)

                                                         if _isPhrase && _phraseString.Contains(_term) then
                                                            _occ.SetAttributeValue("term", _phraseString)
                                                            _occ.SetAttributeValue("len", _length)

                                                         if _isCTerm && _ctermString.Contains(_term) then
                                                            _occ.SetAttributeValue("term", _ctermString)
                                                            _occ.SetAttributeValue("len", _length)

                                                         _occList.Add(_occ)
                                                         _occList.SetAttributeValue("count", _occList.Elements().Count())
                                             )
                                 _qryLoc.Add(_occList)
                                 _qryLocList <- _qryLoc :: _qryLocList
                       )
            _qryLocList
            |> Seq.iter(fun l -> _queryLocs.AddFirst(l))
            let _hits = _queryLocs.Descendants("QueryLocation").Count()
            _queryLocs.SetAttributeValue("count", _hits)
            _queryResult.Add(_queryLocs)
            _queryResult.SetAttributeValue("locationCount", _hits)
            return _queryResult
        } |> Async.StartAsTask


