namespace UBViews.ViewModels;

using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using Microsoft.Maui.Controls.Compatibility;

using UBViews.Models;
using UBViews.Services;
using UBViews.Models.Query;
using UBViews.Models.Ubml;
using UBViews.Views;

using QueryEngine;
using QueryFilter;

[QueryProperty(nameof(TokenCount), nameof(TokenCount))]
[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class QueryInputViewModel : BaseViewModel
{
    private QueryInputDto _queryInput;
    public ContentPage contentPage;

    IAppDataService appDataService;
    IFileService fileService;
    IFSRepositoryService repositoryService;

    public ObservableCollection<QueryCommandDto> QueryCommands { get; } = new();

    ParserService parserService;
    QueryService queryService;

    public QueryInputViewModel(IAppDataService appDataService, IFileService fileService, IFSRepositoryService repositoryService)
    {
        this.appDataService = appDataService;
        this.fileService = fileService;
        this.repositoryService = repositoryService;
        this.parserService = new ParserService();
        this.queryService = new QueryService();
    }

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto locationsDto;

    [ObservableProperty]
    QueryInputDto queryInput;

    [ObservableProperty]
    int tokenCount;

    [ObservableProperty]
    XElement queryResult;

    [ObservableProperty]
    string queryInputString;

    [ObservableProperty]
    string queryExpression;

    [ObservableProperty]
    bool queryResultExists;

    [ObservableProperty]
    string queryResultString;

    [RelayCommand]
    async Task QueryInputPageAppearing()
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;
            await repositoryService.InititializeDatabase(IFSRepositoryService.InitOptions.All);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.QueryInputPageAppearing => ",
               ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            //IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task QueryInputPageLoaded()
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;

            //var commands = await appDataService.GetQueryCommandsAsync();
            //var repoCommands = await repositoryService.GetQueryCommandsAsync();
            //foreach (var cmd in repoCommands)
            //{
            //    var newCommand = new QueryCommandDto 
            //    {
            //        Id = cmd.Id,
            //        Hits = cmd.Hits,
            //        Type = cmd.Type,
            //        Terms = cmd.Terms,
            //        Proximity = cmd.Proximity,
            //        QueryString = cmd.QueryString,
            //        ReverseQueryString = cmd.ReverseQueryString,
            //        QueryExpression = cmd.QueryExpression
            //    };
            //    QueryCommands.Add(newCommand);
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.QueryInputPageLoaded => ",
               ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
            //IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task ParseQuery(string queryString)
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;

            var validChars = QueryFilterService.checkForValidChars(queryString);
            var validCharsSuccess = validChars.Item1;
            var validForm = QueryFilterService.checkForValidForm(queryString);
            var validFormSuccess = validForm.Item1;

            if (queryString == "EmptyQuery" || 
                queryString == null || 
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

                var result = await App.Current.MainPage.DisplayPromptAsync("Query Error", msg, "OK", "Cancel", null, -1, null, queryString);
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
                var queryText = QueryInput.Text;
                var result = parserService.ParseQuery(queryText);
                QueryExpression = result.ToString();
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.ParseQuery => ", 
                ex.Message, "Cancel");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task RunQuery(string queryString)
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;

            var validChars = QueryFilterService.checkForValidChars(queryString);
            var validCharsSuccess = validChars.Item1;
            var validForm = QueryFilterService.checkForValidForm(queryString);
            var validFormSuccess = validForm.Item1;

            if (queryString == "EmptyQuery" || 
                queryString == null || 
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
            }
            else
            {
                await NormalizeQueryString(queryString);
                var _queryInputDto = new QueryInputDto { Text = QueryInput.Text, TokenCount = QueryInput.TokenCount };

                QueryInput = _queryInputDto;
                QueryInputString = QueryInput.Text;

                // Check if query exists
                (bool queryExists, QueryResultDto dto) = await repositoryService.QueryResultExistsAsync(queryString);
                var queryId = dto.Id;
                QueryResultExists = queryExists;
                QueryResultLocationsDto queryResultLocationsDto = null;
                if (queryExists)
                {
                    queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(queryId);
                    QueryExpression = queryResultLocationsDto.QueryExpression;
                }
                else
                {
                    // Run Query
                    // 
                    var queryText = QueryInput.Text;
                    var queryList = await parserService.ParseQueryAsync(queryText);
                    var queryHead = queryList.Head;
                    QueryExpression = await parserService.QueryToStringAsync(queryHead);

                    var tokenPostingList = await queryService.RunQueryAsync(queryHead);
                    var basePostingList = tokenPostingList.BasePostingList.Head;
                    var queryResultElm = await queryService.ProcessTokenPostingListAsync(queryString, 
                                                                                         queryHead, 
                                                                                         basePostingList);
                    //var qryId = await repositoryService.SaveQueryResultAsync(queryResultElm);
                    //queryResultElm.SetAttributeValue("id", qryId);

                    // Create object model
                    //queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(qryId);

                    // Add queryResultEml to QueryHistory AppData file here
                    //await _appDataService.AddQueryResult(queryResultElm);
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.RunQuery => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    async Task TappedGesture(string lableName)
    {
        try
        {
            var arry = lableName.Split('_', StringSplitOptions.RemoveEmptyEntries);
            var paperId = Int32.Parse(arry[0]);
            var seqId = Int32.Parse(arry[1]);
            Paragraph paragraph = await fileService.GetParagraphAsync(paperId, seqId);
            PaperDto paperDto = await fileService.GetPaperDtoAsync(paperId);
            var uid = paragraph.Uid;
            var pid = paragraph.Pid;
            paperDto.ScrollTo = true;
            paperDto.SeqId = seqId;
            paperDto.Pid = pid;
            paperDto.Uid = uid;
            await GoToDetails(paperDto);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    [RelayCommand]
    async Task GoToDetails(PaperDto dto)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            if (dto == null)
                return;

            string className = "_" + dto.Id.ToString("000");

            await Shell.Current.GoToAsync(className, true, new Dictionary<string, object>
            {
                {"PaperDto", dto }
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    async Task LookupStem(string queryString)
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;

            if (queryString == "EmptyQuery" || queryString == null)
            {
                await App.Current.MainPage.DisplayPromptAsync("Query Status", "Bad query, enter a valid query");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                //QueryExpression = await parserService.ParseQuery(QueryInput.Text);
                //QueryExpression = parserService.ParseQuery(QueryInput.Text).ToString();
                throw new NotImplementedException("LookupStem is not implemented");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LookupStem => ",
                ex.Message, "Ok");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Helper Methods
    private async Task LoadXaml(QueryResultLocationsDto queryResultLocationsDto)
    {
        try
        {
            // See: https://learn.microsoft.com/en-us/dotnet/maui/xaml/runtime-load
            /*
            string navigationButtonXAML = "<Button Text=\"Navigate\" />";
            Button navigationButton = new Button().LoadFromXaml(navigationButtonXAML);
            stackLayout.Add(navigationButton); ...
            */

            var contentScrollView = contentPage.FindByName("queryResultScrollView") as ScrollView;
            var contentVSL = contentPage.FindByName("queryResultVSL") as VerticalStackLayout;

            var locations = queryResultLocationsDto.QueryLocations;
            foreach (var location in locations)
            {
                var id = location.Id;
                var pid = location.Pid;
                var arry = id.Split(':');
                var paperId = Int32.Parse(arry[0]);
                var seqId = Int32.Parse(arry[1]);
                var paragraph = await fileService.GetParagraphAsync(paperId, seqId);
                var text = paragraph.Text;
                var paraStyle = paragraph.ParaStyle;

                // Study the .NET Maui ScrollView examples

                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
                FormattedString fs = await CreateFormattedString(paragraph);

                Label label = new Label { FormattedText = fs };
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
                tapGestureRecognizer.CommandParameter = $"{labelName}";
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                label.GestureRecognizers.Add(tapGestureRecognizer);

                label.SetValue(ToolTipProperties.TextProperty, "Tap to go to paragraph.");

                Border newBorder = new Border()
                {
                    Stroke = Colors.Blue,
                    Padding = new Thickness(10),
                    Margin = new Thickness(.5),
                    Content = label
                };
                contentVSL.Add(newBorder);

                foreach (var termOcc in location.TermOccurrences)
                {
                    var position = termOcc.TextPosition;
                    var length = termOcc.TextLength;
                    var term = text.Substring(position, length);
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
        }
    }
    private async Task<FormattedString> CreateFormattedString(Paragraph paragraph)
    {
        // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
        try
        {
            FormattedString formattedString = new FormattedString();
            await Task.Run(() =>
            {
                var paperId = paragraph.Id;
                var seqId = paragraph.SeqId;
                var pid = paragraph.Pid;
                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                Span tabSpan = new Span() { Style = (Style)App.Current.Resources["TabsSpan"] };
                Span pidSpan = new Span() { Style = (Style)App.Current.Resources["PID"], StyleId = labelName, Text = pid };
                Span spaceSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpaceSpan"] };

                var runs = paragraph.Runs;
                Span newSpan = null;

                formattedString.Spans.Add(tabSpan);
                formattedString.Spans.Add(pidSpan);
                formattedString.Spans.Add(spaceSpan);
                foreach (var run in runs)
                {
                    newSpan = new Span() { Style = (Style)App.Current.Resources["RegularSpan"], Text = run.Text };
                    formattedString.Spans.Add(newSpan);
                }
            });
            return formattedString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.LoadXaml => ",
                ex.Message, "Ok");
            return null;
        }
    }
    private async Task NormalizeQueryString(string queryString)
    {
        try
        {
            await Task.Run(() => 
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
                QueryInput = _queryInput;
                QueryInputString = _queryInput.Text;
                TokenCount = _queryInput.TokenCount;
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.NormalizeQueryString => ",
                ex.Message, "Cancel");
        }
    }
    private async Task<string> GetQueryType(string queryString)
    {
        Regex rgxFilterBy = new Regex("filterby");
        Regex rgxAnd = new Regex(@"\sand\s");
        Regex rgxOr = new Regex(@"\sor\s");
        var queryType = string.Empty;
        try
        {
            var filteryByOp = rgxFilterBy.Match(queryString).Success;
            var andOp = rgxAnd.Match(queryString).Success;
            var orOp = rgxOr.Match(queryString).Success;
            if (andOp && filteryByOp)
            {
                queryType = "FilterBy+And";
            }
            else if (orOp && filteryByOp)
            {
                queryType = "FilterBy+Or";
            }
            else if (andOp && !filteryByOp)
            {
                queryType = "And";
            }
            else if (orOp && !filteryByOp)
            {
                queryType = "Or";
            }
            return queryType;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.GetQueryType => ",
                ex.Message, "Cancel");
            return string.Empty;
        }
    }
    private async Task<string> ReverseQueryString(string queryString, string queryType)
    {
        Regex rgxFilterBy = new Regex("filterby");
        Regex rgxAnd = new Regex(@"\sand\s");
        Regex rgxOr = new Regex(@"\sor\s");
        var reverseQueryString = string.Empty;
        var filterByOp = string.Empty;
        var baseQuery = string.Empty;
        var len = queryString.Length;
        try
        {
            if (queryType == "And")
            {
                var m = rgxAnd.Match(queryString);
                var terms = queryString.Split(" and ");
                reverseQueryString = terms[1] + " and " + terms[0];
            }
            else if (queryType == "FilterBy+And")
            {
                var m = rgxFilterBy.Match(queryString);
                if (m.Success)
                {
                    filterByOp = queryString.Substring(m.Index, queryString.Length - m.Index);
                    baseQuery = queryString.Substring(0, m.Index - 1);
                    var terms = baseQuery.Split(" and ");
                    reverseQueryString = terms[1] + " and " + terms[0] + " " + filterByOp;
                }
            }
            return reverseQueryString;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.ReverseQueryString => ",
                ex.Message, "Cancel");
            return string.Empty;
        }
    }
    private async Task TestMethods()
    {
        try
        {
            // TODO: Add to Repository .... or change to Repository?
            //var queryResultLocationsDto = await repositoryService.GetQueryResultByIdAsync(queryId);
            //var queryResultByQueryString = await repositoryService.GetQueryResultByQueryStringAsync(queryString);

            //var testQR = await repositoryService.GetTermOccurrencesByQueryResultIdAsync(queryId);

            //var termOccurrenceLst = await repositoryService.GetTermOccurrencesByQueryResultIdAsync(queryId);

            //var postingLst1 = await repositoryService.GetPostingByLexemeAsync("rejuvenation");
            //var stem = await repositoryService.GetTokenStemAsync("rejuvenation");

            //// 1. Get PostingList for each term in query use F# to parse QueryExpression and get posting lists
            //var postingLst2 = await repositoryService.GetPostingByLexemeAsync("foreword");
            //var postingLst3 = await repositoryService.GetPostingByLexemeAsync("orvonton");

            //// 2. Get TokenOccurrenceList for each PostingListId
            //var tokenOccurrences1 = await repositoryService.GetTokenOccurrencesByPostingListIdAsync(postingLst2.Id);
            //var tokenOccurrences2 = await repositoryService.GetTokenOccurrencesByPostingListIdAsync(postingLst3.Id);

            // TODO: Navitage to QueryResultPage here for test puroses

            //await LoadXaml(queryResultLocationsDto);
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised in QueryInputViewModel.TestMethods => ",
                ex.Message, "Cancel");
            return;
        }
    }
    #endregion
    Task Back() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    async Task NavigateTo(string target)
    {
        if (IsBusy)
            return;

        try
        {
            LocationsDto = new QueryResultLocationsDto();

            IsBusy = true;

            string targetName = string.Empty;
            if (target == "QueryResultPage")
            {
                targetName = nameof(QueryResultPage);
            }
            else
            {
                targetName = nameof(QueryInputPage);
            }


            await Shell.Current.GoToAsync(targetName, new Dictionary<string, object>()
            {
                {"QueryResultLocatinsDto", LocationsDto }
            });
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
}
