using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
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
        public ContentPage contentPage;

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<Paragraph> Paragraphs { get; private set; } = new();

        /// <summary>
        /// 
        /// </summary>
        public ObservableCollection<AudioMarker> AudioMarkers { get; private set; } = new();

        /// <summary>
        /// 
        /// </summary>
        IFileService fileService;

        /// <summary>
        /// 
        /// </summary>
        IAppSettingsService settingsService;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileService"></param>
        public XamlPaperViewModel(IFileService fileService, IAppSettingsService settingsService)
        {
            this.fileService = fileService;
            this.settingsService = settingsService;
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
        string paperNumber;

        [ObservableProperty]
        bool showReferencePids;

        [ObservableProperty]
        TimeSpan currentPosition;

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
        async Task PaperViewLoaded(PaperDto dto)
        {
            if (IsBusy)
                return;

            try
            {
                IsBusy = true;

                var showPids = await settingsService.Get("show_pids", true);
                ShowReferencePids = showPids;
               
                int paperId = dto.Id;
                string uid = dto.Uid;
                IsScrollToLabel = dto.ScrollTo;
                if (IsScrollToLabel)
                {
                    ScrollToLabelName = "_" + uid.Substring(4, 3) + "_" + uid.Substring(0, 3);
                }
                PaperTitle = "Paper " + paperId;
                PaperNumber = paperId.ToString("0");

                var paragraphs = await fileService.GetParagraphsAsync(paperId);
                if (Paragraphs.Count != 0)
                    return;

                foreach (var paragraph in paragraphs)
                    Paragraphs.Add(paragraph);

                if (IsScrollToLabel)
                {
                    await ScrollTo(ScrollToLabelName);
                }
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

        //[RelayCommand]
        //async Task PaperViewDisappearing()
        //{

        //}

        [RelayCommand]
        async Task TappedGestureForPaper(string value)
        {
            try
            {
                if (contentPage == null)
                    return;

                string paperTitle = PaperTitle;
                string message = $"Playing {paperTitle} Timespan {value}";

#if ANDROID
                // Opening State for Windows or After Completion of PlayAudioRange
                if (CurrentState == "Paused" && PreviousState == "Buffering" ||
                    CurrentState == "Stopped" && PreviousState == "Buffering")
                {
                    await PlayAudio();
                }
                else if (CurrentState == "Playing" && PreviousState == "Paused" ||
                         CurrentState == "Playing" && PreviousState == "Buffering")
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
#elif WINDOWS
                // Opening State for Windows
                if (CurrentState == "Paused" && PreviousState == "Opening" ||
                    CurrentState == "Paused" && PreviousState == "Stopped")
                {
                    await PlayAudio();
                }
                else if (CurrentState == "Playing" && PreviousState == "Paused" ||
                         CurrentState == "Playing" && PreviousState == "Buffering")
                {
                    await PauseAudio();
                    message = $"Pausing {paperTitle} Timespan {value}";
                }
                else if (CurrentState == "Paused" && PreviousState == "Playing")
                {
                    await PlayAudio();
                    message = $"Resume Playing {paperTitle} Timespan {value}";
                }
                else if (CurrentState == "Stopped" && PreviousState == "Paused")
                {
                    await PlayAudio();
                    message = $"Playing {paperTitle} Timespan {value}";
                }
                else
                {
                    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                    throw new Exception("Uknown State: " + msg);
                }

#endif
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
                string message = $"Stopping {paperTitle} Timespan {value}";

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

                Label currentLabel = (Label)contentPage.FindByName(id);
                string timeSpanRange = currentLabel.GetValue(AttachedProperties.Audio.TimeSpanProperty) as string;
                string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
                // 001.000.000.000
                string[] arr = uid.Split('.');
                string pid = Int32.Parse(arr[1]).ToString("0")
                             + ":" +
                             Int32.Parse(arr[2]).ToString("0")
                             + "." +
                             Int32.Parse(arr[3]).ToString("0");

                string message = $"Playing {pid} Timespan {timeSpanRange}";

#if ANDROID
                // Opening State for Windows or After Completion of PlayAudioRange
                if (CurrentState == "Paused" && PreviousState == "Buffering" ||
                    CurrentState == "Stopped" && PreviousState == "Buffering")
                {
                    await PlayAudioRange(timeSpanRange);
                }
                else if (CurrentState == "Playing" && PreviousState == "Paused" ||
                         CurrentState == "Playing" && PreviousState == "Buffering")
                {
                    await PauseAudio();
                    message = $"Pausing {pid} Timespan {timeSpanRange}";
                }
                else if (CurrentState == "Paused" && PreviousState == "Playing")
                {
                    await PlayAudio();
                    message = $"Resume Playing {pid} Timespan {timeSpanRange}";
                }
                else
                {
                    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                    throw new Exception("Uknown State: " + msg);
                }
#elif WINDOWS
                // Opening State for Windows
                if (CurrentState == "Paused" && PreviousState == "Opening" ||
                    CurrentState == "Paused" && PreviousState == "Stopped")
                {
                    await PlayAudioRange(timeSpanRange);
                }
                else if (CurrentState == "Playing" && PreviousState == "Paused" ||
                         CurrentState == "Playing" && PreviousState == "Buffering")
                {
                    await PauseAudio();
                    message = $"Pausing {pid} Timespan {timeSpanRange}";
                }
                else if (CurrentState == "Paused" && PreviousState == "Playing")
                {
                    await PlayAudio();
                    message = $"Resume Playing {pid} Timespan {timeSpanRange}";
                }
                else
                {
                    string msg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                    throw new Exception("Uknown State: " + msg);
                }
#endif
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
                string uid = currentLabel.GetValue(AttachedProperties.Ubml.UniqueIdProperty) as string;
                // 001.000.000.000
                string[] arr = uid.Split('.');
                string pid = Int32.Parse(arr[1]).ToString("0")
                             + ":" +
                             Int32.Parse(arr[2]).ToString("0")
                             + "." +
                             Int32.Parse(arr[3]).ToString("0");

                string message = $"Stopping {pid} Timespan {timeSpanRange}";

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
        async Task ScrollTo(string labelName)
        {
            try
            {
                var scrollView = contentPage.FindByName("contentScrollView") as ScrollView;
                var label = contentPage.FindByName(labelName) as Label;
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (scrollView != null && label != null)
                    {
#if ANDROID
                        var _x = label.X;
                        var _y = label.Y;
                        scrollView.ScrollToAsync(_x, _y, false);
#elif WINDOWS
                        scrollView.ScrollToAsync(label, ScrollToPosition.Start, false);
#endif
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
                PreviousState = CurrentState;
                CurrentState = state;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
                return;
            }
        }
        async Task SetReferencePids(bool value)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (!ShowReferencePids)
                    {
                        var count = Paragraphs.Count();
                        foreach (var paragraph in Paragraphs)
                        {
                            var seqId = paragraph.SeqId;
                            var pid = paragraph.Pid;
                            var spanName = "span_" + seqId.ToString("000");
                            var span = contentPage.FindByName(spanName) as Span;
                            var spanText = span.Text;
                            span.Text = "";
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

    }
}
