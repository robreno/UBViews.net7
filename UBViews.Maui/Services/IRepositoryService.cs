using SQLiteRepository;
using SQLiteRepository.Dtos;
using SQLiteRepository.Models;

namespace UBViews.Services
{
    /// <summary>
    /// Database initialization options allows selective 
    /// initialization of database(s).
    /// </summary>
    public enum InitOptions { QueryDB, PostingDB, All };

    public interface IRepositoryService
    {
        #region Database Initialization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbPath"></param>
        /// <returns></returns>
        Task<bool> DatabaseExistsAsync(string dbPath);
       
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task InititializeDatabase(InitOptions options);
        #endregion

        #region Database Repository Methods
        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResult"></param>
        /// <returns></returns>
        Task<int> SaveQueryResultAsync(QueryResult queryResult);

        /// <summary>
        /// QueryResultExistsAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        Task<(bool, int)> QueryResultExistsAsync(string queryString);

        /// <summary>
        /// GetQueryResultsAsync
        /// </summary>
        /// <returns></returns>
        Task<List<QueryResult>> GetQueryResultsAsync();

        /// <summary>
        /// GetQueryResultByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<QueryResultDto> GetQueryResultByIdAsync(int id);

        /// <summary>
        /// GetQueryResultByQueryStringAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        Task<QueryResultDto> GetQueryResultByQueryStringAsync(string queryString);

        /// <summary>
        /// GetTermOccurrencesAsync
        /// </summary>
        /// <returns></returns>
        Task<List<TermOccurrence>> GetTermOccurrencesAsync();

        /// <summary>
        /// GetTermOccurrencesByQueryResultIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int id);

        /// <summary>
        /// GetQueryCommandsAsync
        /// </summary>
        /// <returns></returns>
        Task<List<QueryCommandDto>> GetQueryCommandsAsync();

        /// <summary>
        /// GetPostingsAsync
        /// </summary>
        /// <returns></returns>
        Task<List<PostingList>> GetPostingsAsync();

        /// <summary>
        /// GetPostingByLexemeAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        Task<PostingList> GetPostingByLexemeAsync(string lexeme);

        /// <summary>
        /// GetTokenOccurrencesAsync
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns></returns>
        Task<List<TokenOccurrence>> GetTokenOccurrencesAsync(int postingId);

        /// <summary>
        /// GetTokenStemsAsync
        /// </summary>
        /// <returns></returns>
        Task<List<TokenStem>> GetTokenStemsAsync();

        /// <summary>
        /// GetTokenStemAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns></returns>
        Task<TokenStem> GetTokenStemAsync(string lexeme);
        #endregion
    }
}
