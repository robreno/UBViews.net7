namespace UBViews.LexParser

open FSharp.Text.Lexing

type ParserService() = 
    member this.ParseQuery input =
        let lexbuff = LexBuffer<char>.FromString(input)
        let query = Parser.start Lexer.tokenize lexbuff
        query

module Say =
    let hello name =
        printfn "Hello %s" name
