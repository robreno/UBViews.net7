namespace UBViews.Helpers;

using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;

using System.Xml.Linq;
using System.Linq;
using Microsoft.FSharp.Core;

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
        parserService = new ParserService();
        Task.Run(async () =>
        {
            await InitializeQueryResults();
        });
    }
    #endregion

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

    #region  Public Properties
    public QueryInputDto QueryInputDto { get; set; }
    public string QueryInputString { get; set; }
    public string QueryExpression { get; set; }
    public bool QueryResultExists { get; set; }
    public List<QueryResultDto> QueryResults { get; set; }
    public QueryResultLocationsDto QueryLocations { get; set; }
    public XElement QueryResult { get; set; }
    public List<string> TermList { get; set; }
    public int MaxQueryResults { get; set; }
    #endregion

    #region  Public Methods
    public async Task<bool> ParseQueryAsync(string queryString)
    {
        string _method = "ParseQuery";
        var isSuccess = false;
        try
        {
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
                QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query empty", "Bad query, enter a valid query");
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
                isSuccess = _queryParsingSuccessful = true;
            }
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task<bool> RunQueryAsync(string queryString)
    {
        string _method = "RunQuery";
        bool isSuccess = false;
        try
        {
            var errorMessage = string.Empty;
            var msg = string.Empty;
            if (!_queryParsingSuccessful)
            {
                return false;
            }

            (bool queryExists, QueryResultDto queryResultDto) = await QueryResultExistsAsync(queryString);
            QueryResultExists = queryExists;
            QueryResultLocationsDto queryResultLocationsDto = null;
            int queryResultId = 0;
            if (queryExists)
            {
                queryResultId = queryResultDto.Id;
                queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(queryResultId);

                var queryType = await GetQueryType(queryString);
                queryResultLocationsDto.BaseQuery = queryType.BaseQuery;

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
                    msg = $"The query \"{QueryInputString}\" returned no results. Try another query.";
                    await App.Current.MainPage.DisplayAlert("Query Results => ", msg, "Cancel");
                    await Shell.Current.GoToAsync("..");
                }
            }
            var lst = await InitializeQueryLocationsDto(QueryLocations);
            return isSuccess;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
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
    public async Task SetMaxQueryResults(int max)
    {
        string _method = "GetQueryInputString";
        try
        {
            MaxQueryResults = max;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
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
    #endregion

    #region Helper Methods
    /// <summary>
    /// NormalizeQueryString
    /// </summary>
    /// <param name="queryString"></param>
    /// <returns></returns>
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
