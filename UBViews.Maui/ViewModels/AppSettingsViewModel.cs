using UBViews.Services;

namespace UBViews.ViewModels;

public partial class AppSettingsViewModel : BaseViewModel
{
    private IAppSettingsService settingsService;

    int previousMaxQuery;
    double previousLineHeight;
    bool previousShowPids;
    bool previousShowPaperContents;
    public AppSettingsViewModel(IAppSettingsService settingsService)
    {
        this.settingsService = settingsService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    bool showReferencePids;

    [ObservableProperty]
    double lineHeight;

    [ObservableProperty]
    bool showPaperContents;

    [RelayCommand]
    public async Task AppSettingPageAppearing()
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
    public async Task AppSettingPageDisappearing()
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
    public async Task MaxQueryResultValueChanged(double value)
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
    public async Task LineHeightValueChanged(double value)
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
    public async Task ShowPidsCheckedChanged(bool value)
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
    public async Task ShowPaperContentsCheckedChanged(bool value)
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
    public async Task LoadSettings()
    {
        try
        {
            MaxQueryResults = previousMaxQuery = await settingsService.Get("max_query_results", 50);
            ShowReferencePids = previousShowPids = await settingsService.Get("show_pids", false);
            LineHeight = previousLineHeight = await settingsService.Get("line_height", 1.0);
            ShowPaperContents = previousShowPaperContents = await settingsService.Get("show_paper_contents", false);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in AppSettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
    }

    [RelayCommand]
    public async Task SaveSettings()
    {
        try
        {
            if (previousMaxQuery != MaxQueryResults)
            {
                await settingsService.SetCache("max_query_results", MaxQueryResults);
            }
            if (previousShowPids != ShowReferencePids)
            {
                await settingsService.SetCache("show_pids", ShowReferencePids);
            }
            if (previousLineHeight != LineHeight)
            {
                await settingsService.SetCache("line_height", LineHeight);
            }
            if (previousShowPaperContents != showPaperContents)
            {
                await settingsService.SetCache("show_paper_contents", showPaperContents);
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
    public async Task SaveCacheSettings()
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
