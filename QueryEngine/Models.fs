namespace QueryEngine

open UBViews.Query.Ast
open DataTypesEx

module Models = 

    type QueryResultObject = 
        { Id: int
          Hits: int
          Type: string
          Terms: string
          Proximity: string
          QueryString: string 
          ReverseQueryString: string
          QueryExpression: string }

    type TermLocationObject = 
        { Term: string 
          DocumentId: int
          SequenceId: int
          DocumentPosition: int
          TextPosition: int
          TextLength: int }

    type TermOccurrenceObject =
        { Id:int
          QueryResutlId: int
          DocumentId: int
          SequenceId: int
          DocumentPosition: int
          TextPosition: int
          TextLength: int
          ParagraphId: string 
          Term: string }

    type TokenOccurrenceObject =
        { Id:int
          PostingId: int
          DocumentId: int
          SequenceId: int
          SectionId: int
          DocumentPosition: int
          TextPosition: int 
          ParagraphId: string }

    type QueryLocationObject = 
        { Id: int
          Pid: string
          TermOccurrences: TermLocationObject list}

    type QueryResultLocationsObject =
        { Id: int
          Hits: int
          Type: string
          Terms: string
          Proximity: string
          QueryString: string
          ReverseQueryString: string
          QueryExpression: string
          QueryLocations: QueryLocationObject list }

    type PostingListObject = 
        { Id: int
          Lexeme: string }

    type TokenStemObject = 
        { Id: int
          Lexeme: string
          Stemmed: string }

