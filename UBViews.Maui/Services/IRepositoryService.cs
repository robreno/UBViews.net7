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
        /// <returns>int success flag</returns>
        Task<int> SaveQueryResultAsync(QueryResult queryResult);

        /// <summary>
        /// QueryResultExistsAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>(bool, int) tuple</returns>
        Task<(bool, int)> QueryResultExistsAsync(string queryString);

        /// <summary>
        /// GetQueryResultsAsync
        /// </summary>
        /// <returns>List of QueryResult</returns>
        Task<List<QueryResult>> GetQueryResultsAsync();

        /// <summary>
        /// GetQueryResultByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>QueryResultDto</returns>
        Task<QueryResultDto> GetQueryResultByIdAsync(int id);

        /// <summary>
        /// GetQueryResultByQueryStringAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>QueryResultDto</returns>
        Task<QueryResultDto> GetQueryResultByQueryStringAsync(string queryString);

        /// <summary>
        /// GetTermOccurrencesAsync
        /// </summary>
        /// <returns>List of TermOccurrence</returns>
        Task<List<TermOccurrence>> GetTermOccurrencesAsync();

        /// <summary>
        /// GetTermOccurrencesByQueryResultIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of TermOccurrence</returns>
        Task<List<TermOccurrence>> GetTermOccurrencesByQueryResultIdAsync(int id);

        /// <summary>
        /// GetQueryCommandsAsync
        /// </summary>
        /// <returns>List of QueryCommandDto</returns>
        Task<List<QueryCommandDto>> GetQueryCommandsAsync();

        /// <summary>
        /// GetPostingsAsync
        /// </summary>
        /// <returns>List of PostingList</returns>
        Task<List<PostingList>> GetPostingsAsync();

        /// <summary>
        /// GetPostingByLexemeAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>PostingList</returns>
        Task<PostingList> GetPostingByLexemeAsync(string lexeme);

        /// <summary>
        /// GetTokenOccurrencesAsync
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns>List of TokenOccurrence</returns>
        Task<List<TokenOccurrence>> GetTokenOccurrencesAsync(int postingId);

        /// <summary>
        /// GetTokenStemsAsync
        /// </summary>
        /// <returns>List of TokenStem</returns>
        Task<List<TokenStem>> GetTokenStemsAsync();

        /// <summary>
        /// GetTokenStemAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>TokenStem</returns>
        Task<TokenStem> GetTokenStemAsync(string lexeme);
        #endregion
    }
}
