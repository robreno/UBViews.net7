namespace QueryFilterLib

module RecordTypes =

    type Term               = string
    type RegexPattern       = string
    type TermLocation       = {did:int; sid:int; idx:int; len:int}
    type TermLocationList   = TermLocation list
    type TermLocationRecord = {Term:Term; LocationList:TermLocationList}

    type TermLocationEx        = {did:int; sid:int; idx:int; len:int; pid:string}
    type TermLocationListEx    = TermLocationEx list
    type TermLocationArrayEx   = TermLocationEx array
    type TermLocationRecordEx  = {Term:Term; Pattern:RegexPattern; LocationsEx:TermLocationArrayEx}

    type TermPositionMU = { did:int; sid:int; dpo:int; tpo:int; len:int; pid:string }
    type TermPositionListMU = TermPositionMU list
    type TermPostionArrayMU = TermPositionMU array

    type SpecialTermLocationRecordEx  = {Term:Term; Pattern:RegexPattern; LocationsEx:TermLocationArrayEx}
    type HyphenTermLocationRecordEx   = {Term:Term; Pattern:RegexPattern; LocationsEx:TermLocationArrayEx}
    type CompoundTermLocationRecordEx = {Term:Term; Pattern:RegexPattern; TermLocations:TermPostionArrayMU}

    type ParagraphTerms         = {term:string; Index:int;}
    type ParagraphTermsList     = ParagraphTerms list
    type ParagraphTermsArray    = ParagraphTerms array
    type ParagraphTermLookupMap = {ID:ParagraphTermsList}

    type TermLookupRec = {term:string; did:int; sid:int; idx:int; len:int; pid:string; }
    type TermLookupRecList = TermLookupRec list

    type StatisticsDto = {cf:int; mutable df:int; mutable idf:double; mutable freq_vec: (int * double) array;} 
    type DiffDto = {term:string; mutable cf:int; mutable df:int; mutable idf:double; mutable cmpt:int; rmn:int;} 

    type StemmedDto = {tokenId:string; stemmed:string;}

