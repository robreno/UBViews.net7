namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;
public interface IAppDataService
{
    Task<string> LoadAppDataAsync(string filename);
    Task SaveAppDataAsync(string filename, string content);
    Task SaveAppDataExAsync(string fileName);
    Task<List<AppFileDto>> GetAppFilesAsync();
}
