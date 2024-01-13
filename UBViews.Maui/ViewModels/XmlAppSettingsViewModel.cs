namespace UBViews.ViewModels;

using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Views;

public partial class XmlAppSettingsViewModel : BaseViewModel
{
    #region  Private Data Members
    const int SMALL = 0;
    const int MEDIUM = 1;
    const int LARGE = 2;

    Dictionary<int, (int,int)> WindowDimensions = new Dictionary<int, (int,int)>()
    {
        { 0, (1080, 920) },
        { 1, (880,720) },
        { 2, (680,520) }
    };

    /// <summary>
    /// 
    /// </summary>
    public ContentPage contentPage;

    bool previousUseCaching;
    int previousMaxQuery;
    double previousLineHeight;
    bool previousShowPids;
    bool previousShowPaperContents;
    bool previousShowPlaybackControls;
    bool previousAutoSendEmail;
    bool previousRunPreCheckSilent;
    string previousAudioDownlandStatus;
    bool previousUseDefaultAudioPath;
    string previousAudioFolderName;
    string previousAudioFolderPath;
    int previousWindowSize;

    private IAppSettingsService settingsService;

    readonly string _class = nameof(XmlAppSettingsViewModel);
    #endregion

    #region Constructor
    public XmlAppSettingsViewModel(IAppSettingsService settingsService)
    {
        this.settingsService = settingsService;
    }
    #endregion

    #region Public Observable Properties
    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    bool settingsDirty;

    [ObservableProperty]
    double lineHeight;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool useCaching;

    [ObservableProperty]
    bool runPreCheckSilent;

    [ObservableProperty]
    bool showPaperContents;

    [ObservableProperty]
    bool showPlaybackControls;

    [ObservableProperty]
    bool autoSendEmail;

    [ObservableProperty]
    bool autoSendEmailList;

    [ObservableProperty]
    int windowSize;

    [ObservableProperty]
    string audioDownloadStatus;

    [ObservableProperty]
    bool useDefaultAudoPath;

    [ObservableProperty]
    string audioFolderName = string.Empty;

    [ObservableProperty]
    string audioFolderPath = string.Empty;
    #endregion

    #region Public Relay Commands
    [RelayCommand]
    async Task AppSettingPageAppearing()
    {
        string _method = "AppSettingPageAppearing";
        try
        {
            await LoadSettingsAsync();
            var audioPathBorder = contentPage.FindByName("audioPathBorder") as Border;
            var playbackControlsHSL = contentPage.FindByName("PlaybackControlsHSL") as HorizontalStackLayout;
            var lineHeightHSL = contentPage.FindByName("LineHeightHSL") as HorizontalStackLayout;
            var windowSizeHSL = contentPage.FindByName("WindowSizeHSL") as HorizontalStackLayout;
            //var autoSendEmailHSL = contentPage.FindByName("AutoSendEmailHSL") as HorizontalStackLayout;
#if WINDOWS
            //audioPathBorder.IsVisible = false;
            playbackControlsHSL.IsVisible = false;
            lineHeightHSL.IsVisible = false;
            windowSizeHSL.IsVisible = false;
            //autoSendEmailHSL.IsVisible = false;
#elif ANDROID
            playbackControlsHSL.IsVisible = false;
            audioPathBorder.IsVisible = false;
            windowSizeHSL.IsVisible = false;
            lineHeightHSL.IsVisible = false;
            //autoSendEmailHSL.IsVisible = false;
#endif
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AppSettingPageLoaded()
    {
        string _method = "AppSettingPageLoaded";
        try
        {
            // Do nothing for now
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AppSettingPageDisappearing()
    {
        string _method = "AppSettingPageDisappearing";
        try
        {
            if (SettingsDirty == true)
            {
                await SaveCacheSettings();
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task MaxQueryResultValueChanged(double value)
    {
        string _method = "MaxQueryResultValueChanged";
        try
        {
            previousMaxQuery = MaxQueryResults;
            MaxQueryResults = Convert.ToInt32(value);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task LineHeightValueChanged(double value)
    {
        string _method = "LineHeightValueChanged";
        try
        {
            previousLineHeight = LineHeight;
            LineHeight = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPidsCheckedChanged(bool value)
    {
        string _method = "ShowPidsCheckedChanged";
        try
        {
            previousShowPids = ShowReferencePids;
            ShowReferencePids  = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task UseCachingCheckedChanged(bool value)
    {
        string _method = "UseCachingCheckedChanged";
        try
        {
            previousUseCaching = UseCaching;
            UseCaching = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPlaybackControlsCheckedChanged(bool value)
    {
        string _method = "ShowPlaybackControlsCheckedChanged";
        try
        {
            previousShowPlaybackControls = ShowPlaybackControls;
            ShowPlaybackControls = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPaperContentsCheckedChanged(bool value)
    {
        string _method = "ShowPaperContentsCheckedChanged";
        try
        {
            previousShowPaperContents = ShowPaperContents;
            ShowPaperContents = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AutoSendEmailCheckedChanged(bool value)
    {
        string _method = "AutoSendEmailCheckedChanged";
        try
        {
            previousAutoSendEmail = AutoSendEmail;
            AutoSendEmail = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task UseDefaultAudioPathChanged(bool value)
    {
        string _method = "UseDefaultAudioPathChanged";
        try
        {
            previousUseDefaultAudioPath = UseDefaultAudoPath;
            UseDefaultAudoPath = value;

            if (previousUseDefaultAudioPath != UseDefaultAudoPath)
            {
                if (UseDefaultAudoPath)
                {
                    if (AudioFolderPath != "LocalState\\AudioFiles")
                    {
                        previousAudioFolderName = AudioFolderName;
                        AudioFolderName = "AudioFiles";
                        previousAudioFolderPath = AudioFolderPath;
                        AudioFolderPath = "LocalState\\AudioFiles";

                        var lbl = contentPage.FindByName("audioPathLabel") as Label;
                        lbl.Text = "LocalState\\AudioFiles";
                        SettingsDirty = true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task WindowSizeSelectedIndexChanged(int value)
    {
        string _method = "WindowSizeSelectedIndexChanged";
        try
        {
            // 0 small, 1 medium, 2 large
            previousWindowSize = WindowSize;
            WindowSize = value;
            if (value.Equals(0))
            {
                await settingsService.Set("window_size", SMALL);
            }

            if (value.Equals(1))
            {
                await settingsService.Set("window_size", MEDIUM);
            }

            if (value.Equals(2))
            {
                await settingsService.Set("window_size", LARGE);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task PickAudioFolder()
    {
        string _method = "PickAudioFolder";
        try
        {
            CancellationToken cancellationToken = new CancellationToken();
            (bool isSuccess, string value) = await PickFolderAsync(cancellationToken);
            if (isSuccess)
            {
                var arry = value.Split("_", StringSplitOptions.RemoveEmptyEntries);
                previousAudioFolderName = AudioFolderName;
                previousAudioFolderPath = AudioFolderPath;
                AudioFolderName = arry[0];
                AudioFolderPath = arry[1];
            }
            else
            {
                string errorMessage = value;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SaveSettings()
    {
        string _method = "SaveSettings";
        try
        {
            if (previousUseCaching != UseCaching)
            {
                await settingsService.SetCache("use_caching", UseCaching);
                SettingsDirty = true;
            }
            if (previousMaxQuery != MaxQueryResults)
            {
                await settingsService.SetCache("max_query_results", MaxQueryResults);
                SettingsDirty = true;
            }
            if (previousShowPids != ShowReferencePids)
            {
                await settingsService.SetCache("show_reference_pids", ShowReferencePids);
                SettingsDirty = true;
            }
            if (previousLineHeight != LineHeight)
            {
                await settingsService.SetCache("line_height", LineHeight);
                SettingsDirty = true;
            }
            if (previousShowPaperContents != ShowPaperContents)
            {
                await settingsService.SetCache("show_paper_contents", ShowPaperContents);
                SettingsDirty = true;
            }
            if (previousShowPlaybackControls != ShowPlaybackControls)
            {
                await settingsService.SetCache("show_playback_controls", ShowPlaybackControls);
                SettingsDirty = true;
            }
            if (previousAutoSendEmail != AutoSendEmail)
            {
                await settingsService.SetCache("auto_send_email", AutoSendEmail);
                SettingsDirty = true;
            }
            if (previousWindowSize != WindowSize)
            {
                await settingsService.SetCache("window_size", WindowSize);
                SettingsDirty = true;
            }
            if (previousAudioDownlandStatus != AudioDownloadStatus)
            {
                await settingsService.SetCache("audio_download_status", AudioDownloadStatus);
                SettingsDirty = true;
            }
            if (previousUseDefaultAudioPath != UseDefaultAudoPath)
            {
                if (UseDefaultAudoPath)
                {
                    await settingsService.SetCache("use_default_audio_path", true);
                    await settingsService.SetCache("audio_folder_name", "AudioFiles");
                    await settingsService.SetCache("audio_folder_path", "LocalState\\AudioFiles");
                }
                else
                {

                    await settingsService.SetCache("use_default_audio_path", false);
                }
                SettingsDirty = true;
            }
            if (previousAudioFolderPath != AudioFolderPath)
            {
                string[] arr = AudioFolderPath.Split(@"\");
                AudioFolderName = arr[arr.Length - 1];
                await settingsService.SetCache("audio_folder_name", AudioFolderName);
                await settingsService.SetCache("audio_folder_path", AudioFolderPath);
                SettingsDirty = true;
            }
            if (previousRunPreCheckSilent != RunPreCheckSilent)
            {
                await settingsService.SetCache("run_precheck_silent", RunPreCheckSilent);
                SettingsDirty = true;
            }
            if (SettingsDirty && !UseCaching)
            {
                await SaveCacheSettings();
            }
            await App.Current.MainPage.DisplayAlert("Settings", "Settings were saved!", "Ok");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SaveCacheSettings()
    {
        string _method = "SaveCacheSettings";
        try
        {
            await settingsService.SaveCache();
            SettingsDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task NavigateTo(string target)
    {
        string _method = "NavigateTo";
        try
        {
            IsBusy = true;

            string targetName = string.Empty;
            if (target == "AppContacts")
            {
                targetName = nameof(ContactsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "AppData")
            {
                targetName = nameof(AppDataPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else
            {
                targetName = nameof(MainPage);
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }
    #endregion

    #region Private Helper Methods  
    private async Task LoadSettingsAsync()
    {
        string _method = "LoadSettings";
        try
        {
            UseCaching = previousUseCaching = await settingsService.Get("use_caching", false);
            MaxQueryResults = previousMaxQuery = await settingsService.Get("max_query_results", 50);
            ShowReferencePids = previousShowPids = await settingsService.Get("show_reference_pids", false);
            LineHeight = previousLineHeight = await settingsService.Get("line_height", 1.0);
            ShowPaperContents = previousShowPaperContents = await settingsService.Get("show_paper_contents", false);
            ShowPlaybackControls = previousShowPlaybackControls = await settingsService.Get("show_playback_controls", false);
            AutoSendEmail = previousAutoSendEmail = await settingsService.Get("auto_send_email", false);
            WindowSize = previousWindowSize = await settingsService.Get("window_size", LARGE);
            AudioDownloadStatus = previousAudioDownlandStatus = await settingsService.Get("audio_download_status", "");
            AudioFolderName = previousAudioFolderName = await settingsService.Get("audio_folder_name", "");
            AudioFolderPath = previousAudioFolderPath = await settingsService.Get("audio_folder_path", "");
            UseDefaultAudoPath = previousUseDefaultAudioPath = await settingsService.Get("use_default_audio_path", true);
            RunPreCheckSilent = previousRunPreCheckSilent = await settingsService.Get("run_precheck_silent", true);
            SettingsDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<(bool, string)> PickFolderAsync(CancellationToken cancellationToken)
    {
        string _method = "PickFolderAsync";
        try
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            string _name = string.Empty;
            string _path = string.Empty;
            string _value = string.Empty;
            bool _isSuccess = false;
            if (result.IsSuccessful)
            {
                _name = result.Folder.Name;
                _path = result.Folder.Path;
                _value = _name + "_" + _path;
                _isSuccess = true;
            }
            else
            {
                _value = result.Exception.Message;
                _isSuccess = false;
            }
            return (_isSuccess, _value);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return (false, null);
        }
    }
    private async Task DisplayMessageAsync(string source, string message)
    {
        string _method = "DisplayMessageAsync";
        try
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {source} => ", message, "Ok");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion
}
