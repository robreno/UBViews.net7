namespace QueryEngine

open System.IO
open SQLite
open Models
open UtilFuncs

module PostingRepository =

    // PostingList Tables
    [<Table("PostingLists")>]
    [<AllowNullLiteral>]
    type PostingList() = 
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set
        member val Lexeme = "" with get, set

    [<Table("TokenOccurrences")>]
    [<AllowNullLiteral>]
    type TokenOccurrence() =
        [<PrimaryKeyAttribute; AutoIncrement>]
        member val Id = 0 with get, set
        member val PostingId = 0 with get, set
        member val DocumentId = 0 with get, set
        member val SequenceId = 0 with get, set
        member val SectionId = 0 with get, set
        member val DocumentPosition = 0 with get, set
        member val TextPosition = 0 with get, set
        member val ParagraphId = "" with get, set

    [<Table("TokenStems")>]
    [<AllowNullLiteral>]
    type TokenStem() =
        [<PrimaryKeyAttribute; AutoIncrement>]
        member val Id = 0 with get, set
        member val Lexeme = "" with get, set
        member val Stemmed = "" with get, set 

    // Conversion Methods
    let convertPostingListToModel (obj: PostingList) : PostingListObject =
        { Id = obj.Id
          Lexeme = obj.Lexeme } 

    let convertTokenOccurrenceToModel (obj: TokenOccurrence) : TokenOccurrenceObject =
        { Id = obj.Id
          PostingId = obj.PostingId
          DocumentId = obj.DocumentId
          SequenceId = obj.SequenceId
          SectionId = obj.SectionId
          DocumentPosition = obj.DocumentPosition
          TextPosition = obj.TextPosition 
          ParagraphId = obj.ParagraphId } 

    let convertTokenStemToModel (obj: TokenStem) : TokenStemObject =
        { Id = obj.Id
          Lexeme = obj.Lexeme
          Stemmed = obj.Stemmed } 

    /// <summary>
    /// connect
    /// </summary>
    /// <remarks>Creates connection to database.</remarks>
    /// <param name="dbPath"></param>
    /// <returns>SQLiteAsyncConnection to database.</returns>
    let connect dbPath = 
        async {
            let db = SQLiteAsyncConnection(SQLiteConnectionString dbPath)
            let path = db.DatabasePath
            do! db.CreateTableAsync<PostingList>() |> Async.AwaitTask |> Async.Ignore
            return db
        }

    /// <summary>
    /// getPostingListsAsync
    /// </summary>
    /// <remarks>Returns all PostingLists in database.</remarks>
    /// <param name="dbPath"></param>
    /// <returns>List of PostingList models.</returns>
    let getPostingListsAsync dbPath = 
        async {
            let! database = connect dbPath
            let! objs = database.Table<PostingList>().ToListAsync() |> Async.AwaitTask
            return objs |> Seq.toList |> List.map convertPostingListToModel
        } |> Async.StartAsTask

    /// <summary>
    /// getPostingListByLexemeAsync
    /// </summary>
    /// <remarks>Returns PostingList associated with lexeme.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="lexeme">Lexeme PostingList.</param>
    /// <returns>Some if obj not null or None if obj is null.</returns>
    let getPostingListByLexemeAsync dbPath lexeme = 
        async {
            let! database = connect dbPath
            let! obj = database.Table<PostingList>().Where(fun p -> p.Lexeme = lexeme).FirstOrDefaultAsync() |> Async.AwaitTask
            let result = CheckForNull(obj)
            if (result.IsNone) then
                return None
            else
                let objModel = obj |> convertPostingListToModel
                return Some(objModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getTokenOccurrencesByPostingListIdAsync
    /// </summary>
    /// <remarks>Returns PostingList by its ID.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="id">PostingList ID.</param>
    /// <returns>Some if obj not null or None if obj is null.</returns>
    let getTokenOccurrencesByPostingListIdAsync dbPath id =
        async {
            let! database = connect dbPath
            let! objs = database.Table<TokenOccurrence>().Where(fun o -> o.PostingId = id).ToListAsync() |> Async.AwaitTask
            if (objs.Count = 0) then
                return None
            else
                let objsModel = objs |> Seq.toList |> List.map convertTokenOccurrenceToModel
                return Some(objsModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getTokenStemsAsync
    /// </summary>
    /// <remarks>Returns List of all TokenStems in database.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <returns>List of all TokenStems or empty list.</returns>
    let getTokenStemsAsync dbPath = 
        async {
            let! database = connect dbPath
            let! objs = database.Table<TokenStem>().ToListAsync() |> Async.AwaitTask
            return objs |> Seq.toList |> List.map convertTokenStemToModel
        } |> Async.StartAsTask

    /// <summary>
    /// getTokenStemAsync
    /// </summary>
    /// <remarks>Returns TokenStem by its lexeme.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="lexeme">Lexeme.</param>
    /// <returns>Some if obj not null or None if obj is null.</returns>
    let getTokenStemAsync dbPath lexeme = 
        async {
            let! database = connect dbPath
            let! obj = database.Table<TokenStem>().Where(fun t -> t.Lexeme = lexeme)
                                                  .FirstOrDefaultAsync() |> Async.AwaitTask
            let result = CheckForNull(obj)
            if (result.IsNone) then
                return None
            else
                let objModel = obj |> convertTokenStemToModel
                return Some(objModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getPostingListsByStemAsync
    /// </summary>
    /// <remarks>Returns TokenStem by its lexeme.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="stem">Stem.</param>
    /// <returns>Some if obj not null or None if obj is null.</returns>
    let getPostingListsByStemAsync (dbPath:string) (stem:string) = 
        async {
            let! database = connect dbPath
            let! postingLists = database.Table<PostingList>().Where(fun pl -> pl.Lexeme.Contains(stem))
                                                             .ToListAsync() |> Async.AwaitTask
            if (postingLists.Count = 0) then 
                return None
            else
                let objsModel = postingLists |> Seq.toList |> List.map(fun pl -> convertPostingListToModel pl)
                return Some(objsModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getTokenOccurrencesByStemAsync
    /// </summary>
    /// <remarks>Returns TokenStem by its lexeme.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="stem">Stem.</param>
    /// <returns>Some if obj not null or None if obj is null.</returns>
    let getTokenOccurrencesByStemAsync (dbPath:string) (stem:string) = 
        async {
            let! database = connect dbPath
            let! postingListObjs = getPostingListsByStemAsync dbPath stem |> Async.AwaitTask
            if (postingListObjs.IsNone) then 
                return None
            else
                let plObjs = postingListObjs.Value // TODO: Test this method (?)
                let objsModel1 = plObjs |> List.map(fun pl -> let occs = getTokenOccurrencesByPostingListIdAsync dbPath, pl.Id
                                                              occs)
                let objsModel = postingListObjs.Value |> Seq.toList //|> List.map(fun pl -> convertPostingListToModel pl)
                return Some(objsModel)
            // Original
            //let objs = postingListObjs |> List.map(fun pl -> let occs = getTokenOccurrencesByPostingListIdAsync dbPath pl.Id
            //                                                 let opt = CheckForNull(occs)
            //                                                 occs.Result)
            //return objs
        } |> Async.StartAsTask

module QueryRepository =

    // QueryResults Tables
    [<Table("QueryResults")>]
    [<AllowNullLiteral>]
    type QueryResult() =
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set
        member val Hits = 0 with get, set
        member val Type = "" with get, set
        member val Terms = "" with get, set
        member val Proximity = "" with get, set
        member val QueryString = "" with get, set
        member val ReverseQueryString = "" with get, set
        member val QueryExpression = "" with get, set

    [<Table("TermOccurrences")>]
    type TermOccurrence() =
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set
        member val QueryResultId = 0 with get, set
        member val DocumentId = 0 with get, set
        member val SequenceId = 0 with get, set
        member val DocumentPosition = 0 with get, set
        member val TextPosition = 0 with get, set
        member val TextLength = 0 with get, set
        member val ParagraphId = "" with get, set
        member val Term = "" with get, set

    // Conversion Methods
    let convertModelToQueryResult (obj: QueryResultObject) : QueryResult =
        let qry = new QueryResult()
        qry.Id <- obj.Id
        qry.Hits <- obj.Hits
        qry.Type <- obj.Type
        qry.Terms <- obj.Terms
        qry.Proximity <- obj.Proximity
        qry.QueryString <- obj.QueryString
        qry.ReverseQueryString <- obj.ReverseQueryString
        qry.QueryExpression <- obj.QueryExpression
        qry

    let convertQueryResultToModel (obj: QueryResult) : QueryResultObject =
        { Id = obj.Id
          Hits = obj.Hits
          Type = obj.Type
          Terms = obj.Terms
          Proximity = obj.Proximity
          QueryString = obj.QueryString
          ReverseQueryString = obj.ReverseQueryString
          QueryExpression = obj.QueryExpression } 

    let convertModelToTermOccurrence (obj: TermOccurrenceObject) : TermOccurrence =
        let toc = new TermOccurrence()
        toc.Id <- obj.Id
        toc.QueryResultId <- obj.QueryResutlId
        toc.DocumentId <- obj.DocumentId
        toc.SequenceId <- obj.SequenceId
        toc.DocumentPosition <- obj.DocumentPosition
        toc.TextPosition <- obj.TextPosition
        toc.TextLength <- obj.TextLength
        toc.ParagraphId <- obj.ParagraphId
        toc.Term <- obj.Term
        toc

    let convertTermOccurrenceToModel (obj: TermOccurrence) : TermOccurrenceObject =
        { Id = obj.Id
          QueryResutlId = obj.QueryResultId
          DocumentId = obj.DocumentId
          SequenceId = obj.SequenceId
          DocumentPosition = obj.DocumentPosition
          TextPosition = obj.TextPosition
          TextLength = obj.TextLength
          ParagraphId = obj.ParagraphId
          Term = obj.Term }

    /// <summary>
    /// connect
    /// </summary>
    /// <remarks>Creates connection to database.</remarks>
    /// <param name="dbPath"></param>
    /// <returns>SQLiteAsyncConnection to database.</returns>
    let connect dbPath = 
        async {
            let db = SQLiteAsyncConnection(SQLiteConnectionString dbPath)
            let path = db.DatabasePath
            do! db.CreateTableAsync<QueryResult>() |> Async.AwaitTask |> Async.Ignore
            return db
        }

    // TODO: check with C# respository and sync
    /// <summary>
    /// insertQueryResult
    /// </summary>
    /// <remarks>Inserts QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="queryResult">QueryResult.</param>
    /// <returns>ID of row added to database.</returns>
    let insertQueryResultAsync dbPath queryResult = 
        async {
            let! database = connect dbPath
            let obj = convertModelToQueryResult queryResult
            do! database.InsertAsync(obj) |> Async.AwaitTask |> Async.Ignore
            let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
            let rowId = rowIdObj |> int
            return { queryResult with Id = rowId }
        } |> Async.StartAsTask

    // TODO: check with C# respository and sync
    /// <summary>
    /// saveQueryResult
    /// </summary>
    /// <remarks>Saves QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="queryResultObj">QueryResultObject.</param>
    /// <returns>ID of row added to database. This is the QueryResultId.</returns>
    let saveQueryResultAsync dbPath queryResultObj = 
        async {
            let! database = connect dbPath
            let obj = convertModelToQueryResult queryResultObj
            do! database.InsertAsync(obj) |> Async.AwaitTask |> Async.Ignore
            let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
            let rowId = rowIdObj |> int
            return { queryResultObj with Id = rowId }
        } |> Async.StartAsTask

    // TODO: check with C# respository and sync
    /// <summary>
    /// saveQueryResult
    /// </summary>
    /// <remarks>Saves QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="queryResultObj">QueryResultObject.</param>
    /// <returns>ID of row added to database.</returns>
    let saveTermOccurrenceAsync dbPath termOccurrenceObj = 
        async {
            let! database = connect dbPath
            let obj = convertModelToTermOccurrence termOccurrenceObj
            do! database.InsertAsync(obj) |> Async.AwaitTask |> Async.Ignore
            let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
            let rowId = rowIdObj |> int
            return { termOccurrenceObj with Id = rowId }
        } |> Async.StartAsTask

    /// <summary>
    /// getQueryResultsAsync 
    /// </summary>
    /// <remarks>Gets QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <returns>QueryResultObj.</returns>
    let getQueryResultsAsync dbPath = 
        async {
            let! database = connect dbPath
            let! objs = database.Table<QueryResult>().ToListAsync() |> Async.AwaitTask
            return objs |> Seq.toList |> List.map convertQueryResultToModel
        } |> Async.StartAsTask

    /// <summary>
    /// getQueryResultByQueryStringAsync
    /// </summary>
    /// <remarks>Gets QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="queryString">QueryString.</param>
    /// <returns>Some(QueryResltObj) or None.</returns>
    let getQueryResultByStringAsync dbPath queryString = 
        async {
            let! database = connect dbPath
            let! obj = database.Table<QueryResult>().Where(fun q -> q.QueryString = queryString ||
                                                                    q.ReverseQueryString = queryString)
                                                    .FirstOrDefaultAsync() |> Async.AwaitTask
            let opt = CheckForNull(obj)
            if (opt.IsNone) then
                return None
            else
                let objModel = opt.Value |> convertQueryResultToModel
                return Some(objModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getQueryResultByQueryIdAsync
    /// </summary>
    /// <remarks>Gets QueryResult.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="id">QueryResultId.</param>
    /// <returns>Some(QueryResltObj) or None.</returns>
    let getQueryResultByIdAsync dbPath id = 
        async {
            let! database = connect dbPath
            let! obj = database.Table<QueryResult>().Where(fun q -> q.Id = id)
                                                    .FirstOrDefaultAsync() |> Async.AwaitTask
            let opt = CheckForNull(obj)
            if (opt.IsNone) then
                return None
            else
                let objModel = opt.Value |> convertQueryResultToModel
                return Some(objModel)
        } |> Async.StartAsTask

    /// <summary>
    /// getTermOccurrencesByQueryResultIdAsync 
    /// </summary>
    /// <remarks>Gets TermOccurrences for QueryResultId.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="id">QueryResultId.</param>
    /// <returns>Some(TermOccurrenceObj list) or None.</returns>
    let getTermOccurrencesByQueryResultIdAsync dbPath id =
        async {
            let! database = connect dbPath
            let! objList = database.Table<TermOccurrence>().Where(fun t -> t.QueryResultId = id)
                                                           .ToListAsync() |> Async.AwaitTask
            if (objList.Count = 0) then
                return None
            else
                let occList = objList |> Seq.toList |> List.map convertTermOccurrenceToModel
                return Some(occList)
        } |> Async.StartAsTask

    /// <summary>
    /// getTermOccurrencesAsync
    /// </summary>
    /// <remarks>Gets all TermOccurrences.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <returns>Some(TermOccurrenceObj list) or None.</returns>
    let getTermOccurrencesAsync dbPath =
        async {
            let! database = connect dbPath
            let! objList = database.Table<TermOccurrence>().ToListAsync() |> Async.AwaitTask
            if (objList.Count = 0) then
                return None
            else
                let occList = objList |> Seq.toList |> List.map convertTermOccurrenceToModel
                return Some(occList)
        } |> Async.StartAsTask

module ContactRepository =

    // Contacts Tables
    [<Table("Contacts")>]
    [<AllowNullLiteral>]
    type Contact() =
        [<PrimaryKey; AutoIncrement>]
        member val Id = 0 with get, set
        member val AutoSendEmail = false with get, set
        member val FirstName = "" with get, set
        member val LastName = "" with get, set
        member val DisplayName = "" with get, set
        member val Email = "" with get, set

    // Conversion Methods
    let convertModelToContact (obj: ContactObject) : Contact =
        let _contact = new Contact()
        _contact.Id <- obj.Id
        _contact.AutoSendEmail <- obj.AutoSendEmail
        _contact.FirstName <- obj.FirstName
        _contact.LastName <- obj.LastName
        _contact.DisplayName <- obj.DisplayName
        _contact.Email <- obj.Email
        _contact

    let convertContactToModel (obj: Contact) : ContactObject =
        { Id = obj.Id
          AutoSendEmail = obj.AutoSendEmail
          FirstName = obj.FirstName
          LastName = obj.LastName
          DisplayName = obj.DisplayName
          Email = obj.Email }

    /// <summary>
    /// connect
    /// </summary>
    /// <remarks>Creates connection to database.</remarks>
    /// <param name="dbPath"></param>
    /// <returns>SQLiteAsyncConnection to database.</returns>
    let connect dbPath = 
        async {
            let db = SQLiteAsyncConnection(SQLiteConnectionString dbPath)
            let path = db.DatabasePath
            do! db.CreateTableAsync<Contact>() |> Async.AwaitTask |> Async.Ignore
            return db
        }

    /// <summary>
    /// getContactsAsync 
    /// </summary>
    /// <remarks>Gets Contact.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <returns>ContactObj.</returns>
    let getContactsAsync dbPath = 
        async {
            let! database = connect dbPath
            let! objs = database.Table<Contact>().ToListAsync() |> Async.AwaitTask
            return objs |> Seq.toList |> List.map convertContactToModel
        } |> Async.StartAsTask

    /// <summary>
    /// getContactByIdAsync
    /// </summary>
    /// <remarks>Gets Contact.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="id">ContactId.</param>
    /// <returns>Some(ContactObj) or None.</returns>
    let getContactByIdAsync dbPath id = 
        async {
            let! database = connect dbPath
            let! obj = database.Table<Contact>().Where(fun q -> q.Id = id)
                                                .FirstOrDefaultAsync() |> Async.AwaitTask
            let opt = CheckForNull(obj)
            if (opt.IsNone) then
                return None
            else
                let objModel = opt.Value |> convertContactToModel
                return Some(objModel)
        } |> Async.StartAsTask

    /// <summary>
    /// saveQueryResult
    /// </summary>
    /// <remarks>Saves Contact.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="contactObj">ContactObject.</param>
    /// <returns>ID of row added to database. This is the ContactId.</returns>
    let saveContactAsync dbPath contactObj = 
        async {
            let! database = connect dbPath
            let obj = convertModelToContact contactObj
            do! database.InsertAsync(obj) |> Async.AwaitTask |> Async.Ignore
            let! rowIdObj = database.ExecuteScalarAsync("select last_insert_rowid()", [||]) |> Async.AwaitTask
            let rowId = rowIdObj |> int
            return { contactObj with Id = rowId }
        } |> Async.StartAsTask

    /// <summary>
    /// updateContact
    /// </summary>
    /// <remarks>Updates Contact.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="contactObj">ContactObject.</param>
    /// <returns>ID of row updated in database. This is the ContactId.</returns>
    //let updateContactAsync dbPath contactObj = 
    //    async {
    //        let! database = connect dbPath
    //        let obj = convertModelToContact contactObj
    //        do! database.UpdateAsync(obj) |> Async.AwaitTask |> Async.Ignore
    //        let! rowIdObj = database.ExecuteScalarAsync("select last_update_rowid()", [||]) |> Async.AwaitTask
    //        let rowId = rowIdObj |> int
    //        return { contactObj with Id = rowId }
    //    } |> Async.StartAsTask

    /// <summary>
    /// deleteContact
    /// </summary>
    /// <remarks>Deletes Contact.</remarks>
    /// <param name="dbPath">Path to database.</param>
    /// <param name="contactObj">ContactObject.</param>
    /// <returns>ID of row deleted in database. This is the ContactId.</returns>
    //let deleteContactAsync dbPath contactObj = 
    //    async {
    //        let! database = connect dbPath
    //        let obj = convertModelToContact contactObj
    //        do! database.DeleteAsync(obj) |> Async.AwaitTask |> Async.Ignore
    //        let! rowIdObj = database.ExecuteScalarAsync("select last_delete_rowid()", [||]) |> Async.AwaitTask
    //        let rowId = rowIdObj |> int
    //        return { contactObj with Id = rowId }
    //    } |> Async.StartAsTask
        


    
       