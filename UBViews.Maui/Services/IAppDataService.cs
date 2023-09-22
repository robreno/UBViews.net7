using UBViews.Models.XmlAppData;

namespace UBViews.Services;

public interface IAppDataService
{
    Task<string> LoadAppDataAsync(string filename);
    Task SaveAppDataAsync(string filename, string content);
    Task<List<AppFileDto>> GetAppFilesAsync();
    Task<(bool, int)> QueryResultExistsAsync(string query);
    Task<QueryResultLocationsDto> GetQueryResultByIdAsync(int queryId);
    Task<QueryResultLocationsDto> GetQueryResultAsync(string query);
    Task<List<QueryCommandDto>> GetQueryCommandsAsync();
}
