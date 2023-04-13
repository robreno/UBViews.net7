using UBViews.Models;
using UBViews.Models.Query;

namespace UBViews.Services;

public interface IAppDataService
{
    Task<string> LoadAppDataAsync(string filename);
    Task SaveAppDataAsync(string filename, string content);
    Task<List<AppFile>> GetAppFilesAsync();
    Task<(bool, int)> QueryResultExistsAsync(string query);
    Task<QueryResult> GetQueryResultByIdAsync(int queryId);
    Task<QueryResult> GetQueryResultAsync(string query);
    Task<List<QueryCommand>> GetQueryCommandsAsync();
}
