namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models;
using UBViews.Services;
public class DownloadService : IDownloadService
{
    #region  Data Members
    /// <summary>
    /// ContentPage
    /// </summary>
    private ContentPage contentPage;

    /// <summary>
    /// 
    /// </summary>
    public HttpClient httpClient;

    readonly string _class = "DownloadService";
    #endregion

    #region   Services
    IAppSettingsService settingsService;
    #endregion

    public DownloadService()
    {
        this.settingsService = ServiceHelper.Current.GetService<IAppSettingsService>();
    }

    #region  Public Properties
    public bool ValidAudioDownloadPath { get; set; } = false;
    public bool ValidAudioUriPath { get; set; } = false;
    public bool AudioFolderExists { get; set; } = false;
    public bool AudioFileExists { get; set; } = false;
    public bool Initialized { get; set; } = false;
    public PaperDto PaperDto { get; set; } = null;
    public string AudioUriString { get; set; } = null;
    public Uri AudioUri { get; set; } = null;
    public string PaperName { get; set; } = null;
    public string AudioDownloadPath { get; set; } = null;
    public string AudioDownloadFullPathName { get; set; } = null;
    public string LocalStatePath { get; set; } = FileSystem.AppDataDirectory;
    #endregion

    #region  Interface Implementations
    public async Task  InitializeDataAsync(ContentPage contentPage, PaperDto dto)
    {
        string _method = "InitializeDataAsync";
        try
        {
            this.contentPage = contentPage;
            PaperDto = dto;
            PaperName = PaperDto.Id.ToString("000") + ".mp3";

            this.httpClient = ServiceHelper.Current.GetService<HttpClient>();

            var hasValue = contentPage.Resources.TryGetValue("audioUri", out object uri);
            if (hasValue)
            {
                AudioUriString = (string)uri;
                AudioUri = new Uri(AudioUriString);
                ValidAudioUriPath = true;
            }

            // LocalState/AudioFiles
            // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
            string audioFolderPath = await settingsService.Get("audio_folder_path", "");
            string audioFolderName = await settingsService.Get("audio_folder_name", "");
            if (!string.IsNullOrEmpty(audioFolderPath) && !string.IsNullOrEmpty(audioFolderName))
            {
                if (audioFolderName.Equals("[Empty]"))
                {
                    ValidAudioDownloadPath = false;
                }
                else if (audioFolderPath == "LocalState\\AudioFiles")
                {
                    AudioDownloadPath = Path.Combine(LocalStatePath, audioFolderName);
                    AudioFolderExists = Directory.Exists(AudioDownloadPath);
                    if (!AudioFolderExists)
                    {
                        System.IO.Directory.CreateDirectory(AudioDownloadPath);
                    }
                    ValidAudioDownloadPath = true;
                }
                else
                {
                    AudioDownloadPath = audioFolderPath;
                    ValidAudioDownloadPath = true;
                }

                if (ValidAudioDownloadPath)
                {
                    AudioDownloadFullPathName = Path.Combine(AudioDownloadPath, PaperName);
                    AudioFileExists = File.Exists(AudioDownloadFullPathName);
                }
            }
            Initialized = (ValidAudioUriPath && ValidAudioDownloadPath);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task<bool> DownloadAudioFileAsync()
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;
            if (File.Exists(AudioDownloadFullPathName))
            {
                isSuccess = true;
            }
            else
            {
                if (ValidAudioDownloadPath)
                {
                    var response = await httpClient.GetAsync(AudioUri);
                    var result = response.EnsureSuccessStatusCode();
                    if (result.IsSuccessStatusCode)
                    {
                        var stream = await response.Content.ReadAsStreamAsync();
                        var fileInfo = new FileInfo(AudioDownloadFullPathName);
                        using (var fileStream = fileInfo.OpenWrite())
                        {
                            await stream.CopyToAsync(fileStream);
                        }
                        isSuccess = true;
                    }
                    else
                    {
                        throw new Exception("File not found.");
                    }
                    return isSuccess;
                }
                else
                {
                    throw new Exception("Invalid Audio Path.");
                }
            }
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }

    public async Task<bool> DownloadAudioFileExAsync()
    {
        string _method = "DownloadAudioFileExAsync";
        try
        {
            bool isSuccess = false;

            if (ValidAudioDownloadPath)
            {
                var response = await httpClient.GetAsync(AudioUri);
                var result = response.EnsureSuccessStatusCode();
                if (result.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(AudioDownloadFullPathName);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    isSuccess = true;
                }
                else
                {
                    throw new Exception("File not found.");
                }
                return isSuccess;
            }
            else
            {
                throw new Exception("Invalid Audio Path.");
            }

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    public async Task<bool> DownloadAudioFileAsync(string fileName, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;

            // Setup Uri and File Path
  
            string uriBasePath = AudioUriString;
            string uriFullPath = uriBasePath + fileName;
            Uri uri = new Uri(uriFullPath);

            string fileNamePath = Path.Combine(audioDir, "000.mp3");
            
            var response = await httpClient.GetAsync(uri);
            var result = response.EnsureSuccessStatusCode();
            if (result.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var fileInfo = new FileInfo(fileNamePath);
                using (var fileStream = fileInfo.OpenWrite())
                {
                    await stream.CopyToAsync(fileStream);
                }
                isSuccess = true;
            }
            else
            {
                throw new Exception("File not found");
            }
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    public async Task<bool> DownloadAudioFileAsync(Uri uri, string audioDir)
    {
        string _method = "DownloadAudioFileAsync";
        try
        {
            bool isSuccess = false;
            // Setup Uri and File Path
            string fileName = PaperDto.Id.ToString("000") + ".mp3";
            string fileNamePath = Path.Combine(audioDir, fileName);

            var response = await httpClient.GetAsync(uri);
            var result = response.EnsureSuccessStatusCode();
            if (result.IsSuccessStatusCode)
            {
                var stream = await response.Content.ReadAsStreamAsync();
                var fileInfo = new FileInfo(fileNamePath);
                using (var fileStream = fileInfo.OpenWrite())
                {
                    await stream.CopyToAsync(fileStream);
                }
                isSuccess = true;
            }
            else
            {
                throw new Exception("File not found");
            }
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    #endregion
}
