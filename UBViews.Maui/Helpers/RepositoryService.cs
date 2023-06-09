using UBViews.Services;

using UBViews.Models.Query;
using UBViews.SQLiteRepository;
using UBViews.SQLiteRepository.Dtos;
using UBViews.SQLiteRepository.Models;

namespace UBViews.Helpers
{
    public class RepositoryService : IRepositoryService
    {
        #region Private Members
        private static string _queriesDbPathLocalState = Path.Combine(FileSystem.AppDataDirectory, "queryResults.db3");
        private static string _queriesDbPathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "queryResults.db3");
        private static bool _queryDbExists = false;

        private static string _postingDbPathLocalState = Path.Combine(FileSystem.AppDataDirectory, "postingLists.db3");
        private static string _postingDbPathLocalCache =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "postingLists.db3");
        private static bool _postingDbExists = false;
        #endregion

        #region Database Initialization
        /// <summary>
        /// DatabaseExistsAsync checks to see if database at dbPath exits.
        /// </summary>
        /// <param name="dbPath"></param>
        /// <returns>True if exists, false if does not exist.</returns>
        public async Task<bool> DatabaseExistsAsync(string dbPath)
        {
            try
            {
                return await QueryRepository.DatabaseExistsAsync(dbPath);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised => DatabaseExistsAsync: ", ex.Message, "Cancel");
                return false;
            }
        }

       /// <summary>
       /// Initializes database.
       /// </summary>
       /// <param name="initOptions"></param>
       /// <returns></returns>
        public async Task InititializeDatabase(InitOptions initOptions)
        {
            try
            {
                switch (initOptions)
                {
                    case InitOptions.QueryDB:
                        await QueryRepository.InitializeDatabase();
                        break;
                    case InitOptions.PostingDB:
                        await PostingRepository.InitializeDatabase();
                        break;
                    default:
                        await QueryRepository.InitializeDatabase();
                        await PostingRepository.InitializeDatabase();
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            }
        }
        #endregion

        #region Database Repository Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        public async Task<int> SaveQueryResultAsync(QueryResult queryResult)
        {
            return await QueryRepository.SaveQueryResultAsync(queryResult);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public async Task<(bool, int)> QueryResultExistsAsync(string queryString)
        {
            string methodName = "QueryResultExistsAsync";
            try
            {
                return await QueryRepository.QueryResultExistsAsync(queryString);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return (false, -1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<QueryResult>> GetQueryResultsAsync()
        {
            string methodName = "GetQueryResultsAsync";
            try
            {
                return await QueryRepository.GetQueryResultsAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        public async Task<QueryResultLocations> GetQueryResultByQueryStringAsync(string queryString)
        {
            string methodName = "GetQueryResultByQueryStringAsync";
            try
            {
                return await QueryRepository.GetQueryResultByQueryStringAsync(queryString);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResultId"></param>
        /// <returns></returns>
        public async Task<QueryResultLocations> GetQueryResultByIdAsync(int queryResultId)
        {
            string methodName = "GetQueryResultByIdAsync";
            try
            {
                return await QueryRepository.GetQueryResultByIdAsync(queryResultId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<QueryCommand>> GetQueryCommandsAsync()
        {
            string methodName = "GetQueryCommandsAsync";
            try
            {
                List<QueryCommand> queryCommands = new List<QueryCommand>();
                var queryResults = await QueryRepository.GetQueryResultsAsync();
                foreach (var result in queryResults)
                {
                    var queryCommand = new QueryCommand
                    {
                        Id = result.Id,
                        Type = result.Type,
                        Proximity = result.Proximity,
                        Terms = result.Terms,
                        QueryString = result.QueryString,
                    };
                    queryCommands.Add(queryCommand);
                }
                return queryCommands;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<PostingList>> GetPostingsAsync()
        {
            string methodName = "GetPostingsAsync";
            try
            {
                return await PostingRepository.GetPostingsAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public async Task<PostingList> GetPostingByLexemeAsync(string lexeme)
        {
            string methodName = "GetPostingByLexemeAsync";
            try
            {
                return await PostingRepository.GetPostingByLexemeAsync(lexeme);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TermOccurrence>> GetTermOccurrencesAsync()
        {
            string methodName = "GetTermOccurrencesAsync";
            try
            {
                return await QueryRepository.GetTermOccurrencesAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResultId"></param>
        /// <returns></returns>
        public async Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int queryResultId)
        {
            string methodName = "GetTermOccurrencesByQueryResultIdAsync";
            try
            {
                return await QueryRepository.GetTermOccurrencesByQueryResultIdAsync(queryResultId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns></returns>
        public async Task<List<TokenOccurrence>> GetTokenOccurrencesByPostingListIdAsync(int postingId)
        {
            string methodName = "GetTokenOccurrencesByPostingListIdAsync";
            try
            {
                return await PostingRepository.GetTokenOccurrencesByPostingListIdAsync(postingId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TokenStem>> GetTokenStemsAsync()
        {
            string methodName = "GetTokenStemsAsync";
            try
            {
                return await PostingRepository.GetTokenStemsAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public async Task<TokenStem> GetTokenStemAsync(string lexeme)
        {
            string methodName = "GetTokenStemsAsync";
            try
            {
                return await PostingRepository.GetTokenStemAsync(lexeme);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
                return null;
            }
        }
        #endregion

        #region Private Helper Methods
        //private async Task<QueryResultLocationsDto> MapQueryResultToDto(QueryResultLocations queryResult)
        //{
        //    string methodName = "MapQueryResultToDto";
        //    try
        //    {
        //        var queryResultDto = new QueryResultLocationsDto()
        //        {
        //            Id = queryResult.Id,
        //            Hits = queryResult.Hits,
        //            Type = queryResult.Type,
        //            Terms = queryResult.Terms,
        //            Proximity = queryResult.Proximity,
        //            QueryString = queryResult.QueryString,
        //            QueryExpression = queryResult.QueryExpression
        //        };

        //        var termOccurrences = await GetTermOccurrencesByQueryResultIdAsync(queryResultDto.Id);
        //        var queryLocations = termOccurrences.GroupBy(g => g.ParagraphId);

        //        foreach (var locations in queryLocations)
        //        {
        //            var id = locations.First().DocumentId + ":" + locations.First().SequenceId;
        //            var queryLocation = new QueryLocationDto() { Id = id, Pid = locations.Key };

        //            foreach (var location in locations)
        //            {
        //                var termLocation = new TermLocationDto()
        //                {
        //                    Term = location.Term,
        //                    DocumentId = location.DocumentId,
        //                    SequenceId = termOccurrences.First().SequenceId,
        //                    DocumentPosition = location.DocumentPosition,
        //                    TextPosition = location.TextPosition,
        //                    TextLength = location.TextLength
        //                };
        //                queryLocation.TermOccurrences.Add(termLocation);
        //            }
        //            queryResultDto.QueryLocations.Add(queryLocation);
        //        }
        //        return queryResultDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        //private async Task<Local.PostingListDto> MapPostingListToDto(PostingList postingList)
        //{
        //    string methodName = "MapPostingListToDto";
        //    try
        //    {
        //        var postingListDto = new Local.PostingListDto()
        //        {
        //            Id = postingList.Id,
        //            Lexeme = postingList.Lexeme
        //        };
        //        return postingListDto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        //private async Task<Local.TermOccurrenceDto> MapTermOccurrenceToDto(TermOccurrence termOccurrence)
        //{
        //    string methodName = "MapTermOccurrenceToDto";
        //    try
        //    {
        //        var dto = new Local.TermOccurrenceDto()
        //        {
        //            Id = termOccurrence.Id,
        //            QueryResultId = termOccurrence.QueryResultId,
        //            DocumentId = termOccurrence.DocumentId,
        //            SequenceId = termOccurrence.SequenceId,
        //            DocumentPosition = termOccurrence.DocumentPosition,
        //            TextPosition = termOccurrence.TextPosition,
        //            TextLength = termOccurrence.TextLength,
        //            ParagraphId = termOccurrence.ParagraphId,
        //            Term = termOccurrence.Term,
        //        };
        //        return dto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        //private async Task<Local.TokenOccurrenceDto> MapTokenOccurrenceToDto(TokenOccurrence tokenOccurrence)
        //{
        //    string methodName = "MapTokenOccurrenceToDto";
        //    try
        //    {
        //        var dto = new Local.TokenOccurrenceDto()
        //        {
        //            Id = tokenOccurrence.Id,
        //            PostingId = tokenOccurrence.PostingId,
        //            DocumentId = tokenOccurrence.DocumentId,
        //            SequenceId = tokenOccurrence.SequenceId,
        //            DocumentPosition = tokenOccurrence.DocumentPosition,
        //            TextPosition = tokenOccurrence.TextPosition
        //        };
        //        return dto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        //private async Task<Local.TokenStemDto> MapTokenStemToDto(TokenStem tokenStem)
        //{
        //    string methodName = "MapTokenStemToDto";
        //    try
        //    {
        //        var dto = new Local.TokenStemDto()
        //        {
        //            Id = tokenStem.Id,
        //            Lexeme = tokenStem.Lexeme,
        //            Stemmed = tokenStem.Stemmed
        //        };
        //        return dto;
        //    }
        //    catch (Exception ex)
        //    {
        //        await App.Current.MainPage.DisplayAlert($"Exception raised => {methodName}.", ex.Message, "Cancel");
        //        return null;
        //    }
        //}
        #endregion
    }
}
