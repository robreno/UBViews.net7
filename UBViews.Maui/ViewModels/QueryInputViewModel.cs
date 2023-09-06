using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Compatibility;

using UBViews.Services;
using UBViews.Models;
using UBViews.Models.Query;
using UBViews.Models.Ubml;

using UBViews.SQLiteRepository;
using UBViews.SQLiteRepository.Dtos;
using UBViews.SQLiteRepository.Models;

using UBViews.LexParser;
//using QueryFilterLib;

namespace UBViews.ViewModels;

[QueryProperty(nameof(TokenCount), nameof(TokenCount))]
[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class QueryInputViewModel : BaseViewModel
{
    private QueryInputDto _queryInput;
    public ContentPage contentPage;

    IAppDataService appDataService;

    IFileService fileService;
    IRepositoryService repositoryService;

    public ObservableCollection<QueryCommandDto> QueryCommands { get; } = new();

    ParserService parserService;

    public QueryInputViewModel(IAppDataService appDataService, IFileService fileService, IRepositoryService repositoryService)
    {
        this.appDataService = appDataService;
        this.fileService = fileService;
        this.parserService = new ParserService();
        this.repositoryService = repositoryService;
    }

    [ObservableProperty]
    bool isRefreshing;

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
            await repositoryService.InititializeDatabase(InitOptions.All);
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

            var commands = await appDataService.GetQueryCommandsAsync();
            var repoCommands = await repositoryService.GetQueryCommandsAsync();
            foreach (var cmd in repoCommands)
            {
                var newCommand = new QueryCommandDto 
                {
                    Id = cmd.Id,
                    Type = cmd.Type,
                    Terms = cmd.Terms,
                    Proximity = cmd.Proximity,
                    Stemmed = cmd.Stemmed,
                    FilterId = cmd.FilterId,
                    QueryString = cmd.QueryString
                };
                QueryCommands.Add(newCommand);
            }
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

            if (queryString == "EmptyQuery" || queryString == null)
            {
                await App.Current.MainPage.DisplayPromptAsync("Query Status", "Bad query, enter a valid query");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                var queryText = QueryInput.Text;
                var result = parserService.ParseQuery(queryText);
                QueryExpression = result.ToString();
                var terms = parserService.ParseQueryStringToTermList(queryText);
                foreach (var term in terms)
                {
                    var postingList = await repositoryService.GetPostingByLexemeAsync(term);
                    var tokOccs = await repositoryService.GetTokenOccurrencesByPostingListIdAsync(postingList.Id);
                    var newPostingListOccurrences = new PostingListOccurrencesDto
                    {
                        Id = postingList.Id,
                        Lexeme = postingList.Lexeme
                    };
                    foreach (var occ in tokOccs)
                    {
                        var occDto = new TokenOccurrenceDto
                        { 
                            PostingId = occ.PostingId,
                            DocumentId = occ.DocumentId,
                            SequenceId = occ.SequenceId,
                            DocumentPosition = occ.DocumentPosition,
                            TextPosition = occ.TextPosition
                        };
                        newPostingListOccurrences.TokenOccurrences.Add(occDto);
                    }
                }
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

            if (queryString == "EmptyQuery" || queryString == null)
            {
                await App.Current.MainPage.DisplayPromptAsync("Query Status", "Bad query, enter a valid query");
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                // C:\Users\robre\AppData\Local\Packages\A5924E32-1AFA-40FB-955D-1C58BE2D2ED5_9zz4h110yvjzm\LocalState\
                // QueryCommands.xml
                (bool queryExists, int queryId) = await repositoryService.QueryResultExistsAsync(queryString);
                QueryResultExists = queryExists;
                if (queryExists)
                {
                    // TODO: Add to Repository .... or change to Repository?
                    var queryResultById = await repositoryService.GetQueryResultByIdAsync(queryId);
                    var queryResultByQueryString = await repositoryService.GetQueryResultByQueryStringAsync(queryString);

                    var testQR = await repositoryService.GetTermOccurrencesByQueryResultIdAsync(queryId);

                    var termOccurrenceLst = await repositoryService.GetTermOccurrencesByQueryResultIdAsync(queryId);

                    var postingLst1 = await repositoryService.GetPostingByLexemeAsync("rejuvenation");
                    var stem = await repositoryService.GetTokenStemAsync("rejuvenation");

                    // 1. Get PostingList for each term in query use F# to parse QueryExpression and get posting lists
                    var postingLst2 = await repositoryService.GetPostingByLexemeAsync("foreword");
                    var postingLst3 = await repositoryService.GetPostingByLexemeAsync("orvonton");

                    // 2. Get TokenOccurrenceList for each PostingListId
                    var tokenOccurrences1 = await repositoryService.GetTokenOccurrencesByPostingListIdAsync(postingLst2.Id);
                    var tokenOccurrences2 = await repositoryService.GetTokenOccurrencesByPostingListIdAsync(postingLst3.Id);

                    //var parserService = new ParserService();
                    // 3. Use Linq QueryLanguage to filter to Hits
                    //var set1 = new SortedSet<TokenOccurrence>();
                    //var set2 = new SortedSet<TokenOccurrence>();
                    //foreach (var token in tokenOccurrences1)
                    //{
                    //    set1.Add(token);
                    //}
                    //foreach (var token in tokenOccurrences2)
                    //{
                    //    set2.Add(token);
                    //}
                    // join, union, etc., 

                    await LoadXaml(queryResultById);
                }
                else
                {
                    // Run Query
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

    private async Task LoadXaml(QueryResultLocations queryResultLocationsDto)
    {
        try
        {
            // See: https://learn.microsoft.com/en-us/dotnet/maui/xaml/runtime-load
            /*
            string navigationButtonXAML = "<Button Text=\"Navigate\" />";
            Button navigationButton = new Button().LoadFromXaml(navigationButtonXAML);
            stackLayout.Add(navigationButton); ...
            */

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


                var contentVSL = contentPage.FindByName("queryResultVSL") as VerticalStackLayout;

                var labelName = "_" + paperId.ToString("000") + "_" + seqId.ToString("000");

                // See: https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/label
                FormattedString fs = await CreateFormattedString(paragraph);

                Label label = new Label { FormattedText = fs };
                TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.SetBinding(TapGestureRecognizer.CommandProperty, "TappedGestureCommand");
                tapGestureRecognizer.CommandParameter = $"{labelName}";
                tapGestureRecognizer.NumberOfTapsRequired = 1;
                label.GestureRecognizers.Add(tapGestureRecognizer);

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
                    var length = termOcc.Length;
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
            await App.Current.MainPage.DisplayAlert("Exception raised in MainViewModel.NormalizeQueryString => ",
                ex.Message, "Cancel");
        }
    }

    Task Back() => Shell.Current.GoToAsync("..");
}
