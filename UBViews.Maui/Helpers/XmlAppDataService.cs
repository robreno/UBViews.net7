namespace UBViews.Helpers;

using System.Xml.Linq;

using UBViews.Services;
using UBViews.Models;
using UBViews.Models.AppData;

public class XmlAppDataService : IAppDataService
{
    /// <summary>
    /// Private Data Members
    /// </summary>
    //private const string _appDataFileName = "QueryHistory.xml";
    private XDocument _appData = null;
    private XElement _appDataRoot = null;
    private string _content = null;
    private string _appDir = null;
    private bool _cacheDirty = false;
    private int _fileCount = 0;

    readonly string[] sizeSuffixes = { "Bytes", "Kb", "Mb", "Gb", "Tb", "Pb", "Eb", "Zb", "Yb" };

    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;

    public XmlAppDataService(IFileService fileService)
    {
        this.fileService = fileService;
    }
    public async Task<string> LoadAppDataAsync(string filename)
    {
        try
        {
            string appDir = FileSystem.Current.AppDataDirectory;
            string targetFile = System.IO.Path.Combine(appDir, filename);
            using FileStream inputStream = System.IO.File.OpenRead(targetFile);
            using StreamReader reader = new StreamReader(inputStream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    public async Task SaveAppDataAsync(string filename, string content)
    {
        try
        {
            string targetFile = System.IO.Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            using var stream = System.IO.File.OpenWrite(targetFile);
            using StreamWriter writer = new StreamWriter(stream);
            await writer.WriteAsync(content);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }
    public async Task SaveAppDataExAsync(string fileName)
    {
        try
        {
            if (_cacheDirty)
            {
                string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
                using var stream = File.Create(targetFilePath);
                CancellationToken token = new CancellationToken(false);
                await _appDataRoot.SaveAsync(stream, SaveOptions.None, token);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }
    public async Task<List<AppFileDto>> GetAppFilesAsync()
    {
        try
        {
            string mainDir = FileSystem.Current.AppDataDirectory;
            string[] files = Directory.GetFiles(mainDir);

            List<AppFileDto> appFiles = new List<AppFileDto>();

            int count = 0;
            foreach (string file in files)
            {
                var _fi = new FileInfo(file);
                var _fileName = _fi.Name;
                var _fileLength = _fi.Length;
                var _creationTime = _fi.CreationTime;
                var _ns = _fi.DirectoryName.Normalize();
                var _folderName = _fi.Directory.Name;

                var _size = await GetFileSize(_fileLength);

                var newFile = new AppFileDto
                {
                    Id = ++count,
                    Name = _fileName,
                    Path = _ns,
                    Folder = _folderName,
                    Length = _fileLength,
                    Size = _size,
                    Created = _creationTime
                };
                appFiles.Add(newFile);
            }
            return appFiles;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    #region  Private Methods
    private async Task<string> GetFileSize(long length)
    {
        try
        {
            const string formatTemplate  = "{0}{1:0.#} {2}";
            const string formatTemplate2 = "{0:0.##} {1}";

            if (length == 0)
            {
                return string.Format(formatTemplate, null, 0, sizeSuffixes[0]);
            }

            var absSize = Math.Abs((double)length);
            var fpPower = Math.Log(absSize, 1000);
            var intPower = (int)fpPower;
            var iUnit = intPower >= sizeSuffixes.Length
                ? sizeSuffixes.Length - 1
                : intPower;
            var normSize = absSize / Math.Pow(1000, iUnit);

            return string.Format(
                formatTemplate,
                length < 0 ? "-" : null, normSize, sizeSuffixes[iUnit]);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in SettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
            return null;
        }
    }
    #endregion
}
