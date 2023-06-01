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
            try
            {
                return await QueryRepository.QueryResultExistsAsync(queryString);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised => QueryResultExistsAsync.", ex.Message, "Cancel");
                return (false, -1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<QueryResult>> GetQueryResultsAsync()
        {
            try
            {
                return await QueryRepository.GetQueryResultsAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
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
            try
            {
                return await QueryRepository.GetQueryResultByQueryStringAsync(queryString);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised => GetQueryResultByQueryStringAsync.", ex.Message, "Cancel");
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
            try
            {
                return await QueryRepository.GetQueryResultByIdAsync(queryResultId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised => GetQueryResultByIdAsync.", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<QueryCommand>> GetQueryCommandsAsync()
        {
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
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<PostingList>> GetPostingsAsync()
        {
            return await PostingRepository.GetPostingsAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public async Task<PostingList> GetPostingByLexemeAsync(string lexeme)
        {
            return await PostingRepository.GetPostingByLexemeAsync(lexeme);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TermOccurrence>> GetTermOccurrencesAsync()
        {
            return await QueryRepository.GetTermOccurrencesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResultId"></param>
        /// <returns></returns>
        public async Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int queryResultId)
        {
            try
            {
                return await QueryRepository.GetTermOccurrencesByQueryResultIdAsync(queryResultId);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised => GetTermOccurrencesByQueryResultIdAsync.", ex.Message, "Cancel");
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
            return await PostingRepository.GetTokenOccurrencesByPostingListIdAsync(postingId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<List<TokenStem>> GetTokenStemsAsync()
        {
            return await PostingRepository.GetTokenStemsAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        public async Task<TokenStem> GetTokenStemAsync(string lexeme)
        {
            return await PostingRepository.GetTokenStemAsync(lexeme);
        }
        #endregion
    }
}
