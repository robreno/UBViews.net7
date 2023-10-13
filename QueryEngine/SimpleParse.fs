namespace QueryEngine

module SimpleParse =
    
    open System.Linq
    open QueryEngine.Models
    open SimpleEnumeratorsEx 
    open DataTypesEx
    open UBViews.Query.Ast


    let mutable _dbpath = ""
    let setDatabasePath (dbpath: string) =
        _dbpath <- dbpath

    let makeTokenPostingList (tokenExLst: List<TokenPositionEx>) : TokenPostingList =
        let mutable tpl = new TokenPostingList([])
        let newSeq = Seq.ofList tokenExLst
        tpl <- SimpleEnumeratorsEx.TokenPostingList(newSeq)
        tpl

    let compoundTermFromList(ctl: string list) =
        String.concat " " <| List.map string (List.rev ctl)

    /// Combines a list of query objects into a single one.
    /// e.g., And(Term("foreword"), Term("orvonton")) ->
    /// Or (NoOpQuery, And (Term "foreword", Term "orvonton"))
    let rec joinQueries (queries : Query list) =
        List.fold(fun acc query -> Or(acc, query)) NoOpQuery queries

    // Map a query object into a single document interator.
    let rec getIteratorEx (query : Query) : TokenPostingList =
        match query with
        | Term(term) ->
            let mutable tpl = new TokenPostingList([])
            let opt = async { let! retval = PostingRepository.getPostingListByLexemeAsync _dbpath term 
                                                |> Async.AwaitTask 
                              return retval } |> Async.StartAsTask
            
            let isSome = opt.Result.IsSome
            if (isSome) then
                let postingList = opt.Result.Value
                let postingListId = postingList.Id
                let tokOccs = async { let! retval = PostingRepository.getTokenOccurrencesByPostingListIdAsync _dbpath postingListId 
                                                    |> Async.AwaitTask
                                      let tokPosSeq = retval.Value
                                                      |> List.map(fun o -> let tp = { PostingListID = postingListId
                                                                                      Token = term   
                                                                                      DocumentID = o.DocumentId
                                                                                      SequenceID = o.SequenceId
                                                                                      SectionID = o.SectionId
                                                                                      DocumentPosition = o.DocumentPosition
                                                                                      TermPosition = o.TextPosition
                                                                                      ParagraphID = o.ParagraphId }
                                                         
                                                                           tp)
                                                      |> List.toSeq
                                      return tokPosSeq
                                    } |> Async.StartAsTask
                tpl <- new TokenPostingList(tokOccs.Result)
                tpl
            else
                tpl <- new TokenPostingList([])
                tpl 

        | STerm(term) ->
            // TODO: Add error handling. SQLiteException raised if path DBPath is bad.
            let mutable tpl = new TokenPostingList([])
            try
                let plObjList = async { let! result = PostingRepository.getTokenStemAsync _dbpath term |> Async.AwaitTask
                                        // handle bad return value here
                                        if (result.IsNone) then
                                            // failwith here and return empty TokenPostingList
                                            failwithf "Invalid Token -> [%s] in query." term
                                        let stem = result.Value
                                        let! plist = PostingRepository.getPostingListsByStemAsync _dbpath stem.Stemmed 
                                                                |> Async.AwaitTask
                                        return plist} |> Async.StartAsTask
            
                let mutable _tol = []
                let plo = plObjList.Result

                // Test case: ~rejuvenation -> rejuven -> five tokens with six hits
                // (rejuvenated[9241]: 41:7.15), (rejuvenated[9241]: 119:3.4), (self-rejuvenators[10303]: 48:4.11),
                // (rejuvenating[103 48:4.19), (rejuvenation[23349], 195:4.4), (rejuvenations[23453]: 196:1.2)

                if (plo.IsNone) then 
                    // failwith here and return empty TokenPostingList
                    failwithf "Invalid Token -> [%s] in query." term

                plo.Value
                |> List.iter(fun pl -> let ol = async { let! occs = PostingRepository.getTokenOccurrencesByPostingListIdAsync _dbpath pl.Id 
                                                                        |> Async.AwaitTask
                                                        // Pass thru postingLists exist
                                                        return occs.Value } |> Async.StartAsTask
                                       ol.Result |> List.iteri(fun i o -> let tp = { PostingListID = o.PostingId
                                                                                     Token = plo.Value.AsQueryable()
                                                                                                      .Where(fun oc -> oc.Id = o.PostingId)
                                                                                                      .FirstOrDefault().Lexeme
                                                                                     DocumentID = o.DocumentId
                                                                                     SequenceID = o.SequenceId
                                                                                     SectionID = o.SectionId
                                                                                     DocumentPosition = o.DocumentPosition
                                                                                     TermPosition = o.TextPosition
                                                                                     ParagraphID = o.ParagraphId }
                                                                          _tol <- tp :: _tol
                                                                       )
                                       )
                let tps = _tol |> List.toSeq
                tpl <- new TokenPostingList(tps)
                tpl
            with
            | _ as ex -> tpl

        | CTerm(cterm) ->
            let mutable tpl = new TokenPostingList([])
            try 
                let tokenIters =
                    cterm
                    |> List.rev 
                    |> List.map(fun token -> let opt = async { let! retval = PostingRepository.getPostingListByLexemeAsync _dbpath token |> Async.AwaitTask                                               
                                                               return retval } |> Async.StartAsTask
                                             if (opt.Result.IsNone) then
                                                // failwith here and return empty TokenPostingList
                                                failwithf "Invalid Token -> [%s] in query prhase." token
                                             else
                                                opt.Result.Value)
                    |> List.map(fun pl -> let tokOccs = async { let! retval = PostingRepository.getTokenOccurrencesByPostingListIdAsync _dbpath pl.Id 
                                                                                                    |> Async.AwaitTask
                                                                // Pass thru postingLists exist
                                                                let tokPosSeq = retval.Value
                                                                                |> List.map(fun o -> let tp = { Token = pl.Lexeme 
                                                                                                                PostingListID = pl.Id                                                                                                               
                                                                                                                ParagraphID = o.ParagraphId
                                                                                                                DocumentID = o.DocumentId 
                                                                                                                SequenceID = o.SequenceId 
                                                                                                                SectionID = o.SectionId
                                                                                                                DocumentPosition = o.DocumentPosition
                                                                                                                TermPosition = o.TextPosition }
                                                                                                     tp)
                                                                                |> List.toSeq
                                                                return tokPosSeq
                                                            } |> Async.StartAsTask
                                          let occs = tokOccs.Result
                                          occs) 
                    |> Seq.toList
                    |> List.map(fun tokenHits -> new TokenPostingList(tokenHits))
                    |> List.toArray
                tpl <- createCompoundTermEnumerator tokenIters
                tpl
            with
            | _ as ex -> tpl
            //| _ as ex -> raise <| ArgumentNullException(ex.Message, ex.InnerException)

        | Phrase(phrase) ->
            let mutable tpl = new TokenPostingList([])
            try 
                let tokenIters  =
                    phrase
                    |> List.rev 
                    |> List.map(fun token -> let opt = async { let! retval = PostingRepository.getPostingListByLexemeAsync _dbpath token 
                                                                                |> Async.AwaitTask                                               
                                                               return retval } |> Async.StartAsTask
                                             if (opt.Result.IsNone) then
                                                // failwith here and return empty TokenPostingList
                                                failwithf "Invalid Token -> [%s] in query prhase." token
                                             else
                                                opt.Result.Value)
                    |> List.map(fun pl -> let tokOccs = async { let! retval = PostingRepository.getTokenOccurrencesByPostingListIdAsync _dbpath pl.Id
                                                                                    |> Async.AwaitTask
                                                                // Pass thru postingLists exist
                                                                let tokPosSeq = retval.Value
                                                                                |> List.map(fun o -> let tp = { PostingListID = pl.Id
                                                                                                                Token = pl.Lexeme   
                                                                                                                DocumentID = o.DocumentId 
                                                                                                                SequenceID = o.SequenceId 
                                                                                                                SectionID = o.SectionId
                                                                                                                DocumentPosition = o.DocumentPosition
                                                                                                                TermPosition = o.TextPosition 
                                                                                                                ParagraphID = o.ParagraphId}
                                                                                                     tp)
                                                                                |> List.toSeq
                                                                return tokPosSeq
                                                            } |> Async.StartAsTask
                                          let occs = tokOccs.Result
                                          occs) 
                    |> Seq.toList
                    |> List.map(fun tokenHits -> new TokenPostingList(tokenHits))
                    |> List.toArray
                tpl <- createExactPhraseEnumerator tokenIters 
                tpl
            with
            | _ as ex -> tpl
            //| _ as ex -> raise <| ArgumentNullException(ex.Message, ex.InnerException)
            
        | And(x, y) -> 
            let results = conjunctiveQueryWithRangePID 
                            (getIteratorEx x) (getIteratorEx y)
            results
        | Or(x, y) -> 
            let results = disjunctiveQueryWithRangePID
                            (getIteratorEx x) (getIteratorEx y)
            results
        | SubQuery(q) -> getIteratorEx q
        | FilterBy(q, f) -> let tpl =
                                match q with
                                | Term(term)     -> termQueryWithRange
                                                        (getIteratorEx q) (f)
                                | STerm(term)    -> new TokenPostingList([])
                                | Phrase(phrase) -> new TokenPostingList([])
                                | And(x, y)      -> conjunctiveQueryWithRange
                                                        (getIteratorEx x) (getIteratorEx y) (f)
                                | Or(x, y)       -> disjunctiveQueryWithRange
                                                        (getIteratorEx x) (getIteratorEx y) (f)
                                | SubQuery(sq)   -> let newQuery = FilterBy(sq, f)
                                                    getIteratorEx newQuery
                                | _ -> getIteratorEx q
                            tpl
        | NoOpQuery   -> new TokenPostingList([])

    let runQuery (dbPath: string) (query: Query) =
        setDatabasePath dbPath
        let result = getIteratorEx query
        result

    let runQueryAsync (dbPath: string) (query: Query) =
        async {
            setDatabasePath dbPath
            let result = getIteratorEx query
            return result
        } |> Async.StartAsTask

