module DataTypes

    open System
    open System.Collections.Generic
    open System.IO

    open Microsoft.FSharp.Core.String.FSharpStringExtensions

    let StringSplitPatternArrayLocal = [|"["; ";"; "]";|]

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

    type Lexicon = Map<TokenID, Token>

    /// Unique ID to represetn a posting list
    type PostingListID = int

    /// Unique ID to represent a paper name. 
    type PaperID = int

    /// Unique ID to represent a paper document [0 .. 196]. 
    type DocumentID = int

    /// Unique ID to represent a paragraph sequence within a paper document.
    type SequenceID = int

    /// Unique ID to represent a paragraph sectionId within a paper document.
    type SectionID = int

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
        { TokenID: int;
          DocumentID: int; 
          SequenceID: int; 
          SectionID: int;
          DocumentPosition: int; 
          PositionID: string; 
          ParagraphId: string;
          TermPosition: int;
          Lexeme: string }
        override this.ToString() =
            sprintf "%d %d| %d| %d| %d| %s| %s| %d| %s" 
                this.TokenID this.DocumentID this.SequenceID this.SectionID this.DocumentPosition this.PositionID this.ParagraphId this.TermPosition this.Lexeme

    /// Converts a RawToken into a TokenID
    /// Most Current Used For Data
    let makeExtendedToken (tokenId : TokenID) 
                          (docID: int) 
                          (seqID: int)
                          (secID: int)
                          (dpoId: int)
                          (paraId: string) 
                          (tpoId: int) 
                          (rawToken: string) =
        { TokenID = tokenId
          DocumentID = docID; 
          SequenceID = seqID;
          SectionID = secID;
          DocumentPosition = dpoId
          PositionID = { PaperID = docID; SequenceID = seqID }.ToString()
          ParagraphId = paraId;
          TermPosition = tpoId;
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
        { Token: RawToken;
          PostingListID: int; 
          ParagraphID: ParagraphID; 
          DocumentID : DocumentID; 
          SequenceID: SequenceID; 
          SectionID: SectionID;
          DocumentPosition : DocumentPosition 
          mutable TermPosition : TermPosition }

          /// Placeholder position for invalid locations. (E.g. an ended iterator.)
        static member InvalidTokenPosition =
            { Token = "InvalidTokenPosition";
              PostingListID = 0xffff; 
              ParagraphID = String.Empty; 
              DocumentID = 0xffff; 
              SequenceID = 0xffff; 
              SectionID = 0xffff;
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
        //static member Parse(str : string) =
        //    let throwInvalidException() =
        //        raise <| new ArgumentException("Invalid format.")
        //    if (str.[0] <> '{') || (str.[str.Length - 1] <> '}') then
        //        throwInvalidException()

        //    let parts = str.Substring(1, str.Length - 2).Split(',')
        //    if parts.Length <> 7 then
        //        throwInvalidException()

        //    try
        //        { Token = parts.[0]
        //          ParagraphID = parts.[2]
        //          DocumentID = Int32.Parse(parts.[3])
        //          SequenceID = Int32.Parse(parts.[4])
        //          DocumentPosition = Int32.Parse(parts.[5])
        //          TermPosition = Int32.Parse(parts.[6]) }
        //    with _ -> 
        //        throwInvalidException()

        override this.ToString() =
            sprintf "{%s,%d,%s,%d,%d %d,%d,%d}" this.Token this.PostingListID this.ParagraphID this.DocumentID this.SequenceID this.SectionID this.DocumentPosition this.TermPosition

        override this.GetHashCode() = (this.DocumentID ^^^ this.SequenceID ^^^ this.DocumentPosition ^^^ this.TermPosition)

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
        { PostingID : PostingListID; Token : RawToken; TokenOccurrences : seq<TokenPosition> }

        /// Creates a new, empty BasicTokenPostingListExt.
        static member Empty(postingID) = { PostingID = postingID; Token = ""; TokenOccurrences = [] }


    /// Most Current Used For Data
    let makeTokenPosition (token: RawToken)
                          (postingListID: PostingListID)
                          (paragraphID: ParagraphID)
                          (documentID: DocumentID) 
                          (sequenceID: SequenceID) 
                          (sectionID: SectionID)
                          (documentPosition: DocumentPosition) 
                          (termPosition: TermPosition) : TokenPosition =
        { Token = token;
          PostingListID = postingListID;
          ParagraphID = paragraphID;
          DocumentID = documentID; 
          SequenceID = sequenceID; 
          SectionID = sectionID;
          DocumentPosition = documentPosition;
          TermPosition = termPosition;
        }