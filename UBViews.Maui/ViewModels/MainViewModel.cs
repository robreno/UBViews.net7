﻿namespace UBViews.ViewModels;

using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Services;
using UBViews.Helpers;
using UBViews.Models.Query;
using UBViews.Models.Audio;
using UBViews.Views;
using UBViews.Models;

using QueryEngine;
using QueryFilter;
using static DataTypesEx;

[QueryProperty(nameof(QueryInput), nameof(QueryInput))]
public partial class MainViewModel : BaseViewModel
{
    #region   Private Data Members
    /// <summary>
    /// CultureInfo
    /// </summary>
    public CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage 
    /// </summary>
    public ContentPage contentPage;

    /// <summary>
    /// MediaStatePair
    /// </summary>
    public MediaStatePair MediaState = new();

    /// <summary>
    /// IMediaElement
    /// </summary>
    public IMediaElement mediaElement;

    /// <summary>
    /// Services
    /// </summary>
    IFileService fileService;
    IAppSettingsService settingsService;
    IFSRepositoryService repositoryService;
    IQueryProcessingService queryProcessingService;
    IAudioService audioService;
    IConnectivity connectivityService;

    ParserService parserService;
    ConnectivityViewModel connectivityViewModel;

    readonly string _class = "MainViewModel";
    #endregion

    #region   Constructor
    public MainViewModel(IFileService fileService, 
                         IFSRepositoryService repositoryService,
                         IAppSettingsService settingsService,
                         IQueryProcessingService queryProcessingService, 
                         IAudioService audioService,
                         IConnectivity connectivityService)
    {
        this.fileService = fileService;
        this.settingsService = settingsService;

        this.repositoryService = repositoryService;
       
        this.queryProcessingService = queryProcessingService;
        this.audioService = audioService;
        this.connectivityService = connectivityService;

        this.parserService = new ParserService();
        connectivityViewModel = new ConnectivityViewModel(connectivityService);
    }
    #endregion

    #region  Observable Properties
    [ObservableProperty]
    string audioStatus = "off";

    [ObservableProperty]
    string audioDownloadStatus = "off";

    [ObservableProperty]
    bool audioStreaming = false;

    [ObservableProperty]
    string currentState = "None";

    [ObservableProperty]
    string previousState = "None";

    [ObservableProperty]
    string audioFolderName = null;

    [ObservableProperty]
    string audioFolderPath = null;

    [ObservableProperty]
    string localAudioFilePathName;

    [ObservableProperty]
    string localResourceAudioFilePathName = "Audio/BookIntro.mp3";

    [ObservableProperty]
    bool isInitialized;

    [ObservableProperty]
    bool isRefreshing;

    [ObservableProperty]
    QueryResultLocationsDto queryLocations;

    [ObservableProperty]
    QueryInputDto queryInputDto;

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
    #endregion

    #region  Relay Commands
    [RelayCommand]
    async Task MainPageAppearing()
    {
        string _method = "MainPageAppearing";
        try
        {
            if (contentPage == null)
            {
                return;
            }

            if (mediaElement == null)
            {
                return;
            }

            QueryInputDto = new QueryInputDto() { Text = "[Empty]", TokenCount = 0 };

            AudioStatus = await settingsService.Get("audio_status", "off");
            AudioStreaming = await settingsService.Get("stream_audio", false);
            AudioDownloadStatus = await settingsService.Get("audio_download_status", "off");

            await audioService.SetContentPageAsync(contentPage);
            await audioService.SetMediaElementAsync(mediaElement);
            await audioService.SetMediaSourceAsync("loadLocalResource",
                                                   LocalResourceAudioFilePathName);

            bool hasInternet = await CheckInternetAsync();
            Preferences.Default.Set("has_internet", hasInternet);

            if (!IsInitialized)
            {
                await queryProcessingService.SetContentPageAsync(contentPage);
                MaxQueryResults = await settingsService.Get("max_query_results", 50);
                await queryProcessingService.SetMaxQueryResultsAsync(MaxQueryResults);
                isInitialized = true;
            }

            string titleMessage = $"UBViews Home";
            Title = titleMessage;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageDisappearing()
    {
        string _method = "MainPageDisappearing";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.Stop();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task MainPageUnloaded()
    {
        string _method = "MainPageUnloaded";
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Stop and cleanup MediaElement when we navigate away
                mediaElement.Stop();
                mediaElement.Handler?.DisconnectHandler();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    async Task TappedGesture(string action)
    {
        string _method = "TappedGesture";
        try
        {
            IsBusy = true;

            if (contentPage == null)
            {
                return;
            }

            if (mediaElement == null)
            {
                return;
            }

            if (AudioStatus.Equals("off"))
            {
                return;
            }

            string errorMsg = string.Empty;
            bool _mediaStateDirty = false;
            MediaStatePair _statePair = await audioService.GetMediaStateAsync();
            //await audioService.TappedGestureAsync(action, _mediaStateDirty);

            var _state = mediaElement.CurrentState;
            var _stateStr = _state.ToString();
            PreviousState = CurrentState;
            CurrentState = _stateStr;
            _statePair.SetState(_stateStr);

            switch (_stateStr)
            {
                case "Paused":
                case "Stopped":
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        mediaElement.Play();
                    });
                    break;
                case "Playing":
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        mediaElement.Pause();
                    });
                    break;
                case "Failed":
                    break;
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
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
        string _method = "SubmitQuery";
        try
        {
            // temple and prostitutes
            // prostitutes and temple
            // foreword and orvonton

            IsBusy = true;
            string message = string.Empty;
            bool parsingSuccessful = false;
            bool runPreCheckSilent = await settingsService.Get("run_precheck_silent", true);

            QueryInputString = queryString;
            (bool result, message) = await queryProcessingService.PreCheckQueryAsync(QueryInputString,
                                                                                     runPreCheckSilent);
            if (message.Contains("="))
            {
                if (message.Contains("Audio status"))
                {
                    AudioStatus = await settingsService.Get("audio_status", "off");
                }
                else if (message.Contains("Audio streaming"))
                {
                    AudioStreaming = await settingsService.Get("stream_audio", false);
                }
                else if (message.Contains("Audio download"))
                {
                    AudioDownloadStatus = await settingsService.Get("audio_download_status", "off");
                }
                return;
            }

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
            else // Query parsing error
            {
                string _msg = $"{message}";
                QueryInputString = await App.Current.MainPage.DisplayPromptAsync("Query Parsing Error",
                    _msg, "Ok", "Cancel", "Retry Query here ..", -1, null, "");
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
    async Task MediaOpened(TimeSpan timeSpan)
    {
        string _method = "MediaOpened";
        try
        {
            //
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task PositionChanged(TimeSpan timeSpan)
    {
        string _method = "PositionChanged";
        try
        {
            await audioService.PositionChangedAsync(timeSpan);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task StateChanged(string state)
    {
        string _method = "StateChanged";
        try
        {
            // Opening, Paused
            await audioService.StateChangedAsync(state);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    [RelayCommand]
    async Task MediaEnded(TimeSpan timeSpan)
    {
        string _method = "MediaEnded";
        try
        {
            //
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
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
            else if (target == "PopupHelp")
            {
                targetName = nameof(HelpPage);
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
    #endregion

    #region  Helper Methods
    private async Task<bool> CheckInternetAsync()
    {
        string _method = "CheckInternetAsync";
        try
        {
            bool isInternet = false;
            NetworkAccess accessType = connectivityService.NetworkAccess;
            if (accessType == NetworkAccess.Internet)
            {
                isInternet = true;
            }
            return isInternet;

        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }
    private async Task SendToastAsync(string message)
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
    #endregion
}
