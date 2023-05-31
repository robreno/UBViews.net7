using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls.Compatibility;

using UBViews.Services;
using UBViews.Models.Query;

using SQLiteRepository;
using SQLiteRepository.Dtos;
using SQLiteRepository.Models;

//using LexParser.Library;
using UBViews.LexParser;
namespace UBViews.ViewModels;

[QueryProperty(nameof(TokenCount), nameof(TokenCount))]
[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class QueryInputViewModel : BaseViewModel
{
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
    QueryInput queryInput;

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
                QueryCommands.Add(cmd);
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
                    var tokOccs = await repositoryService.GetTokenOccurrencesAsync(postingList.Id);
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
                (bool queryExists, int queryId) = await appDataService.QueryResultExistsAsync(queryString);
                (bool queryExistsRepo, int queryIdRepo) = await repositoryService.QueryResultExistsAsync(queryString);
                QueryResultExists = queryExists;
                if (queryExistsRepo)
                {
                    // TODO: Add to Repository .... or change to Repository?
                    var queryResultDto = await appDataService.GetQueryResultByIdAsync(queryIdRepo);
                    var queryResultDtoRepo = await repositoryService.GetQueryResultByIdAsync(queryIdRepo);
                    var termOccurrenceLst = await repositoryService.GetTermOccurrencesByQueryResultIdAsync(queryResultDtoRepo.Id);

                    var postingLst1 = await repositoryService.GetPostingByLexemeAsync("rejuvenation");
                    var stem = await repositoryService.GetTokenStemAsync("rejuvenation");

                    // 1. Get PostingList for each term in query use F# to parse QueryExpression and get posting lists
                    var postingLst2 = await repositoryService.GetPostingByLexemeAsync("foreword");
                    var postingLst3 = await repositoryService.GetPostingByLexemeAsync("orvonton");

                    // 2. Get TokenOccurrenceList for each PostingListId
                    var tokenOccurrences1 = await repositoryService.GetTokenOccurrencesAsync(postingLst2.Id);
                    var tokenOccurrences2 = await repositoryService.GetTokenOccurrencesAsync(postingLst3.Id);

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
                QueryInput = new QueryInput() { Text = qs, TokenCount = tokens.Length };
                QueryInputString = QueryInput.Text;
                TokenCount = QueryInput.TokenCount;
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
