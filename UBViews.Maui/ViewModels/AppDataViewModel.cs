using System.Collections.ObjectModel;

using UBViews.Services;
using UBViews.Models;

namespace UBViews.ViewModels;

public partial class AppDataViewModel : BaseViewModel
{
    private string[] srcNames = new string[]
    {
        "UBViews/Query/Queries.template.xml",
        "UBViews/Query/Queries.xml",
        "UBViews/Query/QueryCmdList.xml"
    };
    private string[] trgNames = new string[]
    {
            "UserQueries.xml",
            "QueryHistory.xml",
            "QueryCommands.xml"
    };

    public ObservableCollection<AppFile> DataFiles { get; } = new();
    private IAppDataService appDataService;
    public AppDataViewModel(IAppDataService appDataService)
    {
        this.appDataService = appDataService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    public string sourceNames;

    [ObservableProperty]
    public string targetNames;

    [RelayCommand]
    async Task RefreshingView()
    {
        try
        {
            IsBusy = true;

            await LoadData();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in RefreshingView.NavigateTo => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task AppDataPageAppearing()
    {
        try
        {
            IsBusy = true;

            if (DataFiles.Count != 0)
                return;

            await LoadData();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in SettingsViewModel.SettingsPageAppearing => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task SaveData()
    {
        try
        {
            IsBusy = true;

            await SaveAppData();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in SettingsViewModel.Save => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    async Task LoadData()
    {
        try
        {
            IsBusy = true;

            if (DataFiles.Count != 0)
                return;

            var files = await appDataService.GetAppFilesAsync();
            foreach (var file in files)
                DataFiles.Add(file);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in SettingsViewModel.LoadData => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    async Task SaveAppData()
    {
        try
        {
            IsBusy = true;
            // NotImpl;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in SettingsViewModel.SaveSettings => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}