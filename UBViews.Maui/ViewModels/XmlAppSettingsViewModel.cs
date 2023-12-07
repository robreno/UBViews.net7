namespace UBViews.ViewModels;

    using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Views;

public partial class XmlAppSettingsViewModel : BaseViewModel
{
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
    string previousAudioFolderName;
    string previousAudioFolderPath;
    int previousWindowSize;
    bool settingsDirty;

    private IAppSettingsService settingsService;

    public XmlAppSettingsViewModel(IAppSettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    double lineHeight;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    bool useCaching;

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
    string audioFolderName = string.Empty;

    [ObservableProperty]
    string audioFolderPath = string.Empty;

    // RelayCommands

    [RelayCommand]
    async Task AppSettingPageAppearing()
    {
        try
        {
            await LoadSettings();
            var audioPathBorder = contentPage.FindByName("audioPathBorder") as Border;
            var playbackControlsHSL = contentPage.FindByName("PlaybackControlsHSL") as HorizontalStackLayout;
            var lineHeightHSL = contentPage.FindByName("LineHeightHSL") as HorizontalStackLayout;
            var windowSizeHSL = contentPage.FindByName("WindowSizeHSL") as HorizontalStackLayout;
            //var autoSendEmailHSL = contentPage.FindByName("AutoSendEmailHSL") as HorizontalStackLayout;
#if WINDOWS
            audioPathBorder.IsVisible = false;
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
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AppSettingPageLoaded()
    {
        try
        {
            // Do nothing for now
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AppSettingPageDisappearing()
    {
        try
        {
            if (settingsDirty == true)
            {
                await SaveCacheSettings();
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task MaxQueryResultValueChanged(double value)
    {
        try
        {
            previousMaxQuery = MaxQueryResults;
            MaxQueryResults = Convert.ToInt32(value);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task LineHeightValueChanged(double value)
    {
        try
        {
            previousLineHeight = LineHeight;
            LineHeight = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPidsCheckedChanged(bool value)
    {
        try
        {
            previousShowPids = ShowReferencePids;
            ShowReferencePids  = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task UseCachingCheckedChanged(bool value)
    {
        try
        {
            previousUseCaching = UseCaching;
            UseCaching = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPlaybackControlsCheckedChanged(bool value)
    {
        try
        {
            previousShowPlaybackControls = ShowPlaybackControls;
            ShowPlaybackControls = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task ShowPaperContentsCheckedChanged(bool value)
    {
        try
        {
            previousShowPaperContents = ShowPaperContents;
            ShowPaperContents = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task AutoSendEmailCheckedChanged(bool value)
    {
        try
        {
            previousAutoSendEmail = AutoSendEmail;
            AutoSendEmail = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.AutoSendEmail => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task WindowSizeSelectedIndexChanged(int value)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task LoadSettings()
    {
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
            AudioFolderName = previousAudioFolderName = await settingsService.Get("audio_folder_name", "");
            AudioFolderPath = previousAudioFolderPath = await settingsService.Get("audio_folder_path", "");
            settingsDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task PickAudioFolder()
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SaveSettings()
    {
        try
        {
            if (previousUseCaching != UseCaching)
            {
                await settingsService.SetCache("use_caching", UseCaching);
                settingsDirty = true;
            }
            if (previousMaxQuery != MaxQueryResults)
            {
                await settingsService.SetCache("max_query_results", MaxQueryResults);
                settingsDirty = true;
            }
            if (previousShowPids != ShowReferencePids)
            {
                await settingsService.SetCache("show_reference_pids", ShowReferencePids);
                settingsDirty = true;
            }
            if (previousLineHeight != LineHeight)
            {
                await settingsService.SetCache("line_height", LineHeight);
                settingsDirty = true;
            }
            if (previousShowPaperContents != ShowPaperContents)
            {
                await settingsService.SetCache("show_paper_contents", ShowPaperContents);
                settingsDirty = true;
            }
            if (previousShowPlaybackControls != ShowPlaybackControls)
            {
                await settingsService.SetCache("show_playback_controls", ShowPlaybackControls);
                settingsDirty = true;
            }
            if (previousAutoSendEmail != AutoSendEmail)
            {
                await settingsService.SetCache("auto_send_email", AutoSendEmail);
                settingsDirty = true;
            }
            if (previousWindowSize != WindowSize)
            {
                await settingsService.SetCache("window_size", WindowSize);
                settingsDirty = true;
            }
            if (previousAudioFolderPath != AudioFolderPath)
            {
                await settingsService.SetCache("audio_folder_name", AudioFolderName);
                await settingsService.SetCache("audio_folder_path", AudioFolderPath);
                settingsDirty = true;
            }
            if (settingsDirty && !UseCaching)
            {
                await SaveCacheSettings();
            }
            await App.Current.MainPage.DisplayAlert("Settings", "Settings were saved!", "Ok");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task SaveCacheSettings()
    {
        try
        {
            await settingsService.SaveCache();
            settingsDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    async Task NavigateTo(string target)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NavigateTo => ",
                ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
        }
    }
    async Task<(bool, string)> PickFolderAsync(CancellationToken cancellationToken)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
            return (false, null);
        }
    }
}
