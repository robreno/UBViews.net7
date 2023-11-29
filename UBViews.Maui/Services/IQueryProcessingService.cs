namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using QueryEngine;
using UBViews.Models.Query;

public interface IQueryProcessingService
{
    Task<bool> ParseQueryAsync(string queryString);
    Task<bool> RunQueryAsync(string queryString);
    Task<(bool, QueryResultDto)> QueryResultExistsAsync(string queryString);
    Task<SimpleEnumeratorsEx.TokenPostingList> CreateTokenPostingListAsync(string trm, List<TokenOccurrenceDto> toks);
    Task<bool> GetQueryResultExistsAsync();
    Task<string> GetQueryInputStringAsync();
    Task<string> GetQueryExpressionAsync();
    Task<List<string>> GetTermListAsync();
    Task<QueryResultLocationsDto> GetQueryResultLocationsAsync();
}
