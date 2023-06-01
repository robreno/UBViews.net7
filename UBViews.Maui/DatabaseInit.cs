using System.Reflection;
using System.Xml.Linq;
using UBViews.Helpers;
using UBViews.Services;

using UBViews.SQLiteRepository;
using UBViews.SQLiteRepository.Models;

// See: https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/database-sqlite

namespace UBViews
{
    public partial class AppShell
    {
        #region Database Paths and Data Members 
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
        private string _appLocalState = FileSystem.Current.AppDataDirectory;
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalCache
        private string _appLocalCache = FileSystem.Current.CacheDirectory;

        private string _plDatabaseName = "postingLists.db3";
        private string _postingsPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "postingLists.db3");

        private string _qrDatabaseName = "queryResults.db3";
        private string _queriesPathName = Path.Combine(FileSystem.Current.AppDataDirectory, "queryResults.db3");
        #endregion

        #region Database Initialization and Table Generation Methods
        public async Task InitializeData()
        {
            // Copy Posting Lists Database
            if (!File.Exists(_postingsPathName))
                await CopyDatabase(_plDatabaseName, _postingsPathName);
            // Copy Query Results Database
            if (!File.Exists(_queriesPathName))
                await CopyDatabase(_qrDatabaseName, _queriesPathName);

            return;
        }
        public async Task CopyDatabase(string databaseName, string targetPath)
        {
            try
            {
                var assembly = IntrospectionExtensions.GetTypeInfo(typeof(App)).Assembly;
                var manfests = assembly.GetManifestResourceNames();
                string rootPath = "UBViews.Resources.Raw.Database.";
                using (Stream stream = assembly.GetManifestResourceStream(rootPath + databaseName))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        File.WriteAllBytes(targetPath, ms.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised in AppInit.CopyDatabase => ",
                    ex.Message, "Cancel");
            }
            return;
        }
        public async Task CreateQueryResultsDB()
        {
            await QueryRepository.InitializeDatabase();
        }
        public async Task AddQueryData(IFileService fileService)
        {
            var queryResultsContent = await fileService.LoadAsset("QueryResults.xml");
            var xDocQueries = XDocument.Parse(queryResultsContent);
            var queryRoot = xDocQueries.Root;
            var queries = queryRoot.Descendants("QueryResult");

            foreach (var query in queries)
            {
                var queryLocations = query.Descendants("QueryLocation");
                var queryLocationCount = queryLocations.Count();
                var queryType = query.Attribute("type").Value;
                var queryProximity = query.Attribute("proximity").Value;
                var queyTerms = query.Attribute("terms").Value;
                var queryString = query.Descendants("QueryString").FirstOrDefault().Value;
                var queryExpression = query.Descendants("QueryExpression").FirstOrDefault().Value;
                QueryResult queryResult = new QueryResult
                {
                    Hits = queryLocations.Count(),
                    Type = queryType,
                    Terms = queyTerms,
                    Proximity = queryProximity,
                    QueryString = queryString,
                    QueryExpression = queryExpression
                };
                int qrSuccess = await QueryRepository.SaveQueryResultAsync(queryResult);

                foreach (var queryLocation in queryLocations)
                {
                    var pid = queryLocation.Attribute("pid").Value;
                    var termOccurrences = queryLocation.Descendants("TermOccurrence");
                    foreach (var occ in termOccurrences)
                    {
                        var term = occ.Attribute("term").Value;
                        var docId = occ.Attribute("docId").Value;
                        var seqid = occ.Attribute("seqId").Value;
                        var dpoId = occ.Attribute("dpoId").Value;
                        var topId = occ.Attribute("tpoId").Value;
                        var len = occ.Attribute("len").Value;
                        TermOccurrence termOccurrence = new TermOccurrence
                        {
                            QueryResultId = queryResult.Id,
                            DocumentId = Int32.Parse(docId),
                            SequenceId = Int32.Parse(seqid),
                            DocumentPosition = Int32.Parse(dpoId),
                            TextPosition = Int32.Parse(topId),
                            TextLength = Int32.Parse(len),
                            ParagraphId = pid,
                            Term = term
                        };
                        int toSuccess = await QueryRepository.SaveTermOccurrenceAsync(termOccurrence);
                    }
                }

            }
            return;
        }
        public async Task CreatePostingListsDB()
        {
            await PostingRepository.InitializeDatabase();
        }
        public async Task AddPostingData(IFileService fileService)
        {
            var stemmedContent = await fileService.LoadAsset("StemmedLexiconLookupEx.xml");
            var xDocStems = XDocument.Parse(stemmedContent);
            var stemsRoot = xDocStems.Root;
            var stems = stemsRoot.Descendants("Token");

            foreach (var stemItem in stems)
            {
                var tokenLexeme = stemItem.Attribute("lexeme").Value;
                var tokenStemmed = stemItem.Attribute("stemmed").Value;
                if (tokenLexeme != tokenStemmed)
                {
                    TokenStem stem = new TokenStem
                    {
                        Lexeme = tokenLexeme,
                        Stemmed = tokenStemmed
                    };
                    int sSuccess = await PostingRepository.SaveTokenStem(stem);
                }
            }

            var postingContent = await fileService.LoadAsset("PostingLists/PostingLists.xml");
            var xDocPostings = XDocument.Parse(postingContent);
            var postingsRoot = xDocPostings.Root;
            var posts = postingsRoot.Descendants("PostingList");

            foreach (var post in posts)
            {
                var lexeme = post.Attribute("lexeme").Value;

                var posting = new PostingList()
                {
                    Lexeme = lexeme
                };
                int pSuccess = await PostingRepository.SavePostingAsync(posting);

                var occurences = post.Descendants("TokenOccurrence");
                foreach (var occ in occurences)
                {
                    var occurrence = new TokenOccurrence()
                    {
                        PostingId = posting.Id,
                        DocumentId = Int32.Parse(occ.Attribute("did").Value),
                        SequenceId = Int32.Parse(occ.Attribute("sid").Value),
                        DocumentPosition = Int32.Parse(occ.Attribute("dpo").Value), // TODO: regen db, as spelling error on field in database
                        TextPosition = Int32.Parse(occ.Attribute("tpo").Value)
                    };
                    int oSuccess = await PostingRepository.SaveTokenOccurenceAsync(occurrence);
                }
            }
            return;
        }
        #endregion
    }
}
