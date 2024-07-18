namespace UBViews.ViewModels;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Models.AppData;

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

    readonly string _class = "AppDataViewModel";

    public ObservableCollection<AppFileDto> DataFiles { get; } = new();
    private IAppDataService appDataService;
    public AppDataViewModel(IAppDataService appDataService)
    {
        this.appDataService = appDataService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    string name;

    [ObservableProperty]
    string path;

    [ObservableProperty]
    string folder;

    [ObservableProperty]
    long length;

    [ObservableProperty]
    string size;

    [ObservableProperty]
    DateTime created;

    [ObservableProperty]
    string dateCreated;

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
            {
                return;
            }
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
    async Task SelectionChanged(object selectedItem)
    {
        string _method = "SelectionChanged";

        try
        {
            if (selectedItem == null)
            {
                return;
            }

            var item = selectedItem as AppFileDto;

            Name = item.Name;
            Folder = item.Folder;
            Path = item.Path;
            Length = item.Length;
            Size = item.Size;
            Created = item.Created;
            DateCreated = Created.ToShortTimeString();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
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
            {
                DataFiles.Add(file);
            }
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
            throw new NotImplementedException();
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