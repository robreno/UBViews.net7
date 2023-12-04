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
    bool isRefreshing;

    [ObservableProperty]
    bool isInitialized;

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
            if (contentPage != null && !IsInitialized)
            {
                await queryProcessingService.SetContentPageAsync(contentPage);
                await queryProcessingService.SetAudioStreamingAsync("off", false);
                isInitialized = true;
            }

            MaxQueryResults = await appSettingsService.Get("max_query_results", 50);
            await queryProcessingService.SetMaxQueryResults(MaxQueryResults);

            string titleMessage = $"UBViews Home";
            Title = titleMessage;
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
    async Task ParseQuery_Depreciated(string queryString)
    {
        string _method = "ParseQuery";
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

                QueryInputDto.Text = "Empty Query";
                QueryInputDto.TokenCount = 0;
                QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query empty",
                    "Bad query, enter a valid query");
            }
            else
            {
                await NormalizeQueryString(queryString);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task SubmitQuery_Depreciated(string queryString)
    {
        string _method = "SubmitQuery";
        try
        {
            IsBusy = true;

            queryString = queryString.Trim();

            if (queryString.Contains("^"))
            {
                var value = queryString.Substring(1, queryString.Length-1);
                await SetAudioStreaming(value);
                return;
            }

            var isNullOrEmpty = string.IsNullOrEmpty(queryString);
            if (!isNullOrEmpty)
            {
                var isMinusHyphenated = queryString.Contains("-");
                if (isMinusHyphenated)
                {
                    queryString = queryString.Replace("-", "‑");
                }
            }
            
            var validChars = QueryFilterService.checkForValidChars(queryString);
            var validCharsSuccess = validChars.Item1;
            var validForm = QueryFilterService.checkForValidForm(queryString);
            var validFormSuccess = validForm.Item1;

            var errorMessage = string.Empty;
            var msg = string.Empty;
            if (isNullOrEmpty || !validCharsSuccess || !validFormSuccess)
            {
                if (isNullOrEmpty)
                {
                    errorMessage = "[Empty Query String]";
                }
                else if (!validCharsSuccess || !validFormSuccess)
                {
                    if (!validCharsSuccess)
                        errorMessage = errorMessage + "at " + validChars.Item2 + "; ";
                    if (!validFormSuccess)
                        errorMessage = errorMessage + "at " + validForm.Item2 + "; ";
                }
                else
                {
                    errorMessage = "Unknown Query Error";
                }

                msg = $"Bad query {errorMessage}. \r\nEdit and click Ok or cancel query.";

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
            }
            else
            {
                await NormalizeQueryString(queryString);

                // Check if query exists
                (bool queryExists, QueryResultDto dto) = await repositoryService.QueryResultExistsAsync(QueryInputString);
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
                    // premind and capacity filterby parid

                    var astQuery = await parserService.ParseQueryAsync(QueryInputString);
                    var query = astQuery.Head;
                    QueryExpression = await parserService.QueryToStringAsync(query);

                    // ~caucasoid
                    // ~rejuvenated
                    // crime or punishment
                    // "name of your world"
                    // "be of good cheer"

                    // Bug here: SimpleParse.getIteratorEx return types wrong
                    // never-ending  and "infinite perfection"  => "And+Phrase"
                    // "infinite perfection" and never-ending   => "And+Phrase"
                    // [energy transmitter]

                    var tpl = await repositoryService.RunQueryAsync(QueryInputString);

                    bool isAtEnd = tpl.AtEnd;
                    if (!isAtEnd)
                    {
                        (QueryResultLocationsDto qrl, XElement qre) = await repositoryService.GetQueryResultLocationsAsync(QueryInputString, query, tpl);
                        var maxQryLocs = qrl.QueryLocations.Take(MaxQueryResults).ToList();
                        //qrl.QueryLocations = maxQryLocs;

                        QueryLocations = qrl;
                        await NavigateTo("QueryResults");

                        //var queryRowId = await _repositoryService.SaveQueryResultAsync(queryResultElm);
                        //queryResultElm.SetAttributeValue("id", queryRowId);

                        // Create object model
                        //queryResultLocationsDto = await _repositoryService.GetQueryResultByIdAsync(queryRowId);

                        // Add queryResultEml to QueryHistory AppData file here
                        //await _appDataService.AddQueryResult(queryResultElm);

                        // Navigate to QueryResultPage here
                        //await NavigateTo("QueryResults");
                    }
                    else
                    {
                        msg = $"The query \"{QueryInputString}\" returned no results. Try another query.";
                        await App.Current.MainPage.DisplayAlert("SumbitQuery => ", msg, "Cancel");
                        await Shell.Current.GoToAsync("..");
                    }
                }
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
    async Task SubmitQuery(string queryString)
    {
        // temple and prostitutes
        // prostitutes and temple
        // foreword and orvonton

        string _method = "SubmitQueryEx";
        try
        {
            IsBusy = true;
            string msg = string.Empty;

            //bool isSecretCommand = await queryProcessingService.PreCheckQueryAsync(queryString);
            //if (isSecretCommand)
            //{
            //    return;
            //}

            var parsingSuccessful = await queryProcessingService.ParseQueryAsync(queryString);
            if (parsingSuccessful)
            {
                QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                TermList = await queryProcessingService.GetTermListAsync();

                (bool isSuccess, 
                 QueryResultExists, 
                 QueryLocations) = await queryProcessingService.RunQueryExAsync(queryString);

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
    private async Task NormalizeQueryString(string queryString)
    {
        string _method = "NormalizeQueryString";
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
            QueryInputDto = new QueryInputDto() { Text = qs, TokenCount = tokens.Length };
            QueryInputString = QueryInputDto.Text;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }
    private async Task SetAudioStreaming(string value)
    {
        string _method = "SetAudioStreaming";
        try
        {
            if (value == "on")
            {
                Preferences.Default.Set("stream_audio", true);
            }
            if (value == "off")
            {
                Preferences.Default.Set("stream_audio", false);
            }

            var searchBar = contentPage.FindByName("searchBarControl") as SearchBar;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                searchBar.Text = null;
            });
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    #endregion
}
