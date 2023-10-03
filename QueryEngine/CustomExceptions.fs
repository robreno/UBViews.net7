namespace QueryEngine

module CustomExceptions =
        
    exception UnknownException of string * string

    type GetStemmedException(message, category) = 
        inherit exn(message)
        member x.Category = 
            category override x.ToString() = 
                sprintf "[%s] %s" category message

    type ProcessQueryException(message, category) =
        inherit exn(message)
        member x.Category = 
            category override x.ToString() = 
                sprintf "[%s] %s" category message

