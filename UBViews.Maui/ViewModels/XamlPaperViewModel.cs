using System.Text;
using System.Globalization;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;
using UBViews.Services;
using UBViews.Extensions;

namespace UBViews.ViewModels
{
    [QueryProperty(nameof(PaperDto), "PaperDto")]
    public partial class XamlPaperViewModel : BaseViewModel
    {
        /// <summary>
        /// 
        /// </summary>
        public CultureInfo cultureInfo;

        /// <summary>
        /// 
        /// </summary>
        public ContentPage contentPage;

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
        /// 
        /// </summary>
        IFileService fileService;

        /// <summary>
        /// 
        /// </summary>
        IAudioService audioService;

        /// <summary>
        /// 
        /// </summary>
        IAppSettingsService settingsService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileService"></param>
        public XamlPaperViewModel(IFileService fileService, IAppSettingsService settingsService, IAudioService audioService)
        {
            this.fileService = fileService;
            this.audioService = audioService;
            this.settingsService = settingsService;
            this.cultureInfo = new CultureInfo("en-US");
            PreviousState = "None";
            CurrentState = "None";
        }

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

        //[ObservableProperty]
        //string mediaElementPreviousState;

        //[ObservableProperty]
        //string mediaElementCurrentState;

        [ObservableProperty]
        bool isScrollToLabel;

        [ObservableProperty]
        string scrollToLabelName;

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
            try
            {
                CurrentState = "None";
                PreviousState = "None";

                PaperNumber = dto.Id.ToString("0");
                ShowReferencePids = await settingsService.Get("show_reference_pids", false);
                ShowPlaybackControls = await settingsService.Get("show_playback_controls", false);

                string uid = dto.Uid;
                IsScrollToLabel = dto.ScrollTo;
                if (IsScrollToLabel)
                {
                    ScrollToLabelName = "_" + uid.Substring(4, 3) + "_" + uid.Substring(0, 3);
                }

                Markers = await LoadAudioMarkers(PaperDto.Id);
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
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task PaperViewLoaded(PaperDto dto)
        {
            try
            {
                if (Paragraphs.Count != 0)
                    return;

                int paperId = dto.Id;
                var paragraphs = await fileService.GetParagraphsAsync(paperId);

                foreach (var paragraph in paragraphs)
                {
                    Paragraphs.Add(paragraph);
                }

                if (ShowReferencePids)
                {
                    await SetReferencePids();
                }

                if (ShowPlaybackControls)
                {
                    await SetMediaPlaybackControls(ShowPlaybackControls);
                }

                if (IsScrollToLabel)
                {
                    await ScrollTo(ScrollToLabelName);
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task PaperViewDisappearing(PaperDto dto)
        {
            try
            {
                await StopAudio();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task PaperViewUnloaded(PaperDto dto)
        {
            try
            {
                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Stop and cleanup MediaElement when we navigate away
                    me.Handler?.DisconnectHandler();
                });
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            }
        }

        [RelayCommand]
        async Task TappedGestureForPaper(string value)
        {
            try
            {
                if (contentPage == null)
                    return;

                string paperTitle = PaperTitle;

                string message = $"Playing {paperTitle}";

                if (CurrentState == "None" ||
                    CurrentState == "Paused" ||
                    CurrentState == "Stopped")
                {
                    await PlayAudio();
                }
                else if (CurrentState == "Playing")
                {
                    await PauseAudio();
                    message = $"Pausing {paperTitle} Timespan {value}";
                }
                else if (CurrentState == "Paused" && PreviousState == "Playing")
                {
                    await PlayAudio();
                    message = $"Resume Playing {paperTitle} Timespan {value}";
                }
                else
                {
                    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                    throw new Exception("Uknown State: " + msg);
                }

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
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task DoubleTappedGestureForPaper(string value)
        {
            try
            {
                if (contentPage == null)
                    return;

                string paperTitle = PaperTitle;
                string message = $"Stopping {paperTitle}";

                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                {
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(message, duration, fontSize);
                    await toast.Show(cancellationTokenSource.Token);
                }

                await StopAudio();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task TappedGesture(string id)
        {
            try
            {
                if (contentPage == null)
                    return;

                int paperId = Int32.Parse(id.Substring(1, 3));
                int sequenceId = Int32.Parse(id.Substring(5,3));
                var audioMarker = AudioMarkers.Where(m => m.SequenceId == sequenceId).FirstOrDefault();

                var startTimeStr = audioMarker.StartTime.ToLongTimeString();
                var endTimeStr = audioMarker.EndTime.ToLongTimeString();
                string timeRange = startTimeStr + " - " + endTimeStr;

                Label currentLabel = (Label)contentPage.FindByName(id);
                string timeSpanRange = currentLabel.GetValue(AttachedProperties.Audio.TimeSpanProperty) as string;
                string timeSpanRangeMsg = timeSpanRange.Replace("_", " - ");
                string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
                // 001.000.000.000
                string[] arr = uid.Split('.');
                string pid = Int32.Parse(arr[1]).ToString("0")
                             + ":" +
                             Int32.Parse(arr[2]).ToString("0")
                             + "." +
                             Int32.Parse(arr[3]).ToString("0");

                string message = $"Playing {pid} Timespan {timeRange}";

                // Initial State -> Trigger Play
                if (CurrentState == "None" ||
                    PreviousState == "None")
                {
                    await PlayAudioRangeEx(audioMarker);
                }
                // Play State -> Tappeed Event -> 
                /* || PreviousState = "Paused" */
                else if (CurrentState == "Playing" ||
                         PreviousState == "None") 
                {
                    await PauseAudio();
                    message = $"Pausing {pid} Timespan {timeSpanRangeMsg}";
                }
                // Playing State -> Play Trigger
                else if (CurrentState == "Paused" ||
                         PreviousState == "Playing")
                {
                    await PlayAudio();
                    message = $"Resuming {pid} Timespan {timeSpanRangeMsg}";
                }
                else
                {
                    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                    throw new Exception("Uknown State: " + msg);
                }

                // Opening State for Windows
                //if (CurrentState == "Paused" && PreviousState == "Opening" ||
                //    CurrentState == "Paused" && PreviousState == "Stopped")
                //{
                //    await PlayAudioRange(timeSpanRange);
                //}
                //else if (CurrentState == "Playing" && PreviousState == "Paused" ||
                //         CurrentState == "Playing" && PreviousState == "Buffering")
                //{
                //    await PauseAudio();
                //    message = $"Pausing {pid} Timespan {timeSpanRangeMsg}";
                //}
                //else
                //{
                //    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                //    throw new Exception("Uknown State: " + msg);
                //}

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
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task DoubleTappedGesture(string id)
        {
            try
            {
                if (contentPage == null)
                    return;

                Label currentLabel = (Label)contentPage.FindByName(id);
                string timeSpanRange = currentLabel.GetValue(AttachedProperties.Audio.TimeSpanProperty) as string;
                string timeSpanRangeMsg = timeSpanRange.Replace("_", " - ");
                string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
                // 001.000.000.000
                string[] arr = uid.Split('.');
                string pid = Int32.Parse(arr[1]).ToString("0")
                             + ":" +
                             Int32.Parse(arr[2]).ToString("0")
                             + "." +
                             Int32.Parse(arr[3]).ToString("0");

                //string format = @"dd\:hh\:mm\:ss\.fffffff";
                // Console.WriteLine("The time difference is: {0}", ts.ToString(format));
                //string format = @"dd\:hh\:mm\:ss\.fffffff";

                //string format = @"hh\:mm\:ss\.ff";
                //var hrs = timeSpan.TotalHours;
                //var min = timeSpan.TotalMinutes;
                //var sec = timeSpan.TotalSeconds;
                //var str1 = timeSpan.ToShortTimeString();
                //var str2 = timeSpan.ToString(format);

                string message = $"Stopping {pid} Timespan {timeSpanRangeMsg}";

                using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                {
                    ToastDuration duration = ToastDuration.Short;
                    double fontSize = 14;
                    var toast = Toast.Make(message, duration, fontSize);
                    await toast.Show(cancellationTokenSource.Token);
                }

                await StopAudio();
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PointGesture(string actionId)
        {
            try
            {
                if (contentPage == null)
                    return;

                var msg = "PointGestureCommand " + actionId;
                await App.Current.MainPage.DisplayAlert("Gesture Regognizer =>", msg, "Cancel");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task SwipeRightGesture(string actionId)
        {
            try
            {
                if (contentPage == null)
                    return;

                //var msg = "SwipRightGestureCommand " + actionId;
                //await App.Current.MainPage.DisplayAlert("Gesture Regognizer =>", msg, "Cancel");
                // https://learn.microsoft.com/en-us/dotnet/maui/user-interface/pop-ups
                string action = await App.Current.MainPage.DisplayActionSheet("ActionSheet: Send to?", "Cancel", null, "Email", "Twitter", "Facebook");
                await App.Current.MainPage.DisplayAlert("Sending Paragraph to =>", action, "Cancel");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task SwipeLeftGesture(string actionId)
        {
            try
            {
                if (contentPage == null)
                    return;

                // https://learn.microsoft.com/en-us/answers/questions/1187166/maui-android-is-it-possible-to-highlights-text-in
                // https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/clipboard
                // https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/data/share?tabs=android
                string action = await App.Current.MainPage.DisplayActionSheet("ActionSheet: Archive to?", "Cancel", null, "Local", "Server", "Clipboard");

                string msg = string.Empty;
                switch (action)
                {
                    case "Cancel":
                        // Do nothing
                        break;
                    case "Local":
                        msg = "Archive to local storage is not implemented";
                        await App.Current.MainPage.DisplayAlert("Action =>", msg, "Cancel");
                        break;
                    case "Server":
                        msg = "Archive to server storage is not implemented";
                        await App.Current.MainPage.DisplayAlert("Action =>", msg, "Cancel");
                        break;
                    case "Clipboard":
                        var lbl = contentPage.FindByName(actionId) as Label;
                        var formattedText = lbl.FormattedText;
                        var spans = formattedText.Spans;
                        StringBuilder sb = new StringBuilder();
                        var pretext = "I thought of you when I read this quote from The Urantia Book by The Urantia Foundation - ";
                        sb.AppendLine(pretext);
                        sb.AppendLine("");
                        foreach (var span in spans)
                        {
                            sb.Append(span.Text);
                        }
                        var text = sb.ToString();
                        // Cleear Clipboard of any old content
                        await Clipboard.Default.SetTextAsync(null);
                        // Add paragraph text to clipboard
                        await Clipboard.Default.SetTextAsync(text);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Gesture Regognizer => ", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task FlyoutMenu(string actionId)
        {
            try
            {
                if (contentPage == null)
                    return;

                var msg = "FlyoutMenuCommand " + actionId;
                await App.Current.MainPage.DisplayAlert("Gesture Regognizer => ", msg, "Cancel");
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task SetMediaPlaybackControls(bool value)
        {
            try
            {
                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // Stop and cleanup MediaElement when we navigate away
                    me.ShouldShowPlaybackControls = value;
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task SetPlaybackControlsStartTime(AudioMarker audioMarker)
        {
            try
            {
                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    me.SeekTo(audioMarker.StartTime);
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task SetReferencePids()
        {
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
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task ScrollTo(string labelName)
        {
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
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudio()
        {
            try
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (me != null)
                    {
                        me.Play();
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PauseAudio()
        {
            try
            {
                PreviousState = CurrentState;
                CurrentState = "Paused";

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (me != null)
                    {
                        me.Pause();
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task StopAudio()
        {
            try
            {
                PreviousState = CurrentState;
                CurrentState = "Stopped";

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (me != null)
                    {
                        me.Stop();
                        me.MediaEnded();
                    }
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudioRange(string timeSpanRange)
        {
            try
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";

                char[] separators = { ':', '.' };
                string[] arry = timeSpanRange.Split('_');
                string[] sa = arry[0].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                string[] ea = arry[1].Split(separators, StringSplitOptions.RemoveEmptyEntries);
                TimeSpan start = new TimeSpan(0, Int32.Parse(sa[0]), Int32.Parse(sa[1]), Int32.Parse(sa[2]), Int32.Parse(sa[3]));
                TimeSpan end = new TimeSpan(0, Int32.Parse(ea[0]), Int32.Parse(ea[1]), Int32.Parse(ea[2]), Int32.Parse(ea[3]));

                StartTime = start;
                EndTime = end;

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    me.SeekTo(start);
                    me.Play();
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PlayAudioRangeEx(AudioMarker audioMarker)
        {
            try
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";

                TimeSpan start = audioMarker.StartTime;
                TimeSpan end = audioMarker.EndTime;

                StartTime = start;
                EndTime = end;

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    me.SeekTo(start);
                    me.Play();
                });
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task PositionChanged(TimeSpan timeSpan)
        {
            try
            {
                Position = timeSpan;

                var me = contentPage.FindByName("mediaElement") as IMediaElement;
                if (EndTime.ToShortTimeString() == timeSpan.ToShortTimeString())
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        if (me != null)
                        {
                            me.Stop();
                            me.MediaEnded();
                        }
                    });
                    MediaState.SetState(me.CurrentState.ToString());
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        [RelayCommand]
        async Task StateChanged(string state)
        {
            try
            {
                var currentState = state;
                MediaState.SetState(state);
                // TODO: Remove Above
                //MediaElementPreviousState = MediaElementCurrentState;
                //MediaElementCurrentState = state;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paperId"></param>
        /// <returns></returns>
        async Task<AudioMarkerSequence> LoadAudioMarkers(int paperId)
        {
            try
            {
                this.Markers = await audioService.LoadAudioMarkers(paperId);
                return this.Markers;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
                return null;
            }
        }
    }
}
