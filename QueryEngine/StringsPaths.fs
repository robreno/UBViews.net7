namespace QueryEngine

open System.IO

module StringPaths =
    let dbPathLocalState = @"C:\Users\robre\AppData\Local\Packages\879ca98e-d45e-44b3-9be6-e6d900695058_9zz4h110yvjzm\LocalState\"
    let postingDBName = "postingLists.db3"
    let queryDbName   = "queryResults.db3"
    let queryDbPath   = Path.Combine(dbPathLocalState, queryDbName)
    let postingDbPath = Path.Combine(dbPathLocalState, postingDBName)
    

