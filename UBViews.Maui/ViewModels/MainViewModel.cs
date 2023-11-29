﻿namespace UBViews.ViewModels;

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
    public ContentPage contentPage;

    QueryInputDto _queryInput = new QueryInputDto() { Text = string.Empty, TokenCount = 0 };

    // TODO: Add in later when AudioService is added
    //ConnectivityViewModel connectivityViewModel;
    //IConnectivity connectivityService;

    IFileService fileService;
    IAppSettingsService appSettingsService;
    IFSRepositoryService repositoryService;
    IQueryProcessingService queryProcessingService;

    ParserService parserService;
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
    int tokenCount;

    [ObservableProperty]
    string queryInput;

    [ObservableProperty]
    string queryInputString;

    //[ObservableProperty]
    //string searchInput;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    XElement queryResult;

    [ObservableProperty]
    bool queryResultExists;

    //[ObservableProperty]
    //int queryHits;

    [ObservableProperty]
    int maxQueryResults;

    [ObservableProperty]
    QueryInputDto queryInputObj;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

    [ObservableProperty]
    string partId;

    [ObservableProperty]
    List<string> termList;

    [ObservableProperty]
    string stem;

    [RelayCommand]
    async Task MainPageAppearing()
    {
        try
        {
            string titleMessage = $"UBViews Home";
            Title = titleMessage;
            //MaxQueryResults = await appSettingsService.Get("max_query_results", 50);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainPageAppearing => ",
                ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageLoaded()
    {
        try
        {
            MaxQueryResults = await appSettingsService.Get("max_query_results", 50);  
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainPageLoaded => ",
                ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task SubmitQuery(string queryString)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.SumbitQuery => ", 
                ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task SubmitQueryEx(string queryString)
    {
        try
        {
            IsBusy = true;
            string msg = string.Empty;

            var parsingSuccessful = await queryProcessingService.ParseQueryAsync(queryString);
            if (parsingSuccessful)
            {
                QueryInputString = await queryProcessingService.GetQueryInputStringAsync();
                QueryExpression = await queryProcessingService.GetQueryExpressionAsync();
                TermList = await queryProcessingService.GetTermListAsync();

                bool result = await queryProcessingService.RunQueryAsync(QueryInputString);
                if (result == true)
                {
                    QueryResultExists = await queryProcessingService.GetQueryResultExistsAsync();
                    QueryLocations = await queryProcessingService.GetQueryResultLocationsAsync();
                }
                await NavigateTo("QueryResults");
            }
            else // Parsing failure
            {
                throw new Exception("Uknown Parsing Error!");
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
            IsRefreshing = false;
        }
    }

    //[RelayCommand]
    //async Task CheckInternet()
    //{
    //    if (IsBusy)
    //        return;

    //    try
    //    {
    //        IsBusy = true;

    //        var hasInternet = await connectivityViewModel.CheckInternet();
    //        await App.Current.MainPage.DisplayAlert("Has Internet", hasInternet ? "YES!" : "NO!", "OK");
    //    }
    //    catch (Exception ex)
    //    {
    //        await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.CheckInternet => ",
    //            ex.Message, "Cancel");
    //    }
    //    finally
    //    {
    //        IsBusy = false;
    //    }
    //}

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
            IsRefreshing = false;
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
            TokenCount = _queryInput.TokenCount;
            QueryInputObj = _queryInput;
            QueryInputString = _queryInput.Text;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NormalizeQueryString => ",
                ex.Message, "Cancel");
        }
    }

    private async Task SetAudioStreaming(string value)
    {
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
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.SetAudioStreaming => ",
                ex.Message, "Cancel");
            return;
        }
    }
}
