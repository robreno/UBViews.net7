using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;

// Needed from GlobalUsings file
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Maui.Views;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;
using UBViews.Services;
using UBViews.Extensions;
using UBViews.Views;
using UBViews.Models.Notes;
using UBViews.Controls.Help;

namespace UBViews.ViewModels
{
    [QueryProperty(nameof(PaperDto), "PaperDto")]
    public partial class XamlPaperViewModel : BaseViewModel
    {
        #region  Private Data Members
        /// <summary>
        /// 
        /// </summary>
        public CultureInfo cultureInfo;

        /// <summary>
        /// 
        /// </summary>
        public ContentPage contentPage;

        /// <summary>
        /// MediaElement
        /// </summary>
        public IMediaElement mediaElement;

        /// <summary>
        /// 
        /// </summary>
        public MediaStatePair MediaState = new();

        /// <summary>
        /// 
        /// </summary>
        protected AudioMarkerSequence Markers { get; set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Paragraph> Paragraphs { get; private set; } = new();

        /// <summary>
        /// TODO: Currently not loading this collection
        /// </summary>
        public ObservableCollection<AudioMarker> AudioMarkers { get; private set; } = new();

        /// <summary>
        /// IFileService
        /// </summary>
        IFileService fileService;

        /// <summary>
        /// IAudioService
        /// </summary>
        IAudioService audioService;

        /// <summary>
        /// IEmailService
        /// </summary>
        IEmailService emailService;

        /// <summary>
        /// IAppSettingsService
        /// </summary>
        IAppSettingsService settingsService;

        /// <summary>
        /// IDownloadService
        /// </summary>
        IDownloadService downloadService;

        /// <summary>
        /// INoteService
        /// </summary>
        INoteService notesService;

        readonly string _class = "XamlPaperViewModel";
        #endregion

        #region Constructor
        /// <summary>
        /// CStor XamlPaperViewModel
        /// </summary>
        /// <param name="fileService"></param>
        public XamlPaperViewModel(IFileService fileService,
                                  IEmailService emailService,
                                  IAppSettingsService settingsService,
                                  IAudioService audioService,
                                  IDownloadService downloadService,
                                  INoteService notesService)
        {
            this.fileService = fileService;
            this.emailService = emailService;
            this.audioService = audioService;
            this.settingsService = settingsService;
            this.downloadService = downloadService;
            this.notesService = notesService;
            this.cultureInfo = new CultureInfo("en-US");
        }
        #endregion

        #region Observable Properties
        [ObservableProperty]
        bool isRefreshing;

        [ObservableProperty]
        PaperDto paperDto;

        [ObservableProperty]
        string paperTitle;

        [ObservableProperty]
        string paperAuthor;

        [ObservableProperty]
        string paperNumber;

        [ObservableProperty]
        double lineHeight;

        [ObservableProperty]
        bool showReferencePids;

        [ObservableProperty]
        bool showPlaybackControls;

        [ObservableProperty]
        TimeSpan position;

        [ObservableProperty]
        TimeSpan duration;

        [ObservableProperty]
        TimeSpan startTime;

        [ObservableProperty]
        TimeSpan endTime;

        [ObservableProperty]
        string previousState;

        [ObservableProperty]
        string currentState;

        [ObservableProperty]
        bool isScrollToLabel;

        [ObservableProperty]
        string scrollToLabelName;

        [ObservableProperty]
        string audioUriString;

        [ObservableProperty]
        Uri audioUri;

        [ObservableProperty]
        string downloadAudioStatus;
        #endregion

        #region Relay Commands
        [RelayCommand]
        async Task RefeshingView(PaperDto dto)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                int paperId = dto.Id;
                PaperTitle = "Paper " + paperId;
                PaperNumber = dto.Id.ToString("0");

                var paragraphs = await fileService.GetParagraphsAsync(paperId);
                if (Paragraphs.Count != 0)
                    return;

                foreach (var paragraph in paragraphs)
                    Paragraphs.Add(paragraph);
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
        async Task PaperViewAppearing(PaperDto dto)
        {
            string _method = "PaperViewAppearing";
            try
            {
                if (contentPage == null)
                {
                    // TODO: Warning Dialog
                    return;
                }
                else
                {
                    var paperId = dto.Id;
                    var notes = await notesService.GetNotesAsync();
                    var results = notes.Where(n => n.PaperId == paperId);
                    List<string> noteIds = new();
                    foreach (var note in results)
                    {
                        if (noteIds.Contains(note.LocationId))
                        {
                            continue;
                        }
                        await AddNoteIconAsync(note);
                        noteIds.Add(note.LocationId.ToString());
                    }

                    var hasValue = contentPage.Resources.TryGetValue("audioUri", out object uri);
                    if (hasValue)
                    {
                        AudioUriString = (string)uri;
                        AudioUri = new Uri(AudioUriString);
                    }

                    await downloadService.InitializeDataAsync(contentPage, dto);
                    DownloadAudioStatus = await settingsService.Get("audio_download_status", "off");
                    if (DownloadAudioStatus.Equals("on"))
                    {
                        await downloadService.DownloadAudioFileAsync();
                    }

                    await audioService.InitializeDataAsync(contentPage, mediaElement, dto);
                    await audioService.SetSendToastAsync(true);
#if WINDOWS
                    await audioService.SetPlatformAsync("WINDOWS");
#elif ANDROID        
                    await audioService.SetPlatformAsync("ANDROID");
#endif
                }

                PreviousState = "None";
                CurrentState = "None";
                MediaState.CurrentState = "None";
                MediaState.PreviousState = "None";

                PaperNumber = dto.Id.ToString("0");
                ShowReferencePids = await settingsService.Get("show_reference_pids", false);
                ShowPlaybackControls = await settingsService.Get("show_playback_controls", false);
                await audioService.SetMediaPlaybackControlsAsync(ShowPlaybackControls);

                string uid = dto.Uid;
                IsScrollToLabel = dto.ScrollTo;
                if (IsScrollToLabel)
                {
                    ScrollToLabelName = "_" + uid.Substring(4, 3) + "_" + uid.Substring(0, 3);
                }

                Markers = await audioService.LoadAudioMarkersAsync(PaperDto.Id);
                if (Markers.Size > 0)
                {
                    foreach (var marker in Markers.Values().ToList())
                    {
                        AudioMarkers.Add(marker);
                    }
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
        }

        [RelayCommand]
        async Task PaperViewLoaded(PaperDto dto)
        {
            string _method = "PaperViewLoaded";
            try
            {
                if (dto == null)
                {
                    // raise error.
                    return;
                }

                int paperId = dto.Id;
                var paragraphs = await fileService.GetParagraphsAsync(paperId);

                Paragraphs.Clear();
                foreach (var paragraph in paragraphs)
                {
                    Paragraphs.Add(paragraph);
                }
                await audioService.SetParagraphsAsync(Paragraphs.ToList());

                if (ShowReferencePids)
                {
                    await SetReferencePids();
                }

                if (ShowPlaybackControls)
                {
                    await audioService.SetMediaPlaybackControlsAsync(ShowPlaybackControls);
                }

                if (IsScrollToLabel)
                {
                    await ScrollTo(ScrollToLabelName);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
        }

        [RelayCommand]
        async Task PaperViewDisappearing(PaperDto dto)
        {
            string _method = "PaperViewDisappearing";
            try
            {
                // Do Nothing
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
        }

        [RelayCommand]
        async Task PaperViewUnloaded(PaperDto dto)
        {
            string _method = "PaperViewUnloaded";
            try
            {
                await audioService.DisconnectMediaElementAsync();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
        }

        [RelayCommand]
        async Task TappedGestureForPaper(string value)
        {
            string _method = "TappedGestureForPaper";
            try
            {
                if (contentPage == null) 
                { 
                    return;
                }

                string paperTitle = PaperTitle;
                string message = $"Playing {paperTitle}";
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task DoubleTappedGestureForPaper(string value)
        {
            string _method = "DoubleTappedGestureForPaper";
            try
            {
                if (contentPage == null)
                {
                    return;
                }

                string paperTitle = PaperTitle;
                string message = $"Playing {paperTitle}";

                //await audioService.DoubleTappedGestureForPaperAsync(value);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task TappedGesture(string id)
        {
            string _method = "TappedGesture";
            try
            {
                if (contentPage == null)
                {
                    return;
                }

                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                var audioStatus = await audioService.GetAudioStatusAsync();
                if (audioStatus)
                {
                    await audioService.TappedGestureAsync(id);
                }
                var _currState = MediaState.CurrentState;
                var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task DoubleTappedGesture(string id)
        {
            string _method = "DoubleTappedGesture";
            try
            {
                if (contentPage == null)
                {
                    return;
                }

                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                var audioStatus = await audioService.GetAudioStatusAsync();
                if (audioStatus)
                {
                    await audioService.DoubleTappedGestureAsync(id);
                }
                var _currState = MediaState.CurrentState;
                var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

#if ANDROID
        [RelayCommand]
        async Task SwipeGesture(string actionId)
        {
            string _method = "SwipeGesture";
            try
            {
                if (contentPage == null)
                {
                    return;
                }

                var actionArray = actionId.Split('_', StringSplitOptions.RemoveEmptyEntries);
                var paperId = Int32.Parse(actionArray[0]).ToString("0");
                var seqId = Int32.Parse(actionArray[1]).ToString("0");
                var paperSeqId = paperId + "." + seqId;
                var paragraph = Paragraphs.Where(p => p.PaperSeqId == paperSeqId).FirstOrDefault();
                var pid = paragraph.Pid;

                var plainText = await emailService.CreatePlainTextBodyAsync(paragraph);
                var htmlText = await emailService.CreateHtmlBodyAsync(paragraph);
                var autoSendRecipients = await emailService.GetAutoSendEmailListAsync();

                string action = await App.Current.MainPage.DisplayActionSheet("Action?", "Cancel", null, "Copy", "Share");

                if (autoSendRecipients.Count == 0)
                {
                    var contactsCount = await emailService.ContactsCountAsync();
                    string promptMessage = string.Empty;
                    string secondAction = string.Empty;

                    secondAction = " add or set contact(s) to AutoSend.";
                    promptMessage = $"You have no contacts or none are set to auto send.\r" +
                                    $"Please go to the Settigs => Contacts page and {secondAction}.";

                    await App.Current.MainPage.DisplayAlert("Share Email", promptMessage, "Cancel");
                    return;
                }

                string errorMsg = string.Empty;
                switch (action)
                {
                    case "Copy":
                        // Add paragraph text to clipboard
                        await Clipboard.Default.SetTextAsync(plainText);
                        await SendToastAsync($"Paragraph {pid} copied to clipboard!");
                        break;
                    case "Share":
                        // Share Paragraph
                        await emailService.ShareParagraphAsync(paragraph);
                        break;
                    case "Email":
                        // Email Paragraph
                        await emailService.EmailParagraphAsync(paragraph, IEmailService.EmailType.PlainText, 
                                                                          IEmailService.SendMode.AutoSend);
                        break;
                    case "Cancel":
                        break;
                    default:
                        errorMsg = "Unkown Command!";
                        await App.Current.MainPage.DisplayAlert("Unknown Action =>", errorMsg, "Cancel");
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
#endif

#if WINDOWS
        [RelayCommand]
        async Task FlyoutMenu(string actionId)
        {
            string _method = "FlyoutMenu";
            try
            {
                if (contentPage == null)
                { 
                    return;
                }

                var actionArray = actionId.Split('_', StringSplitOptions.RemoveEmptyEntries);
                var action = actionArray[0];
                var paperId = Int32.Parse(actionArray[1]).ToString("0");
                var seqId = Int32.Parse(actionArray[2]).ToString("0");
                var paperSeqId = paperId + "." + seqId;
                var paragraph = Paragraphs.Where(p => p.PaperSeqId == paperSeqId).FirstOrDefault();
                var pid = paragraph.Pid;

                var plainText = await emailService.CreatePlainTextBodyAsync(paragraph);
                var htmlText = await emailService.CreateHtmlBodyAsync(paragraph);

                var autoSendRecipients = await emailService.GetAutoSendEmailListAsync();

                if (action == "Share" || action == "Email")
                {
                    if (autoSendRecipients.Count == 0)
                    {
                        var contactsCount = await emailService.ContactsCountAsync();
                        string promptMessage = string.Empty;
                        string secondAction = string.Empty;

                        secondAction = " add or set contact(s) to AutoSend.";
                        promptMessage = $"You have no contacts or none are set to auto send.\r" +
                                        $"Please go to the Settigs => Contacts page and {secondAction}.";

                        await App.Current.MainPage.DisplayAlert("Share Email", promptMessage, "Cancel");
                        return;
                    }
                }

                string errorMsg = string.Empty;
                switch (action)
                {
                    case "Copy":
                        // Add paragraph text to clipboard
                        await Clipboard.Default.SetTextAsync(plainText);
                        await SendToastAsync($"Paragraph {pid} copied to clipboard!");
                        break;
                    case "Share":
                        // Share Paragraph
                        await emailService.ShareParagraphAsync(paragraph);
                        await SendToastAsync($"Paragraph {pid} shared!");
                        break;
                    case "Email":
                        // Email Paragraph
                        await emailService.EmailParagraphAsync(paragraph, IEmailService.EmailType.PlainText, 
                                                                          IEmailService.SendMode.AutoSend);
                        break;
                    case "Play":
                        //await audioService.PlayAudioAsync();
                        break;
                    case "Pause":
                        //await audioService.PauseAudioAsync();
                        break;
                    case "Stop":
                        //await audioService.StopAudioAsync();
                        break;
                    default:
                        errorMsg = "Unkown Command!";
                        await App.Current.MainPage.DisplayAlert("Unknown Action =>", errorMsg, "Cancel");
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
#endif

        [RelayCommand]
        async Task SetMediaPlaybackControls(bool value)
        {
            string _method = "SetMediaPlaybackControls";
            try
            {
                //await audioService.SetMediaPlaybackControlsAsync(value);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Stop and cleanup MediaElement when we navigate away
                    mediaElement.ShouldShowPlaybackControls = value;
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task SetPlaybackControlsStartTime(AudioMarker audioMarker)
        {
            string _method = "SetPlaybackControlsStartTime";
            try
            {
                //var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    mediaElement.SeekTo(audioMarker.StartTime);
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task SetReferencePids()
        {
            string _method = "SetReferencePids";
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    var vte = contentPage.Content.GetVisualTreeDescendants();
                    using (var enumerator = vte.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var child = enumerator.Current;
                            if (child != null)
                            {
                                var childType = child.GetType().Name;
                                if (childType == "Label")
                                {
                                    var lbl = child as Label;
                                    var styleId = lbl.StyleId;
                                    var spn = lbl.FindByName("SP" + styleId) as Span;
                                    if (spn != null)
                                    {
                                        var spanText = spn.Text;
                                        if (ShowReferencePids)
                                        {
                                            spn.Text = spn.StyleId;
                                        }
                                        else
                                        {
                                            spn.Text = "";
                                        }
                                    }
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task ScrollTo(string labelName)
        {
            string _method = "ScrollTo";
            try
            {
                var scrollView = contentPage.FindByName("contentScrollView") as ScrollView;
                var currentLabel = contentPage.FindByName(labelName) as Label;

                var lblArry = labelName.Split('_', StringSplitOptions.RemoveEmptyEntries);
                int paperId = Int32.Parse(lblArry[0]);
                int seqId = Int32.Parse(lblArry[1]);

                //var audioMarker = Markers.GetBySeqId(seqId);
                //await SetPlaybackControlsStartTime(audioMarker);

                // See Workaround for Maui bug #7295
                // https://github.com/dotnet/maui/issues/7295

                await Task.Delay(1000);

                var _x = currentLabel.X;
                var _y = currentLabel.Y;

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (scrollView != null && currentLabel != null)
                    {
                        scrollView.ScrollToAsync(currentLabel, ScrollToPosition.Start, false);
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudio()
        {
            string _method = "PlayAudio";
            try
            {
                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                //var audioStatus = await audioService.GetAudioStatusAsync();
                //if (audioStatus)
                //{
                //    await audioService.PlayAudioAsync();
                //}
                //var _currState = MediaState.CurrentState;
                //var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task PauseAudio()
        {
            string _method = "PauseAudio";
            try
            {
                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                //var audioStatus = await audioService.GetAudioStatusAsync();
                //if (audioStatus)
                //{
                //    await audioService.PauseAudioAsync();
                //}
                //var _currState = MediaState.CurrentState;
                //var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task StopAudio()
        {
            string _method = "StopAudio";
            try
            {
                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                var audioStatus = await audioService.GetAudioStatusAsync();

                if (audioStatus)
                {
                    await audioService.StopAudioAsync();
                }
                var _currState = MediaState.CurrentState;
                var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudioRange(string timeSpanRange)
        {
            string _method = "PlayAudioRange";
            try
            {
                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                //var audioStatus = await audioService.GetAudioStatusAsync();
                //if (audioStatus)
                //{
                //    await PlayAudioRange(timeSpanRange);
                //}
                //var _currState = MediaState.CurrentState;
                //var _prevState = MediaState.PreviousState;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudioRangeEx(AudioMarker audioMarker)
        {
            string _method = "PlayAudioRangeEx";
            try
            {
                CurrentState = MediaState.CurrentState;
                PreviousState = MediaState.PreviousState;
                //var audioStatus = await audioService.GetAudioStatusAsync();
                //if (audioStatus)
                //{
                //    await audioService.PlayAudioRangeExAsync(audioMarker);
                //}
                //var _currState = MediaState.CurrentState;
                //var _prevState = MediaState.PreviousState;
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
                await audioService.StateChangedAsync(state);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
        #endregion

        #region Pivate Helper Methods
        /// <summary>
        /// LoadAudioMarkers
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        async Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId)
        {
            string _method = "LoadAudioMarkersAsync";
            try
            {
                this.Markers = await audioService.LoadAudioMarkersAsync(paperId);
                return this.Markers;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return null;
            }
        }

        /// <summary>
        /// AddNoteIconAsync
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        private async Task AddNoteIconAsync(NoteEntry note)
        {
            string _method = "AddNoteIconAsync";
            try
            {
                var pid = note.Pid;
                var locationId = note.LocationId;
                var arry = locationId.Split('.');
                var labelName = "_" + Int32.Parse(arry[0]).ToString("000")
                                + "_" + Int32.Parse(arry[1]).ToString("000");
                var vsl = "VSL" + labelName;
                var labelVsl = contentPage.FindByName(vsl) as VerticalStackLayout;

                if (labelVsl != null)
                {
                    var border = await notesService.CreateNoteBorderAsync(note);
                    var fst = labelVsl.First();
                    labelVsl.Clear();
                    labelVsl.Add(border);
                    labelVsl.Add(fst);
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            }
        }

        /// <summary>
        /// SendToast
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        async Task SendToastAsync(string message)
        {
            string _method = "SendToastAsync";
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
                await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
                return;
            }
        }
        #endregion
    }
}
