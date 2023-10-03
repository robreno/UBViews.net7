using System.Xml.Linq;

using static QueryEngine.QueryRepository;
using static QueryEngine.PostingRepository;
using static QueryEngine.Models;
using static QueryEngine.SimpleEnumeratorsEx;

using UBViews.Models.Query;


namespace UBViews.Services
{
    public interface IFSRepositoryService
    {
        public enum InitOptions { QueryDB, PostingDB, All };

        #region Database Initialization
        /// <summary>
        /// InititializeDatabase
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        Task InititializeDatabase(InitOptions options);
        #endregion

        #region Database Repository Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns></returns>
        Task<TokenPostingList> RunQueryAsync(string queryString);

        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResult"></param>
        /// <returns>int success flag</returns>
        Task<int> SaveQueryResultAsync(QueryResult queryResult);

        /// <summary>
        /// SaveQueryResultAsync
        /// </summary>
        /// <param name="queryResultDto"></param>
        /// <returns>int success flag</returns>
        Task<int> SaveQueryResultAsync(QueryResultDto queryResultDto);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryResultElm"></param>
        /// <returns></returns>
        Task<int> SaveQueryResultAsync(XElement queryResultElm);

        /// <summary>
        /// QueryResultExistsAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>(bool, int)</returns>
        Task<(bool, QueryResultDto)> QueryResultExistsAsync(string queryString);

        /// <summary>
        /// GetQueryResultsAsync
        /// </summary>
        /// <returns>QueryResultDto List</returns>
        Task<List<QueryResultDto>> GetQueryResultsAsync();

        /// <summary>
        /// GetQueryResultByQueryStringAsync
        /// </summary>
        /// <param name="queryString"></param>
        /// <returns>QueryResultDto</returns>
        Task<QueryResultLocationsDto> GetQueryResultByStringAsync(string queryString);

        /// <summary>
        ///GetQueryResultByIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>QueryRestultDto</returns>
        Task<QueryResultLocationsDto> GetQueryResultByIdAsync(int id);

        /// <summary>
        /// GetQueryCommandsAsync
        /// </summary>
        /// <returns>List of QueryResultDto</returns>
        Task<List<QueryCommandDto>> GetQueryCommandsAsync();

        /// <summary>
        /// GetPostingsAsync
        /// </summary>
        /// <returns>List of PostingList</returns>
        Task<List<PostingListDto>> GetPostingListsAsync();

        /// <summary>
        /// GetPostingListByLexemeAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>PostingList</returns>
        Task<PostingListDto> GetPostingListByLexemeAsync(string lexeme);

        /// <summary>
        /// GetTermOccurrencesAsync
        /// </summary>
        /// <returns>List of TermOccurrence</returns>
        Task<List<TermOccurrenceDto>> GetTermOccurrencesAsync();

        /// <summary>
        /// GetTermOccurrencesByQueryResultIdAsync
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of TermOccurrence</returns>
        Task<List<TermOccurrenceDto>> GetTermOccurrencesByQueryResultIdAsync(int id);

        /// <summary>
        /// GetTokenOccurrencesAsync
        /// </summary>
        /// <param name="postingId"></param>
        /// <returns>List of TokenOccurrence</returns>
        Task<List<TokenOccurrenceDto>> GetTokenOccurrencesByPostingListIdAsync(int postingId);

        /// <summary>
        /// GetTokenStemsAsync
        /// </summary>
        /// <returns>List of TokenStem</returns>
        Task<List<TokenStemDto>> GetTokenStemsAsync();

        /// <summary>
        /// GetTokenStemAsync
        /// </summary>
        /// <param name="lexeme"></param>
        /// <returns>TokenStem</returns>
        Task<TokenStemDto> GetTokenStemAsync(string lexeme);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        Task<List<PostingListDto>> GetPostingListsByStemAsync(string stem);

        /// <summary>
        /// GetTokenOccurrencesByPostingListIdAsync
        /// </summary>
        /// <param name="stem"></param>
        /// <returns></returns>
        Task<List<TokenOccurrenceDto>> GetTokenOccurrencesByStemAsync(string stem);
        #endregion
    }
}
