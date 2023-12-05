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
    Task<bool> IsInitializedAsync();
    Task SetContentPageAsync(ContentPage contentPage);
    Task<bool> PreCheckQueryAsync(string queryString);
    Task SetAudioStreamingAsync(string value);
    Task SetMaxQueryResultsAsync(int max);
    Task<(bool, string)> ParseQueryAsync(string queryString);
    Task<(bool, bool, QueryResultLocationsDto)> RunQueryAsync(string queryString);
    Task<(bool, QueryResultDto)> QueryResultExistsAsync(string queryString);
    Task<bool> GetQueryResultExistsAsync();
    Task<string> GetQueryInputStringAsync();
    Task<string> GetQueryExpressionAsync();
    Task<List<string>> GetTermListAsync();
    Task<QueryResultLocationsDto> GetQueryResultLocationsAsync();
    Task<SimpleEnumeratorsEx.TokenPostingList> CreateTokenPostingListAsync(string trm, List<TokenOccurrenceDto> toks);
}
