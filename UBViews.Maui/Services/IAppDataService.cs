namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;
public interface IAppDataService
{
    Task<string> LoadAppDataAsync(string filename);
    Task<List<AppFileDto>> GetAppFilesAsync();
}
