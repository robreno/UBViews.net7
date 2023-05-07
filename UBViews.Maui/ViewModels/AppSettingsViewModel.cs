using UBViews.Services;

namespace UBViews.ViewModels;

public partial class AppSettingsViewModel : BaseViewModel
{
    int previousMaxQuery;
    double previousLineHeight;
    bool previousShowPids;
    bool previousShowPaperContents;
    bool previousShowPlaybackControls;

    private IAppSettingsService settingsService;

    public AppSettingsViewModel(IAppSettingsService settingsService)
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
    bool showPaperContents;

    [ObservableProperty]
    bool showPlaybackControls;

    [RelayCommand]
    async Task AppSettingPageAppearing()
    {
        try
        {
            await LoadSettings();
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
    async Task LoadSettings()
    {
        try
        {
            MaxQueryResults = previousMaxQuery = await settingsService.Get("max_query_results", 50);
            ShowReferencePids = previousShowPids = await settingsService.Get("show_reference_pids", false);
            LineHeight = previousLineHeight = await settingsService.Get("line_height", 1.0);
            ShowPaperContents = previousShowPaperContents = await settingsService.Get("show_paper_contents", false);
            ShowPlaybackControls = previousShowPlaybackControls = await settingsService.Get("show_playback_controls", false);
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
            if (previousMaxQuery != MaxQueryResults)
            {
                await settingsService.SetCache("max_query_results", MaxQueryResults);
            }
            if (previousShowPids != ShowReferencePids)
            {
                await settingsService.SetCache("show_reference_pids", ShowReferencePids);
            }
            if (previousLineHeight != LineHeight)
            {
                await settingsService.SetCache("line_height", LineHeight);
            }
            if (previousShowPaperContents != ShowPaperContents)
            {
                await settingsService.SetCache("show_paper_contents", ShowPaperContents);
            }
            if (previousShowPlaybackControls != ShowPlaybackControls)
            {
                await settingsService.SetCache("show_playback_controls", ShowPlaybackControls);
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
