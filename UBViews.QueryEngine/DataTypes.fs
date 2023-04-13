module UBViews.LexParser.DataTypes

open System
open System.Collections.Generic
open System.IO

open Microsoft.FSharp.Core.String.FSharpStringExtensions

type QPTermPosition = {term:string; docId:int; seqId:int; idx:int; ln:int}
type QPTermPositionList = QPTermPosition list
type QPTermOccurrence = {docId:int; docFreq:int; seqId:int; paraId:string; posLst:QPTermPositionList}

let StringSplitPatternArrayLocal = [|"["; ";"; "]";|]

// Not Used by Solution
[<Serializable>]
type TermSetEx = Set<string>

[<Serializable>]
type CaptureLocationEx = {idx:int; ln:int}

[<Serializable>]
type TermPositionEx = {docId:int; seqId:int; idx:int; ln:int}
[<Serializable>]
type TermPositionListEx = TermPositionEx list
[<Serializable>]
type TermOccurrenceEx = {docId:int; docFreq:int; seqId:int; paraId:string; posLst:TermPositionListEx}
[<Serializable>]
type TermOccurrenceListEx = TermOccurrenceEx list
[<Serializable>]
type PaperVocabularyValueEx = {term:string; docId:int; idocFreq:int; docFreq:int; occLst:TermOccurrenceListEx }
[<Serializable>]
type BookVocabularyValueEx = {term:string; idocFreq:int; docFreq:int; occLst:TermOccurrenceListEx }
[<Serializable>]
type BookVocabularyValueListEx = BookVocabularyValueEx list
[<Serializable>]
type PaperVocabularyKeyValuePairEx = string * PaperVocabularyValueEx 
[<Serializable>]
type BookVocabularyKeyValuePairEx  = string * BookVocabularyValueEx

// Used by Mappers.fs and Tokenization.fs
//type TokenCase =
//    | Normal = 0
//    | ToLower = 1

/// Raw, unprocessed token. An individual word within a document. For example:
/// "Hello"; ","; "World"; "!".
type RawToken = string

/// Unique ID to represent a token. Obtained by taking the hashcode of the
/// raw token.
type TokenID = int

/// Converts a RawToken into a TokenID
let getTokenID (rawToken : RawToken) : TokenID =
    let tokenId = rawToken.ToLowerInvariant().GetHashCode()
    tokenId

//let getTokenIDByLookup (rawToken : RawToken) : TokenID =
//    let tokenId = rawToken.ToLowerInvariant().GetHashCode()
//    tokenId

/// Fancy way to represent a token id and lexeme tuple.
type Token =
    { ID: TokenID; Lexeme: string }
    override this.ToString() =
        sprintf "%d: %s" this.ID this.Lexeme

/// Converts a RawToken into a TokenID
let makeToken (rawToken : RawToken) : Token =
    { ID = rawToken.ToLowerInvariant().GetHashCode(); Lexeme = rawToken }

type ParagraphID = string

/// Unique ID to represent a token. Obtained by taking the hashcode of the
/// raw token.
type StableTokenID = string

/// Converts a RawToken into a TokenID
let getStableTokenID (rawToken : RawToken) : StableTokenID =
    rawToken.ToLowerInvariant().GetStableHashCode()

/// Fancy way to represent a token id and lexeme tuple.
type StableToken =
    { ID: StableTokenID; Lexeme: string }
    override this.ToString() =
        sprintf "%s: %s" this.ID this.Lexeme

/// Converts a RawToken into a TokenID
let makeStableToken (rawToken : RawToken) : StableToken =
    { ID = getStableTokenID(rawToken); Lexeme = rawToken }

type Lexicon = Map<TokenID, Token>

/// Unique ID to represent a paper name. 
type PaperID = int

/// Unique ID to represent a paper document [0 .. 196]. 
type DocumentID = int

/// Unique ID to represent a paragraph sequence within a paper document.
type SequenceID = int

/// 0-indexed term position within a tokenized paragraph's term array.
type DocumentPosition = int

/// 0-indexed term position within a paragraph's untokenized text string.
type TermPosition = int

// Unique ID for UBML PositionalIndex consisting of a tuple <docId>.<seqId>
type PositionID = string

type PositionToken =
    { PaperID: PaperID; SequenceID: SequenceID } 
    override this.ToString() =
        sprintf "%d.%d" this.PaperID this.SequenceID

/// Fancy way to represent a token id and lexeme tuple.
/// Most Current Used For Data
type ExtendedToken =
    { ID: int; 
      DocumentID: int; 
      SequenceID: int; 
      DocumentPosition: int; 
      StableID: string; 
      PositionID: string; 
      ParagraphId: string;
      TermPosition: int;
      Lexeme: string }
    override this.ToString() =
        sprintf "%d| %d| %d| %d| %s| %s| %s| %d| %s" 
            this.ID this.DocumentID this.SequenceID this.DocumentPosition this.StableID this.PositionID this.ParagraphId this.TermPosition this.Lexeme

/// Converts a RawToken into a TokenID
/// Most Current Used For Data
let makeExtendedToken (rawToken : RawToken) 
                      (docID: int) 
                      (seqID: int) 
                      (paraId: string) 
                      (dpoId: int) 
                      (idxId: int) =
    { ID = rawToken.GetHashCode()
      DocumentID = docID; 
      SequenceID = seqID;
      DocumentPosition = dpoId
      StableID = getStableTokenID rawToken;
      PositionID = { PaperID = docID; SequenceID = seqID }.ToString()
      ParagraphId = paraId;
      TermPosition = idxId;
      Lexeme = rawToken;  
    }

/// Fancy way to represent a document ID and sequence ID and position tuple. 
/// Note that this type implements custom comparison and equality.
///
/// TokenPositions are sorted by document id first, and then sequence id, 
/// and then by document position.
/// Most Current Used For Data
[<CustomComparison; CustomEquality>]
type TokenPosition =
    { TokenID: TokenID; 
      StableTokenID: StableTokenID; 
      ParagraphID: ParagraphID; 
      DocumentID : DocumentID; 
      SequenceID: SequenceID; 
      DocumentPosition : DocumentPosition 
      mutable TermPosition : TermPosition }

    /// Placeholder position for invalid locations. (E.g. an ended iterator.)
    static member InvalidTokenPosition =
        { TokenID = 0xffff; 
          StableTokenID = String.Empty; 
          ParagraphID = String.Empty; 
          DocumentID = 0xffff; 
          SequenceID = 0xffff; 
          DocumentPosition = 0xffff 
          TermPosition = 0xffff }

    //Func: compare
    //Retu: -1: first parameter is less than the second
    //       0: first parameter is equal to the second
    //       1: first parameter is greater than the second
    static member mycompare (x, y) = 
        match x, y with
        | (x, y) when x < y -> -1
        | (x, y) when x = y -> 0
        | _ -> 1

    interface IComparable with
        member this.CompareTo((rhs : obj)) =
            let rhsAsTokenPosition = rhs :?> TokenPosition
            let thisAsIComparable = this :> IComparable<TokenPosition>
            thisAsIComparable.CompareTo(rhsAsTokenPosition)

    interface IComparable<TokenPosition> with
        member this.CompareTo((rhs : TokenPosition)) =
            let lhs = this
            if lhs.DocumentID < rhs.DocumentID then
                -1
            elif lhs.DocumentID = rhs.DocumentID then
                if lhs.SequenceID < rhs.SequenceID then
                    -1
                elif lhs.SequenceID = rhs.SequenceID then
                    compare lhs.DocumentPosition rhs.DocumentPosition
                else
                    1
            else
                1

    // Parses a token position from text.
    static member Parse(str : string) =
        let throwInvalidException() =
            raise <| new ArgumentException("Invalid format.")
        if (str.[0] <> '{') || (str.[str.Length - 1] <> '}') then
            throwInvalidException()

        let parts = str.Substring(1, str.Length - 2).Split(',')
        if parts.Length <> 7 then
            throwInvalidException()

        try
            { TokenID = Int32.Parse(parts.[0])
              StableTokenID = parts.[1]
              ParagraphID = parts.[2]
              DocumentID = Int32.Parse(parts.[3])
              SequenceID = Int32.Parse(parts.[4])
              DocumentPosition = Int32.Parse(parts.[5])
              TermPosition = Int32.Parse(parts.[6]) }
        with _ -> 
            throwInvalidException()

    override this.ToString() =
        sprintf "{%d,%s,%s,%d,%d,%d,%d}" this.TokenID this.StableTokenID this.ParagraphID this.DocumentID this.SequenceID this.DocumentPosition this.TermPosition

    override this.GetHashCode() = (this.TokenID ^^^ this.DocumentID ^^^ this.SequenceID ^^^ this.DocumentPosition ^^^ this.TermPosition)

    override this.Equals(o) =
        let thisAsIC = this :> IComparable<TokenPosition>
        match o with
        | null -> false
        | :? TokenPosition as o -> (thisAsIC.CompareTo(o) = 0)
        | _ -> false

/// Posting list for a given token. Note that the token occurences are
/// sorted.
/// Most Current Used For Data
type TokenPostingList = 
    { TokenID : TokenID; StableTokenID: StableTokenID; TokenOccurrences : seq<TokenPosition> }

    /// Creates a new, empty BasicTokenPostingListExt.
    static member Empty(tokenID) = { TokenID = tokenID; StableTokenID = String.Empty; TokenOccurrences = [] }
        
    // Format: Num of posting lists, (TokenID, token position count, token positions)*
    member this.SerializeTo(stream : BinaryWriter) =
        let numHits = Seq.length this.TokenOccurrences
        // printfn "Serializing TPL for token [%d] with length %d" this.TokenID numHits

        stream.Write(int32 this.TokenID)
        stream.Write(string this.StableTokenID)
        stream.Write(int32 numHits)

        this.TokenOccurrences
        |> Seq.iter(fun tokenOccur -> stream.Write(int32 tokenOccur.TokenID)
                                      stream.Write(string tokenOccur.StableTokenID)
                                      stream.Write(string tokenOccur.ParagraphID)
                                      stream.Write(int32 tokenOccur.DocumentID)
                                      stream.Write(int32 tokenOccur.SequenceID)
                                      stream.Write(int32 tokenOccur.DocumentPosition)
                                      stream.Write(int32 tokenOccur.TermPosition))

    static member ParseFrom(stream : BinaryReader) =
        let tokenID = stream.ReadInt32()
        let stableToken = stream.ReadString()
        let tokenOccurrenceCount = stream.ReadInt32()
        // printfn "Deserializing TPL for token [%d] with length %d" tokenID tokenOccurrenceCount

        let tokenOccurrences = Array.zeroCreate tokenOccurrenceCount
        for i = 0 to tokenOccurrenceCount - 1 do
            let tokID = stream.ReadInt32()
            let stbID = stream.ReadString()
            let pidID = stream.ReadString()
            let docID = stream.ReadInt32()
            let seqID = stream.ReadInt32()
            let docPos = stream.ReadInt32()
            let trmPos = stream.ReadInt32()
            let tokenOccurrence = { TokenID = tokID; 
                                    StableTokenID = stbID; 
                                    ParagraphID = pidID; 
                                    DocumentID = docID; 
                                    SequenceID = seqID; 
                                    DocumentPosition = docPos; 
                                    TermPosition = trmPos; }
            tokenOccurrences.[i] <- tokenOccurrence

        { TokenID = tokenID; StableTokenID = stableToken; TokenOccurrences = tokenOccurrences :> seq<_> }

    static member SerializeCollection(collection : seq<TokenPostingList>, filePath) =
        use fileWriter = new StreamWriter(filePath, false (* append *))
        use binWriter = new BinaryWriter(fileWriter.BaseStream)
    
        binWriter.Write(int32 <| Seq.length collection)

        collection
        |> Seq.iter (fun tpl -> tpl.SerializeTo(binWriter))
    
        fileWriter.Close()

    static member DeserializeCollection(filePath : string) =
        use fileReader = new StreamReader(filePath)
        use binReader = new BinaryReader(fileReader.BaseStream)

        let numTpls = binReader.ReadInt32()

        let tpls = new List<_>(numTpls)
        for i = 0 to numTpls - 1 do
            let tpl = TokenPostingList.ParseFrom(binReader)
            tpls.Add(tpl)

        fileReader.Close()

        tpls

/// Most Current Used For Data
let makeTokenPosition (tokenID: TokenID) 
                      (stableID: StableTokenID) 
                      (paragraphID: ParagraphID)
                      (documentID: DocumentID) 
                      (sequenceID: SequenceID) 
                      (documentPosition: DocumentPosition) 
                      (termPosition: TermPosition) : TokenPosition =
    { 
        TokenID = tokenID;
        StableTokenID = stableID;
        ParagraphID = paragraphID;
        DocumentID = documentID; 
        SequenceID = sequenceID; 
        DocumentPosition = documentPosition;
        TermPosition = termPosition;
    }