using System.Text;
using System.Text.RegularExpressions;

using UBViews.Services;
using UBViews.Helpers;
using UBViews.Models.Query;
using UBViews.Views;

using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace UBViews.ViewModels;

[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class MainViewModel : BaseViewModel
{
    QueryInputDto _queryInput;
    ConnectivityViewModel connectivityViewModel;
    IConnectivity connectivity;
    IFileService fileService;
    public MainViewModel(IFileService fileService, IConnectivity connectivity)
    {
        _queryInput = new QueryInputDto();
        this.connectivity = Connectivity.Current;
        connectivityViewModel = new ConnectivityViewModel(this.connectivity);
        this.fileService = fileService;
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    int tokenCount;

    [ObservableProperty]
    string searchInput;

    [ObservableProperty]
    string queryInput;

    [ObservableProperty]
    string partId;

    [RelayCommand]
    async Task MainPageAppearing()
    {
        try
        {
            QueryInput = queryInput;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainPageAppearing => ",
                ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task SumbitQuery(string queryString)
    {
        try
        {
            if (IsBusy)
                return;

            IsBusy = true;

            if (queryString == null || queryString == string.Empty)
            {
                _queryInput.Text = "Empty Query";
                _queryInput.TokenCount = 0;
                QueryInput = await App.Current.MainPage.DisplayPromptAsync("Query empty", 
                    "Bad query, enter a valid query");
            }
            else
            {
                await NormalizeQueryString(queryString);
                await Shell.Current.GoToAsync($"{nameof(QueryInputPage)}?TokeCount={_queryInput.TokenCount}",
                    new Dictionary<string, object>
                    {
                        ["QueryInput"] = _queryInput
                    });
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.SumbitQuery => ", 
                ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task CheckInternet()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            var hasInternet = await connectivityViewModel.CheckInternet();
            await App.Current.MainPage.DisplayAlert("Has Internet", hasInternet ? "YES!" : "NO!", "OK");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.CheckInternet => ",
                ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task NavigateTo(string target)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            string targetName = string.Empty;
            if (target == "PartsPage")
            {
                targetName = nameof(PartsPage);
            }
            else if (target == "PaperTitles")
            {
                targetName = nameof(PaperTitlesPage);
            }
            else if (target == "AppSettings")
            {
                targetName = nameof(AppSettingsPage);
            }
            else if (target == "AppData")
            {
                targetName = nameof(AppDataPage);
            }
            else
            {
                targetName = nameof(MainPage);
            }
            await Shell.Current.GoToAsync(targetName);
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

    private async Task NormalizeQueryString(string queryString)
    {
        try
        {
            string qs = "";
            qs = queryString.ToLower();
            string[] tokens = Regex.Split(qs, "\\s+");
            StringBuilder sb = new StringBuilder();
            foreach (var t in tokens)
            {
                sb.Append(t + " ");
            }
            qs = sb.ToString().Trim();
            _queryInput = new QueryInputDto() { Text = qs, TokenCount = tokens.Length };
            QueryInput = _queryInput.Text;
            TokenCount = _queryInput.TokenCount;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NormalizeQueryString => ",
                ex.Message, "Cancel");
        }
    }
}
