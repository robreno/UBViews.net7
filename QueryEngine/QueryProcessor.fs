namespace QueryEngine

open System
open System.Text
open System.Linq
open System.Xml.Linq
open System.Threading.Tasks
open System.Text.RegularExpressions

open UBViews.Query.Ast
open DataTypesEx
open StringPaths
open UtilFuncs


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

    let mutable _dbpath = ""
    let setDatabasePath (dbpath: string) =
        _dbpath <- dbpath

    let rec queryExpToString (query: Query) : string =
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
            let newStringList = 
                phrase
                |> List.rev 
                |> List.map (fun s -> let ns = s
                                      ns)
            "Phrase(" + newStringList.ToString() + ")"
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
                            let phraseParts =
                                phrase
                                |> List.rev 
                                |> List.toArray
                            let termList = queryToTermList(Phrase(phrase))
                            queryElement.SetAttributeValue("terms", termList)
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
        | Or(x, y)       -> XElement("QueryResult", [])
        | SubQuery(q)    -> XElement("QueryResult", [])
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

    // TODO: Save to Database so move out from here
    let saveQuery (queryFilePath: string) (queryResult : XElement) =
        let _xdoc = XDocument.Load(queryFilePath)
        let _root = _xdoc.Root
        let mutable _cnt = Int32.Parse(_root.Attribute("count").Value)
        queryResult.SetAttributeValue("id", _cnt + 1)
        let _filterType = queryResult.Attribute("type").Value
        let _terms = queryResult.Attribute("terms").Value
        let _proximity = queryResult.Attribute("proximity").Value
        let _queryResult = _root.Descendants("QueryResult").Where(fun q -> q.Attribute("type").Value = _filterType &&
                                                                           q.Attribute("terms").Value = _terms &&
                                                                           q.Attribute("proximity").Value = _proximity)
                                                           .FirstOrDefault()
        if _queryResult = null then
            _root.Add(query)
            _cnt <- _cnt + 1
            _root.SetAttributeValue("count", _cnt)
            _root.Save(queryFilePath)

    let getQueryType queryString =    
        let rgxFilterBy = new Regex("filterb")
        let rgxAnd = new Regex(@"\sand\s")
        let rgxOr  = new Regex(@"\sor\s")
        let mutable queryType = String.Empty

        let filterByOp = rgxFilterBy.Match(queryString).Success
        let andOp = rgxAnd.Match(queryString).Success
        let orOp = rgxOr.Match(queryString).Success
        if (andOp && filterByOp) then
            queryType <- "FilterBy+And"
        else if (orOp && filterByOp) then
            queryType <- "Filterby+Or"
        else if (andOp && not filterByOp) then
            queryType <- "And"
        else if (orOp && not filterByOp) then
            queryType <- "Or"
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

    // TODO: Save to Database so move out from here
    let processTokenPostingSequence (dbPath: string) (queryString: string) (query: Query) (tokenPositionSeq: seq<TokenPositionEx>) =
        setDatabasePath dbPath
        let _queryExpStr = queryExpToString query
        let _queryType = getQueryType queryString
        let _queryStrElm = XElement("QueryString", [XText(queryString)])
        let _reverseQueryStr = reverseQueryString queryString _queryType
        let _reverseQueryElm = XElement("ReverseQueryString", [XText(_reverseQueryStr)])
        let _queryExprElm = XElement("QueryExpression", [XText(_queryExpStr)])
        
        let mutable _queryResultElm = XElement("QueryResult", [])
        _queryResultElm.SetAttributeValue("id", "0")
        _queryResultElm.SetAttributeValue("locationCount", 0)
        _queryResultElm.Add(_queryStrElm)
        if (_reverseQueryStr <> "") then
            _queryResultElm.Add(_reverseQueryElm)
        _queryResultElm.Add(_queryExprElm)

        evaluateQuery _queryResultElm query |> ignore
        let mutable _expandedQueryStr = String.Empty
        let _proximity = _queryResultElm.Attribute("proximity").Value;
        
        if (_queryType.Equals("And") && _proximity.Equals("paragraph")) then 
            _expandedQueryStr <- queryString + " filterby parid"
            _reverseQueryElm.AddAfterSelf(XElement("DefaultQueryString", [XText(_expandedQueryStr)]))

        let mutable _length = 0
        let mutable _queryString = String.Empty
        let mutable _isPhraseOrCTerm = false
        if _queryType.Equals("Phrase") || _queryType.Equals("CTerm") then
            _queryString <- _queryResultElm.Descendants("QueryString").FirstOrDefault().Value
            _queryString <- _queryString.Substring(1, _queryString.Length - 2)
            _length <- _queryString.Length
            _isPhraseOrCTerm <- true

        let mutable _queryLocs = XElement("QueryLocations", 
                                        [XAttribute("count", 0)])
    
        let groups = tokenPositionSeq 
                     |> Seq.groupBy(fun t -> t.DocumentID, t.SequenceID, t.ParagraphID)

        let mutable _qryLocList = []
        groups
        |> Seq.iter(fun g -> let _tpl, _occs = g
                             let _did, _sid, _pid = _tpl
                             let _id = _did.ToString("0") 
                                       + "." + 
                                       _sid.ToString("0")
                             
                             let termList = queryToTermList(query)
                             let mutable _occList = XElement("TermOccurrenceList", [XAttribute("count", 0)])
                             let mutable _qryLoc = XElement("QueryLocation", 
                                                            [XAttribute("id", _id)
                                                             XAttribute("pid", _pid)
                                                             (*XAttribute("termList", termList)*)])
                             _occs
                             |> Seq.sortBy(fun o -> o.TermPosition)
                             |> Seq.iteri(fun i t -> let _pid  = t.ParagraphID
                                                     let _did  = Int32.Parse(t.DocumentID.ToString())
                                                     let _sid  = Int32.Parse(t.SequenceID.ToString())
                                                     let _dpo  = t.DocumentPosition
                                                     let _tpo  = t.TermPosition
                                                     let _term = t.Token

                                                     let _occ = XElement("TermOccurrence", [])
                                                     if _isPhraseOrCTerm then
                                                        _occ.SetAttributeValue("term", _queryString)
                                                     else
                                                        _occ.SetAttributeValue("term", _term)
                                                        _occ.SetAttributeValue("docId", _did)
                                                        _occ.SetAttributeValue("seqId", _sid)
                                                        _occ.SetAttributeValue("dpoId", _dpo)
                                                        _occ.SetAttributeValue("tpoId", _tpo)
                                                     if _isPhraseOrCTerm then
                                                        _occ.SetAttributeValue("len", _length)
                                                     else
                                                        _occ.SetAttributeValue("len", _term.Length)
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
        _queryResultElm.Add(_queryLocs)
        _queryResultElm.SetAttributeValue("locationCount", _hits)
        _queryResultElm

    // See: https://stackoverflow.com/questions/27131774/f-issue-with-async-workflow-and-try-with
    let processTokenPostingSequenceAsync (dbPath: string) (input: string) (query: Query) (tokenPositionSeq: seq<TokenPositionEx>) 
                                         : Task<XElement> =
        async {
            setDatabasePath dbPath
            let _queryType = getQueryType input
            let _str = queryExpToString query
            let _queryStr = XElement("QueryString", [XText(input)])
            let _reverseQueryStr = reverseQueryString input _queryType
            let _reverseQuery = XElement("ReverseQueryString", [XText(_reverseQueryStr)])
            let _queryExpr = XElement("QueryExpression", [XText(_str)])

            let mutable _queryResult = XElement("QueryResult", [])
            _queryResult.SetAttributeValue("id", "0")
            _queryResult.SetAttributeValue("locationCount", 0)
            _queryResult.Add(_queryStr)
            if (_reverseQueryStr <> "") then
                _queryResult.Add(_reverseQuery)
            _queryResult.Add(_queryExpr)
            
            evaluateQuery _queryResult query |> ignore
            let queryType = _queryResult.Attribute("type").Value
            let mutable _length = 0
            let mutable _queryString = String.Empty
            let mutable _isPhraseOrCTerm = false
            if queryType.Equals("Phrase") || queryType.Equals("CTerm") then
                _queryString <- _queryResult.Descendants("QueryString").FirstOrDefault().Value
                _queryString <- _queryString.Substring(1, _queryString.Length - 2)
                _length <- _queryString.Length
                _isPhraseOrCTerm <- true

            let mutable _queryLocs = XElement("QueryLocations", 
                                            [XAttribute("count", 0)])
    
            let groups = tokenPositionSeq 
                         |> Seq.groupBy(fun t -> t.DocumentID, t.SequenceID, t.ParagraphID)

            let mutable _qryLocList = []
            groups
            |> Seq.iter(fun g -> let _tpl, _occs = g
                                 let _did, _sid, _pid = _tpl
                                 let _id = _did.ToString("0") 
                                           + "." + 
                                           _sid.ToString("0")
                             
                                 let termList = queryToTermList(query)
                                 let mutable _occList = XElement("TermOccurrenceList", [XAttribute("count", 0)])
                                 let mutable _qryLoc = XElement("QueryLocation", 
                                                                [XAttribute("id", _id)
                                                                 XAttribute("pid", _pid)
                                                                 (*XAttribute("termList", termList)*)])
                                 _occs
                                 |> Seq.sortBy(fun o -> o.TermPosition)
                                 |> Seq.iteri(fun i t -> let _pid  = t.ParagraphID
                                                         let _did  = Int32.Parse(t.DocumentID.ToString())
                                                         let _sid  = Int32.Parse(t.SequenceID.ToString())
                                                         let _dpo  = t.DocumentPosition
                                                         let _tpo  = t.TermPosition
                                                         let _term = t.Token

                                                         let _occ = XElement("TermOccurrence", [])
                                                         if _isPhraseOrCTerm then
                                                            _occ.SetAttributeValue("term", _queryString)
                                                         else
                                                            _occ.SetAttributeValue("term", _term)
                                                            _occ.SetAttributeValue("docId", _did)
                                                            _occ.SetAttributeValue("seqId", _sid)
                                                            _occ.SetAttributeValue("dpoId", _dpo)
                                                            _occ.SetAttributeValue("tpoId", _tpo)
                                                         if _isPhraseOrCTerm then
                                                            _occ.SetAttributeValue("len", _length)
                                                         else
                                                            _occ.SetAttributeValue("len", _term.Length)
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


