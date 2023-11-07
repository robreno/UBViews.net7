﻿using UBViews.Services;

namespace UBViews.ViewModels;

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
    bool previousAutoSendList;
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

    [RelayCommand]
    async Task AppSettingPageAppearing()
    {
        try
        {
            await LoadSettings();
            var windowSizeHSL = contentPage.FindByName("WindowSizeHSL") as HorizontalStackLayout;
            var playbackControlsHSL = contentPage.FindByName("PlaybackControlsHSL") as HorizontalStackLayout;
            var lineHeightHSL = contentPage.FindByName("LineHeightHSL") as HorizontalStackLayout;
            var autoSendEmailHSL = contentPage.FindByName("AutoSendEmailHSL") as HorizontalStackLayout;
#if WINDOWS
            windowSizeHSL.IsVisible = false;
            playbackControlsHSL.IsVisible = false;
            lineHeightHSL.IsVisible = false;
            autoSendEmailHSL.IsVisible = false;
#elif ANDROID
            playbackControlsHSL.IsVisible = false;
            windowSizeHSL.IsVisible = false;
            lineHeightHSL.IsVisible = false;
            autoSendEmailHSL.IsVisible = false;
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
            await SaveCacheSettings();
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
            settingsDirty = false;
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
            //await App.Current.MainPage.DisplayAlert("Settings", "Cached settings were saved!", "Ok");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
        }
    }
}