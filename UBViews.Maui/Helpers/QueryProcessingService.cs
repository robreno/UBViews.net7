namespace UBViews.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;

using System.Xml.Linq;
using System.Linq;
using Microsoft.FSharp.Core;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Models.Query;
using UBViews.Models.Ubml;
using UBViews.Helpers;
using UBViews.Views;

using QueryEngine;
using QueryFilter;
using static QueryEngine.SimpleParse;
using static DataTypesEx;

public partial class QueryProcessingService : IQueryProcessingService
{
    #region  Private Data Members
    public ContentPage contentPage;

    public ObservableCollection<QueryLocationDto> QueryLocationsDto { get; } = new();

    bool _queryParsingSuccessful = false;
    bool _queryLocationsDtoInitiaized = false;

    IFSRepositoryService repositoryService;
    IAppSettingsService settingsService;

    ParserService parserService;

    Regex rgxFilterBy = new Regex(@"(filterby)\s{1}\w{4,9}");
    Regex rgxAnd = new Regex(@"\sand\s");
    Regex rgxOr = new Regex(@"\sor\s");
    Regex rgxPhrase = new Regex(@"""(.*?)""");

    readonly string _class = "QueryProcessingService";
    #endregion

    #region  Constructors
    public QueryProcessingService(IFSRepositoryService repositoryService)
    {
        this.repositoryService = repositoryService;
        this.settingsService = ServiceHelper.Current.GetService<IAppSettingsService>();
        parserService = new ParserService();

        // TODO: Move this code out of Constructor
        Task.Run(async () =>
        {
            await InitializeQueryResults();
        });
    }
    #endregion

    #region  Initialize Query Results
    private async Task InitializeQueryResults()
    {
        string _method = "nitializeQueryResults";
        try
        {
            QueryResults = await repositoryService.GetQueryResultsAsync();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region  Public Properties
    public bool Initialized { get; set; }
    public QueryInputDto QueryInputDto { get; set; }
    public string QueryInputString { get; set; }
    public string PreviousQueryInputString { get; set; }
    public string QueryExpression { get; set; }
    public bool QueryResultExists { get; set; }
    public List<QueryResultDto> QueryResults { get; set; }
    public QueryResultLocationsDto QueryLocations { get; set; }
    public XElement QueryResult { get; set; }
    public List<string> TermList { get; set; }
    public int MaxQueryResults { get; set; }
    #endregion

    #region  Public Methods
    public async Task<bool> IsInitializedAsync()
    {
        string _method = "SetContentPageAsync";
        try
        {
            return Initialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task SetContentPageAsync(ContentPage contentPage)
    {
        try
        {
            this.contentPage = contentPage;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }
    public async Task<(bool, string)> PreCheckQueryAsync(string queryString, bool silent)
    {
        string _method = "PrecheckQueryAsync";
        try
        {
            bool isValidCommand = true;
            string message = queryString;
            queryString = queryString.Trim();
            if (queryString.Contains("="))
            {
                var arry = queryString.Split("=");
                var command = arry[0];
                var value = arry[1];
                if (command == "audio_streaming")
                {
                    await SetAudioStreamingAsync(value);
                    message = $"Audio streaming = {value.ToUpper()}.";
                    if (!silent)
                    {
                        await SendToast(message);
                    }

                }
                else if (command == "audio_status")
                {
                    await SetAudioStatusAsync(value);
                    message = $"Audio status = {value.ToUpper()}.";
                    if (!silent)
                    {
                        await SendToast(message);
                    }
                }
                else if (command == "audio_download_status")
                {
                    //audio__download_status := [on | off]
                    await SetAudioDownloadStatusAsync(value);
                    message = $"Audio download status = {value.ToUpper()}.";
                    if (!silent)
                    {
                        await SendToast(message);
                    }
                }
                else
                {
                    isValidCommand = false;
                    message = $"Invalid command = [{command}], please try again.";
                    if (!silent)
                    {
                        await SendToast(message);
                    }
                }
                await ClearQuerySearchBar(contentPage);
            }
            return (isValidCommand, message);
        }
        catch (NullReferenceException ex)
        {
            await App.Current.MainPage.DisplayAlert($"NullReference in {_class}.{_method} => ", ex.Message, "Ok");
            return (false, "NULLREFERENCE_COMMAND_FAILED");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return (false, "COMMAND_FAILED");
        }
    }
    public async Task SetAudioStreamingAsync(string value)
    {
        string _method = "SetAudioStreamingAsync";
        try
        {
            if (value == "on")
            {
                Preferences.Default.Set("stream_audio", true);
                await settingsService.Set("stream_audio", true);
            }
            if (value == "off")
            {
                Preferences.Default.Set("stream_audio", false);
                await settingsService.Set("stream_audio", false);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task SetAudioStatusAsync(string value)
    {
        string _method = "SetAudioStatusAsync";
        try
        {
            if (value == "on")
            {
                Preferences.Default.Set("audio_status", "on");
                await settingsService.Set("audio_status", "on");
            }
            if (value == "off")
            {
                Preferences.Default.Set("audio_status", "off");
                await settingsService.Set("audio_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task SetAudioDownloadStatusAsync(string value)
    {
        string _method = "SetAudioDownloadStatusAsync";
        try
        {
            if (value == "on")
            {
                Preferences.Default.Set("audio_download_status", "on");
                await settingsService.Set("audio_download_status", "on");
            }
            if (value == "off")
            {
                Preferences.Default.Set("audio_download_status", "off");
                await settingsService.Set("audio_download_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task SetMaxQueryResultsAsync(int max)
    {
        string _method = "SetMaxQueryResultsAsync";
        try
        {
            MaxQueryResults = max;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    public async Task<(bool, string)> ParseQueryAsync(string queryString)
    {
        string _method = "ParseQuery";
        var isSuccess = false;
        var errorMessage = string.Empty;
        var message = string.Empty;
        try
        {
            //var isSecretCommand = await PreCheckQuery(queryString);
            var validChars = QueryFilterService.checkForValidChars(queryString);
            var validCharsSuccess = validChars.Item1;
            var validForm = QueryFilterService.checkForValidForm(queryString);
            var validFormSuccess = validForm.Item1;

            if (queryString == null ||
                queryString == string.Empty ||
                !validCharsSuccess ||
                !validFormSuccess)
            {
                if (!validCharsSuccess || !validFormSuccess)
                {
                    if (!validCharsSuccess)
                        errorMessage = errorMessage + validChars.Item2 + ";";
                    if (!validFormSuccess)
                        errorMessage = errorMessage + validForm.Item2 + ";";

                    message = $"Bad query at {errorMessage}. Edit and click Ok or cancel query.";
                }

                if (string.IsNullOrEmpty(queryString))
                {
                    errorMessage = "Query empty, please enter a valid query.";
                }

                QueryInputDto.Text = "Empty Query";
                QueryInputDto.TokenCount = 0;
                PreviousQueryInputString = QueryInputString;

                message = errorMessage;
                isSuccess = false;
            }
            else
            {
                // Normalize Query and set QueryInputString property
                await NormalizeQueryString(queryString);
                // Parse queryString with QueryEngine methods
                QueryExpression = parserService.ParseQuery(QueryInputDto.Text).ToString();
                TermList = parserService.ParseQueryStringToTermList(queryString).ToList();
                // At this point have enough data to submit query and to get postinglist for each term
                message = "Query_Success";
                isSuccess = _queryParsingSuccessful = true;
            }
            return (isSuccess, message);
        }
        catch (NullReferenceException ex)
        {
            await App.Current.MainPage.DisplayAlert($"Null reference raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return (false, null);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return (false, null);
        }
    }
    public async Task<(bool, bool, QueryResultLocationsDto)> RunQueryAsync(string queryString)
    {
        string _method = "RunQuery";
        bool isSuccess = false;
        bool isFromQueryHistory = false;
        try
        {
            var errorMessage = string.Empty;
            var msg = string.Empty;
            if (!_queryParsingSuccessful)
            {
                return (false, false, null);
            }

            bool isSameQueryInput = QueryInputString == PreviousQueryInputString;
            if (isSameQueryInput)
            {
                // Same query and returned no results
                if (QueryLocations == null)
                {
                    // Query Returned Empty TokenPostingList
                    msg = $"The query \"{QueryInputString}\" returned no results. Please try another query.";
                    await App.Current.MainPage.DisplayAlert("Query Results", msg, "Cancel");
                    return (false, false, QueryLocations);
                }
                // Same query with results
                else
                {
                    return (true, true, QueryLocations);
                }
            }

            QueryLocations = null;

            (bool queryExists, QueryResultDto queryResultDto) = await QueryResultExistsAsync(queryString);
            QueryResultExists = isFromQueryHistory = queryExists;
            QueryResultLocationsDto queryResultLocationsDto = null;
            int queryResultId = 0;
            if (queryExists)
            {
                queryResultId = queryResultDto.Id;
                queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(queryResultId);

                if (queryString != queryResultLocationsDto.QueryString)
                {
                    queryResultLocationsDto.DefaultQueryString = queryString;
                }
                QueryExpression = queryResultLocationsDto.QueryExpression;
                QueryLocations = queryResultLocationsDto;
                isSuccess = true;
            }
            else
            {
                var astQuery = await parserService.ParseQueryAsync(QueryInputString);
                var query = astQuery.Head;
                QueryExpression = await parserService.QueryToStringAsync(query);

                var tpl = await repositoryService.RunQueryAsync(QueryInputString);

                bool isAtEnd = tpl.AtEnd;
                if (!isAtEnd)
                {
                    (QueryResultLocationsDto qrl, XElement qre) = await repositoryService.GetQueryResultLocationsAsync(QueryInputString, query, tpl);
                    var maxQryLocs = qrl.QueryLocations.Take(MaxQueryResults).ToList();
                    //qrl.QueryLocations = maxQryLocs;

                    QueryResult = qre;
                    QueryLocations = qrl;

                    //await NavigateTo("QueryResults");

                    //var queryRowId = await _repositoryService.SaveQueryResultAsync(qre);
                    //qre.SetAttributeValue("id", queryRowId);

                    // Create object model
                    //queryResultLocationsDto = await _repositoryService.GetQueryResultByIdAsync(queryRowId);

                    // Add queryResultEml to QueryHistory AppData file here
                    //await _appDataService.AddQueryResult(qre);

                    // Navigate to QueryResultPage here
                    //await NavigateTo("QueryResults");
                    isSuccess = true;
                }
                else
                {
                    // Query Returned Empty TokenPostingList
                    msg = $"The query \"{QueryInputString}\" returned no results. Please try another query.";
                    await App.Current.MainPage.DisplayAlert("Query Results", msg, "Cancel");
                }
            }
            PreviousQueryInputString = queryString;
            return (isSuccess, isFromQueryHistory, QueryLocations);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return (false, false, null);
        }
    }
    public async Task<(bool, QueryResultDto)> QueryResultExistsAsync(string queryString)
    {
        string _method = "QueryResultExistsAsync";
        try
        {
            bool _exists = false;
            QueryResultDto _dto = null;
            if (QueryResults != null)
            {
                var queryResultDto = QueryResults.Where(q => q.QueryString == queryString ||
                                                        q.ReverseQueryString == queryString).FirstOrDefault();
                if (queryResultDto != null)
                {
                    _exists = true;
                    _dto = queryResultDto;
                }
                else
                {
                    _exists = false;
                    _dto = null;
                }
            }
            else
            {
                (bool queryExists, QueryResultDto dto) = await repositoryService
                                                               .QueryResultExistsAsync(QueryInputString);
                if (queryExists)
                {
                    _exists = true;
                    _dto = dto;
                }
                else
                {
                    _exists = false;
                    _dto = null;
                }
            }
            return (_exists, _dto);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return (false, null);
        }
    }
    public async Task<string> GetQueryInputStringAsync()
    {
        string _method = "GetQueryInputString";
        try
        {
            return this.QueryInputString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<string> GetQueryExpressionAsync()
    {
        string _method = "GetQueryExpressionString";
        try
        {
            return this.QueryExpression;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<List<string>> GetTermListAsync()
    {
        string _method = "GetTermsList";
        try
        {
            return this.TermList;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<bool> GetQueryResultExistsAsync()
    {
        string _method = "QueryResultExists";
        try
        {
            return this.QueryResultExists;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task<QueryResultLocationsDto> GetQueryResultLocationsAsync()
    {
        string _method = "GetQueryResultLocations";
        try
        {
            return this.QueryLocations;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task<SimpleEnumeratorsEx.TokenPostingList> CreateTokenPostingListAsync(string trm, List<TokenOccurrenceDto> toks)
    {
        string _method = "CreateTokenPostingListAsync";
        try
        {
            QueryResultLocationsDto dto = new QueryResultLocationsDto();
            List<TokenPositionEx> tokenPositionLst = new List<TokenPositionEx>();
            TokenPositionEx tok;
            await Task.Run(() =>
            {
                foreach (var occ in toks)
                {

                    tok = makeTokenPositionEx(trm,
                                              occ.PostingId,
                                              occ.ParagaphId,
                                              occ.DocumentId,
                                              occ.SequenceId,
                                              occ.SectionId,
                                              occ.DocumentPosition,
                                              occ.TextPosition);
                    tokenPositionLst.Add(tok);
                }
            });
            var FS_list = ListModule.OfSeq(tokenPositionLst); // Microsoft.FSharp.Collections.ListModule
            var tpl = makeTokenPostingList(FS_list);
            return tpl;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion

    #region Helper Methods
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    private async Task<string> ReverseQueryString(string queryString)
    {
        string _method = "ReverseQueryString";
        {
            try
            {
                string reverseQueryString = string.Empty;
                var queryType = await GetQueryType(queryString);
                reverseQueryString = queryType.ReverseQuery;
                return reverseQueryString;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return null;
            }
        }
    }
    private async Task<string> BaseQueryString(string queryString)
    {
        string _method = "BaseQueryString";
        try
        {
            var baseQuery = string.Empty;
            var queryType = await GetQueryType(queryString);
            baseQuery = queryType.BaseQuery;
            return baseQuery;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<QueryType> GetQueryType(string queryString)
    {
        string _method = "GetQueryType";
        try
        {
            var queryType = new QueryType();

            var baseQuery = string.Empty;
            var reverseQuery = string.Empty;
            var filterByOp = string.Empty;
            var filterByPostOp = string.Empty;
            var phraseOp = string.Empty;
            var len = queryString.Length;

            var _and = rgxAnd.Match(queryString);
            var _andSuccess = _and.Success;

            var _or = rgxOr.Match(queryString);
            var _orSuccess = _or.Success;

            var _filterby = rgxFilterBy.Match(queryString);
            var _filterbySuccess = _filterby.Success;
   
            queryType.QueryString = queryString;
            queryType.Length = queryString.Length;
            string[] terms;
            if (_filterbySuccess)
            {
                if (_andSuccess)
                {
                    queryType.Type = "FilterBy+And";
                    queryType.FilterByOp = filterByOp = _filterby.Value;
                    queryType.FilterByPostfixOp = filterByPostOp = filterByOp.Replace("filterby", "").Trim();
                    queryType.BaseQuery = baseQuery = queryString.Replace(filterByOp, "").Trim();
                    terms = baseQuery.Split(" and ");
                    queryType.ReverseQuery = reverseQuery = terms[1] + " and " + terms[0] + " " + filterByOp;

                }
                else if (_orSuccess)
                {
                    queryType.Type = "FilterBy+Or";
                    queryType.FilterByOp = filterByOp = _filterby.Value;
                    queryType.FilterByPostfixOp = filterByPostOp = filterByOp.Replace("filterby", "").Trim();
                    queryType.BaseQuery = baseQuery = queryString.Replace(filterByOp, "").Trim();
                    terms = baseQuery.Split(" or ");
                    queryType.ReverseQuery = reverseQuery = terms[1] + " or " + terms[0] + " " + filterByOp;
                }
            }
            else if (_andSuccess)
            {
                queryType.Type = "And";
                queryType.BaseQuery = baseQuery = queryString;
                terms = baseQuery.Split(_and.Value);
                queryType.ReverseQuery = reverseQuery = terms[1] + _and.Value + terms[0];
            }
            else if (_orSuccess)
            {
                queryType.Type = "Or";
                queryType.BaseQuery = baseQuery = queryString;
                terms = baseQuery.Split(_or.Value);
                queryType.ReverseQuery = reverseQuery = terms[1] + _or.Value + terms[0];
            }

            return queryType;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<bool> ClearQuerySearchBar(ContentPage contentPage)
    {
        string _method = "ClearQuerySearchBar";
        try
        {
            if (contentPage != null)
            {
                var searchBar = contentPage.FindByName("searchBarControl") as SearchBar;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    searchBar.Text = null;
                });
            }
            else
            {
                throw new NullReferenceException("ContentPage parameter null.");
            }
            return true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    private async Task SendToast(string message)
    {
        try
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(message, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            return;
        }
    }
    private async Task<List<QueryLocationDto>> InitializeQueryLocationsDto(QueryResultLocationsDto dto)
    {
        string _method = "InitializeQueryLocationsDto";
        try
        {
            List<QueryLocationDto> locations = new();

            var qlr = dto.QueryLocations.Take(MaxQueryResults).ToList();
            foreach (var location in qlr)
            {
                QueryLocationsDto.Add(location);
                locations.Add(location);
            }

            return locations;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
}
