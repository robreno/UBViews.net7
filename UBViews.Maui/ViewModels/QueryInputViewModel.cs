using System.Collections.ObjectModel;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;

using UBViews.Services;
using UBViews.Models.Query;
using UBViews.LexParser;

//using SQLiteRepository;
//using SQLiteRepository.Dtos;

namespace UBViews.ViewModels;

[QueryProperty(nameof(TokenCount), nameof(TokenCount))]
[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class QueryInputViewModel : BaseViewModel
{
    public ContentPage contentPage;

    IAppDataService appDataService;

    IFileService fileService;

    public ObservableCollection<QueryCommand> QueryCommands { get; } = new();

    ParserService parserService;

    public QueryInputViewModel(IAppDataService appDataService, IFileService fileService)
    {
        this.appDataService = appDataService;
        this.fileService = fileService;
        this.parserService = new ParserService();
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
    async Task QueryInputPageLoaded()
    {
        if (IsBusy == true)
            return;

        try
        {
            IsBusy = true;

            var commands = await appDataService.GetQueryCommandsAsync();
            if (QueryCommands.Count != 0)
                return;

            foreach (var command in commands)
                QueryCommands.Add(command);

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
                QueryExpression = parserService.ParseQuery(QueryInput.Text).ToString();
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
                QueryResultExists = queryExists;
                if (queryExists)
                {
                    var queryResultDto = await appDataService.GetQueryResultByIdAsync(queryId);
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
