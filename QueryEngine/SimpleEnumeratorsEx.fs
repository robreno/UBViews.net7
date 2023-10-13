namespace QueryEngine

[<AutoOpen>]
module SimpleEnumeratorsEx =

    open Microsoft.FSharp.Collections
    open System.Linq
    open DataTypesEx
    open UBViews.Query.Ast

    type TokenPostingList(basePostingList : seq<TokenPositionEx>) =
        let underlyingEnumerator = basePostingList.GetEnumerator()
        let _basePostingList = basePostingList
        // Move enumerator to initial position and initialize m_atEnd.
        // ... is how you would initialize a normal enumerator. A seq<_> from the F#
        // compiler however appears do not support Reset().
        // do underlyingEnumerator.Reset()
        let mutable m_atEnd = not <| underlyingEnumerator.MoveNext()
        
        /// If the enumerator as reached its final position. Reset by calling
        member this.AtEnd = m_atEnd

        member this.BasePostingList = [_basePostingList]

        member this.GetBasePostingList() =
            let tokenPositionList = _basePostingList.ToList()
            tokenPositionList

        member this.AdvanceTo(tokenPosition) =
            while (not this.AtEnd) && (this.Current < tokenPosition) do
                this.MoveNext()

        member this.Current =
            if not m_atEnd then
                underlyingEnumerator.Current
            else
                TokenPositionEx.InvalidTokenPosition

        member this.MoveNext() =
            m_atEnd <- not <| underlyingEnumerator.MoveNext()
            ()

        member this.MoveNextDocument() =
            let startingDoc = this.Current.DocumentID
            while not this.AtEnd && this.Current.DocumentID = startingDoc do
                this.MoveNext()

        member this.MoveNextSequence() =
            let startingDoc = this.Current.DocumentID
            let startingSeq = this.Current.SequenceID
            while not this.AtEnd && 
                      this.Current.DocumentID = startingDoc && 
                      this.Current.SequenceID = startingSeq do
                this.MoveNext()

        member this.MoveNextParagraph() =
            this.MoveNextSequence()

        member this.MoveNextPosition() =
            let startingDoc = this.Current.DocumentID
            let startingSeq = this.Current.SequenceID
            let startingPos = this.Current.DocumentPosition
            while not this.AtEnd && 
                      this.Current.DocumentID = startingDoc && 
                      this.Current.SequenceID = startingSeq &&
                      this.Current.DocumentPosition = startingPos do
                this.MoveNext()

    /// Compound term enumerator. Returns all document positions where the given
    /// tokens occur in a specific order.
    ///
    /// This is done by finding all hits of the enumerators where the positions
    /// differ by one and pids are equal.
    let createCompoundTermEnumerator(phraseParts : TokenPostingList[]) =
        // <TokenOccurrence did="25" sid="59" dpo="3" tpo="21" pid="25:4.9" />
        // <TokenOccurrence did="25" sid="59" dpo="4" tpo="31" pid="25:4.9" />
        new TokenPostingList(
            seq {
                if phraseParts.Length > 0 then
                    while not <| phraseParts.[0].AtEnd do
                        let firstTokenPosition = phraseParts.[0].Current

                        // Advance all other enumerators to that point (if possible).
                        for i = 1 to phraseParts.Length - 1 do
                            phraseParts.[i].AdvanceTo(firstTokenPosition)

                        // Check if they all line up.
                        let foldFunc (expectedDocID, expectedSeqID, expectedPosition, expectedPID, lineUp) (phrasePartEnumerator : TokenPostingList) =
                            let lineUp' = 
                                lineUp &&
                                (phrasePartEnumerator.Current.DocumentID = expectedDocID) &&
                                (phrasePartEnumerator.Current.SequenceID = expectedSeqID) &&
                                (phrasePartEnumerator.Current.DocumentPosition = expectedPosition) &&
                                (phrasePartEnumerator.Current.ParagraphID = expectedPID)
                            (expectedDocID, expectedSeqID, expectedPosition + 1, expectedPID, lineUp')

                        let initialState =
                            (firstTokenPosition.DocumentID, 
                             firstTokenPosition.SequenceID, 
                             firstTokenPosition.DocumentPosition, 
                             firstTokenPosition.ParagraphID, 
                             true)

                        let allLineUp =
                            phraseParts
                            |> Array.fold foldFunc initialState
                            |> (fun (_, _, _, _, lineUp) -> lineUp)

                        if allLineUp then
                            yield firstTokenPosition
                        
                        // Move to the next start of the phrase and continue.
                        phraseParts.[0].MoveNext()
        })

    /// Exact phrase enumerator. Returns all document positions where the given
    /// tokens occur in a specific order.
    ///
    /// This is done by finding all hits of the enumerators where the positions
    /// differ by one.
    let createExactPhraseEnumerator(phraseParts : TokenPostingList[]) =
        let newTokenPostingList =
            new TokenPostingList(
                seq {
                    if phraseParts.Length > 0 then
                        while not <| phraseParts.[0].AtEnd do
                            let firstTokenPosition = phraseParts.[0].Current

                            // Advance all other enumerators to that point (if possible).
                            for i = 1 to phraseParts.Length - 1 do
                                phraseParts.[i].AdvanceTo(firstTokenPosition)

                            // Check if they all line up.
                            let foldFunc (expectedDocID, expectedPosition, lineUp) (phrasePartEnumerator : TokenPostingList) =
                                let lineUp' = 
                                    lineUp &&
                                    (phrasePartEnumerator.Current.DocumentID = expectedDocID) &&
                                    (phrasePartEnumerator.Current.DocumentPosition = expectedPosition)
                                (expectedDocID, expectedPosition + 1, lineUp')

                            let initialState =
                                (firstTokenPosition.DocumentID, firstTokenPosition.DocumentPosition, true)

                            let allLineUp =
                                phraseParts
                                |> Array.fold foldFunc initialState
                                |> (fun (_, _, lineUp) -> lineUp)

                            if allLineUp then
                                yield firstTokenPosition
                        
                            // Move to the next start of the phrase and continue.
                            phraseParts.[0].MoveNext()
            })

        // {{minds,4,0:0.1,0,2,2,7}}
        // {{minds,4,1:3.6,1,32,67,409}}
        // {{minds,4,130:4.5,130,43,28,189}}

        newTokenPostingList

    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveEnumerator (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        new TokenPostingList(
            seq {
                while (not iter1.AtEnd) && (not iter2.AtEnd) do
                    if iter1.AtEnd then
                        iter2.MoveNextDocument()
                    elif iter2.AtEnd then
                        iter1.MoveNextDocument()
                    else
                        let i1Current = iter1.Current
                        let i2Current = iter2.Current
                        if i1Current.DocumentID = i2Current.DocumentID &&
                           i1Current.SequenceID = i2Current.SequenceID then
                            // Take care not to yield the same document more than once.
                            yield i1Current
                            iter1.MoveNextSequence()
                            iter2.MoveNextSequence()
                        elif i1Current < i2Current then
                            //yield i1Current
                            iter1.MoveNextSequence()
                        else
                            //yield i2Current
                            iter2.MoveNextSequence()
        })

    /// Disjunctive enumerator returning documents that are found in either list.
    let disjunctiveEnumerator (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        new TokenPostingList(
            seq {
                while (not iter1.AtEnd) || (not iter2.AtEnd) do
                    if iter1.AtEnd then
                        yield iter2.Current
                        iter2.MoveNextDocument()
                    elif iter2.AtEnd then
                        yield iter1.Current
                        iter1.MoveNextDocument()
                    else
                        let i1Current = iter1.Current
                        let i2Current = iter2.Current
                        if i1Current.DocumentID = i2Current.DocumentID &&
                           i1Current.SequenceID = i2Current.SequenceID then
                            // Take care not to yield the same document more than once.
                            yield i1Current
                            iter1.MoveNextSequence()
                            iter2.MoveNextSequence()
                        elif i1Current < i2Current then
                            //yield i1Current
                            iter1.MoveNextSequence()
                        else
                            //yield i2Current
                            iter2.MoveNextSequence()
        })

    /// Experimental Methods

    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRangeTOP (iter1 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let resultSequence = seq1 |> Seq.map(fun t -> t)
        let tpl = new TokenPostingList(resultSequence)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRangeDID (iter1 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let resultSequence = seq1 |> Seq.map(fun t -> t)
        let tpl = new TokenPostingList(resultSequence)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRangeSID (iter1 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let resultSequence = seq1 |> Seq.map(fun t -> t)
        let tpl = new TokenPostingList(resultSequence)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRangePID (iter1 : TokenPostingList) =
    
        let seq1 = iter1.BasePostingList.Head
        let resultSequence = seq1 |> Seq.map(fun t -> t)
        let tpl = new TokenPostingList(resultSequence)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRangeSEC (iter1 : TokenPostingList) =
    
        let seq1 = iter1.BasePostingList.Head
        let resultSequence = seq1 |> Seq.map(fun t -> t)
        let tpl = new TokenPostingList(resultSequence)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let termQueryWithRange (iter1 : TokenPostingList)  
                           (filterBy: FilterValue) =
        match filterBy with
        | TOPID -> termQueryWithRangeTOP iter1
        | DOCID -> termQueryWithRangeDID iter1
        | SEQID -> termQueryWithRangeSID iter1
        | PARID -> termQueryWithRangePID iter1
        | SECID -> termQueryWithRangeSEC iter1

    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRangeDID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let query1 = query { for tp in seq1 do
                             select (tp.DocumentID) }
        let query2 = query { for tp in seq2 do 
                             select (tp.DocumentID) }

        let set1 = query1 |> Set.ofSeq
        let set2 = query2 |> Set.ofSeq
        let iofsets = Set.intersect set1 set2
        let uofseq = seq1.Union seq2
        let newSeq = query { for tp in uofseq do 
                             where (iofsets.Contains((tp.DocumentID)))
                             sortBy (tp.DocumentID, tp.SequenceID, tp.DocumentPosition)
                             select (tp) }

        let tpl = new TokenPostingList(newSeq)
        tpl

    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRangeSEC (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
    
        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let query1 = query { for tp in seq1 do
                             let a = tp.ParagraphID.Split(':','.')
                             select (a.ElementAt(0), a.ElementAt(1)) }
        let query2 = query { for tp in seq2 do 
                             let a = tp.ParagraphID.Split(':','.')
                             select (a.ElementAt(0), a.ElementAt(1)) }

        let set1 = query1 |> Set.ofSeq
        let set2 = query2 |> Set.ofSeq
        let iofsets = Set.intersect set1 set2
        let uofseq = seq1.Union seq2
        let newSeq = query { for tp in uofseq do 
                             let a = tp.ParagraphID.Split(':','.')
                             let tpl = a.ElementAt(0), a.ElementAt(1)
                             where (iofsets.Contains(tpl))
                             sortBy (tp.DocumentID, tp.SequenceID, tp.DocumentPosition)
                             select (tp) }

        let tpl = new TokenPostingList(newSeq)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRangePID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let query1 = query { for tp in seq1 do
                             select (tp.ParagraphID) }
        let query2 = query { for tp in seq2 do 
                             select (tp.ParagraphID) }

        let set1 = query1 |> Set.ofSeq
        let set2 = query2 |> Set.ofSeq
        let iofsets = Set.intersect set1 set2
        let uofseq = seq1.Union seq2
        let newSeq = query { for tp in uofseq do 
                             where (iofsets.Contains(tp.ParagraphID))
                             sortBy (tp.DocumentID, tp.SequenceID, tp.DocumentPosition)
                             select (tp) }

        let tpl = new TokenPostingList(newSeq)
        tpl

    let conjunctiveQueryWithRangePID_test (iter1 : TokenPostingList) (iter2 : TokenPostingList) =

        // TODO: Bug: never-ending  and "infinite perfection"
        // iter1 and iter2 have different types of underlying Enumerators
        // don't know why and causes bug.
        // Force to same type using ToList()

        let lst1 = iter1.BasePostingList.Head.ToList()
        let lst2 = iter2.BasePostingList.Head.ToList()

        let seq1 = Seq.toList lst1 |> Seq.ofList
        let seq2 = Seq.toList lst2 |> Seq.ofList

        let query1 = query { for tp in seq1 do
                             select (tp.DocumentID, tp.SequenceID) }
        let query2 = query { for tp in seq2 do 
                             select (tp.DocumentID, tp.SequenceID) }

        let set1 = query1 |> Set.ofSeq
        let set2 = query2 |> Set.ofSeq
        let iofsets = Set.intersect set1 set2
        let uofseq = seq1.Union seq2
        let newSeq = query { for tp in uofseq do 
                             where (iofsets.Contains((tp.DocumentID, tp.SequenceID)))
                             sortBy (tp.DocumentID, tp.SequenceID, tp.DocumentPosition)
                             select (tp) }

        let tpl = new TokenPostingList(newSeq)
        tpl

    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRangeSID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =

        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let query1 = query { for tp in seq1 do
                             select (tp.DocumentID, tp.SequenceID) }
        let query2 = query { for tp in seq2 do 
                             select (tp.DocumentID, tp.SequenceID) }

        let set1 = query1 |> Set.ofSeq
        let set2 = query2 |> Set.ofSeq
        let iofsets = Set.intersect set1 set2
        let uofseq = seq1.Union seq2
        let newSeq = query { for tp in uofseq do 
                             where (iofsets.Contains((tp.DocumentID, tp.SequenceID)))
                             sortBy (tp.DocumentID, tp.SequenceID, tp.DocumentPosition)
                             select (tp) }

        let tpl = new TokenPostingList(newSeq)
        tpl

    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRangeTOP (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        conjunctiveQueryWithRangeDID iter1 iter2
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let conjunctiveQueryWithRange (iter1 : TokenPostingList) 
                                  (iter2 : TokenPostingList) 
                                  (filterBy: FilterValue) =
        match filterBy with
        | TOPID -> conjunctiveQueryWithRangeTOP iter1 iter2
        | DOCID -> conjunctiveQueryWithRangeDID iter1 iter2
        | SEQID -> conjunctiveQueryWithRangeSID iter1 iter2
        | PARID -> conjunctiveQueryWithRangePID iter1 iter2
        | SECID -> conjunctiveQueryWithRangeSEC iter1 iter2

        /// Combine two enumerators to return only documents found in both enumerators.
    let disjunctiveQueryWithRangeTOP (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let newSeq = Seq.append seq1 seq2
        let tpl = new TokenPostingList(newSeq)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let disjunctiveQueryWithRangeDID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let newSeq = Seq.append seq1 seq2
        let tpl = new TokenPostingList(newSeq)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let disjunctiveQueryWithRangeSID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let newSeq = Seq.append seq1 seq2
        let tpl = new TokenPostingList(newSeq)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let disjunctiveQueryWithRangePID (iter1 : TokenPostingList) (iter2 : TokenPostingList) =
        let seq1 = iter1.BasePostingList.Head
        let seq2 = iter2.BasePostingList.Head

        let newSeq = Seq.append seq1 seq2
        let tpl = new TokenPostingList(newSeq)
        tpl
    
    /// Combine two enumerators to return only documents found in both enumerators.
    let disjunctiveQueryWithRange (iter1 : TokenPostingList) 
                                  (iter2 : TokenPostingList) 
                                  (filterBy: FilterValue) =
        match filterBy with
        | TOPID -> disjunctiveQueryWithRangeTOP iter1 iter2
        | DOCID -> disjunctiveQueryWithRangeDID iter1 iter2
        | SEQID -> disjunctiveQueryWithRangeSID iter1 iter2
        | PARID -> disjunctiveQueryWithRangePID iter1 iter2
        | SECID -> new TokenPostingList([])


