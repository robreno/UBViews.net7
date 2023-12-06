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
using static DataTypesEx;

[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class MainViewModel : BaseViewModel
{
    public ContentPage contentPage;

    // TODO: Add in later when AudioService is added
    //ConnectivityViewModel connectivityViewModel;
    //IConnectivity connectivityService;

    IFileService fileService;
    IAppSettingsService appSettingsService;
    IFSRepositoryService repositoryService;
    IQueryProcessingService queryProcessingService;

    ParserService parserService;

    readonly string _class = "MainViewModel";

    public MainViewModel(IFileService fileService, IAppSettingsService appSettingsService, IFSRepositoryService repositoryService, IQueryProcessingService queryProcessingService)
    {
        this.fileService = fileService;
        this.appSettingsService = appSettingsService;

        this.repositoryService = repositoryService;
        this.parserService = new ParserService();
        this.queryProcessingService = queryProcessingService;
    }

    [ObservableProperty]
    bool isInitialized;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

    [ObservableProperty]
    QueryInputDto queryInputDto = new QueryInputDto() { Text = string.Empty, TokenCount = 0 };

    [ObservableProperty]
    int tokenCount;

    [ObservableProperty]
    string queryInput;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string previousQueryInputString;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    XElement queryResult;

    [ObservableProperty]
    bool queryResultExists;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    string partId;

    [ObservableProperty]
    List<string> termList;

    [ObservableProperty]
    string stem;

    [RelayCommand]
    async Task MainPageAppearing()
    {
        string _method = "MainPageAppearing";
        try
        {
            if (contentPage == null)
            {
                throw new NullReferenceException("ContentPage null.");
            }

            if (!IsInitialized)
            {
                await queryProcessingService.SetContentPageAsync(contentPage);
                await queryProcessingService.SetAudioStreamingAsync("on");

                MaxQueryResults = await appSettingsService.Get("max_query_results", 50);
                await queryProcessingService.SetMaxQueryResultsAsync(MaxQueryResults);

                isInitialized = true;
            }

            string titleMessage = $"UBViews Home";
            Title = titleMessage;
        }
        catch (NullReferenceException ex)
        {
            await App.Current.MainPage.DisplayAlert($"Null reference raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageLoaded()
    {
        string _method = "MainPageLoaded";
        try
        {
            MaxQueryResults = await appSettingsService.Get("max_query_results", 50);  
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task SubmitQuery(string queryString)
    {
        // temple and prostitutes
        // prostitutes and temple
        // foreword and orvonton

        string _method = "SubmitQuery";
        try
        {
            IsBusy = true;
            string message = string.Empty;
            bool parsingSuccessful = false;

            bool isCommand = await queryProcessingService.PreCheckQueryAsync(queryString);
            if (isCommand)
            {
                return;
            }

            QueryInputString = queryString.Trim();
            (parsingSuccessful, message) = await queryProcessingService.ParseQueryAsync(QueryInputString);
            if (parsingSuccessful)
            {
                QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                TermList = await queryProcessingService.GetTermListAsync();

                (bool isSuccess, QueryResultExists, QueryLocations) = await queryProcessingService.RunQueryAsync(QueryInputString);
                if (isSuccess)
                {
                    if (QueryResultExists)
                    {
                        // Query result from history successfully
                        // Navigate to results page
                    }
                    else
                    {
                        // New query run successfully
                        // Navigate to results page
                    }
                    await NavigateTo("QueryResults");
                }
                else // Parsing failure
                {
                    throw new Exception("Uknown Parsing Error!");
                }
            }
            else // Parsing failure
            {
                throw new Exception("Unknown Parsing Error!");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
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
            else if (target == "AppContacts")
            {
                targetName = nameof(ContactsPage);
                await Shell.Current.GoToAsync(targetName);
            }
            else if (target == "QueryResults")
            {
                targetName = nameof(QueryResultPage);
                QueryResultLocationsDto dto = QueryLocations;
                await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
                {
                    {"QueryLocations", dto }
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    #region  Helper Methods
    #endregion
}
