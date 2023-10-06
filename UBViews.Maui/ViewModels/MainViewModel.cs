namespace UBViews.ViewModels;

using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


using UBViews.Services;
using UBViews.Helpers;
using UBViews.Models.Query;
using UBViews.Views;
using UBViews.Models;

using QueryEngine;
using QueryFilter;

[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class MainViewModel : BaseViewModel
{
    QueryInputDto _queryInput = new QueryInputDto() { Text = string.Empty, TokenCount = 0 };

    ConnectivityViewModel connectivityViewModel;

    IConnectivity connectivityService;
    IFileService fileService;
    IFSRepositoryService repositoryService;

    ParserService parserService;
    QueryService queryService;

    public MainViewModel(IFileService fileService, IAppDataService appDataSerivce, 
                         IFSRepositoryService repositoryService, IConnectivity connectivityService)
    {
        this.connectivityService = Connectivity.Current;
        connectivityViewModel = new ConnectivityViewModel(this.connectivityService);
        this.fileService = fileService;

        this.repositoryService = repositoryService;
        this.parserService = new ParserService();
        this.queryService = new QueryService();
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    string queryInput;

    [ObservableProperty]
    QueryInputDto queryInputObj;

    [ObservableProperty]
    int tokenCount;

    [ObservableProperty]
    string searchInput;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    XElement queryResult;

    [ObservableProperty]
    bool queryResultExists;

    [ObservableProperty]
    string partId;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

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

            var validChars = QueryFilterService.checkForValidChars(queryString);
            var validCharsSuccess = validChars.Item1;
            var validForm = QueryFilterService.checkForValidForm(queryString);
            var validFormSuccess = validForm.Item1;

            if (queryString == null ||
                    queryString == string.Empty ||
                    !validCharsSuccess ||
                    !validFormSuccess)
            {
                var errorMessage = string.Empty;
                var msg = string.Empty;

                if (!validCharsSuccess || !validFormSuccess)
                {
                    if (!validCharsSuccess)
                        errorMessage = errorMessage + validChars.Item2 + ";";
                    if (!validFormSuccess)
                        errorMessage = errorMessage + validForm.Item2 + ";";

                    msg = $"Bad query at {errorMessage}. Edit and click Ok or cancel query.";
                }

                var result = await App.Current.MainPage.DisplayPromptAsync("Query Error", msg, "OK",
                                                                           "Cancel", null, -1, null, queryString);

                if (result != null)
                {
                    await Shell.Current.GoToAsync($"..?QueryInput={result}");
                }
                else
                {
                    await Shell.Current.GoToAsync("..");
                }

                // Original
                //_queryInput.Text = "Empty Query";
                //_queryInput.TokenCount = 0;
                //QueryInput = await App.Current.MainPage.DisplayPromptAsync("Query empty", 
                //    "Bad query, enter a valid query");
            }
            else
            {
                await NormalizeQueryString(queryString);
                string qryString = _queryInput.Text;

                var _queryInputDto = new QueryInputDto { Text = qryString, TokenCount = _queryInput.TokenCount };

                QueryInputObj = _queryInputDto;
                QueryInputString = qryString;

                // Check if query exists
                (bool queryExists, QueryResultDto dto) = await repositoryService.QueryResultExistsAsync(queryString);
                QueryResultExists = queryExists;
                QueryResultLocationsDto queryResultLocationsDto = null;
                int queryResultId = 0;
                
                if (queryExists)
                {
                    queryResultId = dto.Id;
                    queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(queryResultId);
                    QueryExpression = queryResultLocationsDto.QueryExpression;
                    QueryLocations = queryResultLocationsDto;
                    // Navigate to QueryResultPage here
                    await NavigateTo("QueryResults");
                }
                else
                {
                    // Run New Query
                    // interregnum and wisdom filterby parid
                    // jesus and courtesans filterby parid
                    // resurrection and halls
                }
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
        try
        {
            IsBusy = true;

            string targetName = string.Empty;
            if (target == "PartsPage")
            {
                targetName = nameof(PartsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "PaperTitles")
            {
                targetName = nameof(PaperTitlesPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "AppSettings")
            {
                targetName = nameof(AppSettingsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "AppData")
            {
                targetName = nameof(AppDataPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "QueryResults")
            {
                targetName = nameof(QueryResultPage);
                QueryResultLocationsDto dto = QueryLocations;
                await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
                {
                    {"LocationsDto", dto }
                });
            }
            else
            {
                targetName = nameof(MainPage);
                await Shell.Current.GoToAsync(targetName);
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
