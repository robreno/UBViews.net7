namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
using Microsoft.FSharp.Collections;

using System.Xml.Linq;
using System.Linq;
using Microsoft.FSharp.Core;

using System.Globalization;
using System.Collections.ObjectModel;

using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Primitives;

// Needed from GlobalUsings file
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;
using UBViews.Services;
using UBViews.Helpers;
using UBViews.Extensions;

public partial class AudioService : IAudioService
{
    #region  Private Data
    private Dictionary<int, List<int>> _astriskDic = new Dictionary<int, List<int>>()
    {
        { 31, new List<int> { 92 } },
        { 56, new List<int> { 92 } },
        { 120, new List<int> { 41 } },
        { 134, new List<int> { 70 } },
        { 196, new List<int> { 78 } },
        { 144, new List<int> { 70, 84, 97, 113, 132, 146 } }
    };

    /// <summary>
    /// CultureInfo
    /// </summary>
    private CultureInfo cultureInfo;

    /// <summary>
    /// ContentPage
    /// </summary>
    private ContentPage contentPage;

    /// <summary>
    /// MediaElement
    /// </summary>
    private IMediaElement mediaElement;

    /// <summary>
    /// 
    /// </summary>
    public HttpClient httpClient;

    /// <summary>
    /// ObservableCollection
    /// </summary>
    public ObservableCollection<AudioMarker> AudioMarkers { get; private set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public ObservableCollection<Paragraph> Paragraphs { get; private set; } = new();

    private readonly string _class = "AudioService";
    #endregion

    #region  Services
    IFileService fileService;
    IAppSettingsService settingsService;
    #endregion

    #region  Constructors
    public AudioService(IFileService fileService)
    {
        this.fileService = fileService;
        this.settingsService = ServiceHelper.Current.GetService<IAppSettingsService>();
    }
    #endregion

    #region  Public Propoerty Methods
    public bool ContentPageInitialized { get; set; } = false;
    public bool MediaElementInitialized { get; set; } = false;
    public bool ShowPlaybackControls { get; set; } = false;
    public bool SendToastState { get; set; } = false;
    public bool AudioDownloadStatus { get; set; } = false;
    public bool AudioStreamingStatus { get; set; } = false;
    public MediaStatePair MediaState { get; set; } = new();
    public MediaStatePair MediaElementMediaState { get; set; } = new();
    public AudioFlag AudioStatus { get; set; } = new();
    public AudioMarkerSequence Markers { get; set; } = new();
    public PaperDto PaperDto { get; set; } = null;
    public int PaperId { get; set; }
    public string Plattform { get; set; }
    public string PaperTitle { get; set; }
    public string PaperAuthor { get; set; }
    public string PaperNumber { get; set; }
    public string TimeSpanString { get; set; }
    public TimeSpan Position { get; set; }
    public TimeSpan Duration { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string PreviousState { get; set; }
    public string CurrentState { get; set; }
    #endregion

    #region  Interface Implementations
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <param name="mediaElement"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task InitializeDataAsync(ContentPage contentPage, IMediaElement mediaElement, PaperDto dto)
    {
        string _method = nameof(InitializeDataAsync);
        try
        {
            this.httpClient = ServiceHelper.Current.GetService<HttpClient>();
            this.contentPage = contentPage;
            this.mediaElement = mediaElement;
            await SetPaperDtoAsync(dto);

            var audioStatus = await settingsService.Get("audio_status", "off");
            if (audioStatus.Equals("on"))
            {
                AudioStatus.SetAudioStatus(AudioFlag.AudioStatus.On);
            }
            else
            {
                AudioStatus.SetAudioStatus(AudioFlag.AudioStatus.Off);
            }

            CurrentState = "None";
            PreviousState = "None";
            MediaState = new MediaStatePair() { CurrentState = "None", PreviousState = "None" };
            SendToastState = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsInitializedAsync()
    {
        string _method = nameof(IsInitializedAsync);
        try
        {
            bool isInitialized = false;
            if (ContentPageInitialized && MediaElementInitialized)
            {
                isInitialized = true;
            }
            return isInitialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <returns></returns>
    public async Task SetContentPageAsync(ContentPage contentPage)
    {
        string _method = nameof(SetContentPageAsync);
        try
        {
            this.contentPage = contentPage;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaElement"></param>
    /// <returns></returns>
    public async Task SetMediaElementAsync(IMediaElement mediaElement)
    {
        string _method = nameof(SetMediaElementAsync);
        try
        {
            this.mediaElement = mediaElement;
            MediaElementInitialized = true;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _method = nameof(GetAtAsync);

        try
        {
            var marker = Markers.GetAt(index);
            return marker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task ClearAsync()
    {
        string _method = nameof(ClearAsync);

        try
        {
            Markers.Clear();
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaMarker"></param>
    /// <returns></returns>
    public async Task InsertAsync(AudioMarker mediaMarker)
    {
        string _method = nameof(InsertAsync);

        try
        {
            await InsertAsync(mediaMarker);
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IList<AudioMarker>> ValuesAsync()
    {
        string _method = "ValuesAsync";
        try
        {
            List<AudioMarker> markers = new List<AudioMarker>();
            await Task.Run(() => markers = AudioMarkers.ToList());
            return markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperId"></param>
    /// <returns></returns>
    public async Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId)
    {
        string _method = nameof(LoadAudioMarkersAsync);

        try
        {
            var fileName = paperId.ToString("000") + ".audio.xml";
            var content = await fileService.LoadAsset("AudioMarkers", fileName);
            var xDoc = XDocument.Parse(content);
            var root = xDoc.Root;
            var markers = root.Descendants("Marker");

            List<int> astriskSeqIds = new List<int>();
            bool isAstriskPaper = _astriskDic.TryGetValue(paperId, out astriskSeqIds);
            foreach (var marker in markers)
            {
                int seqId = Int32.Parse(marker.Attribute("seqId").Value);
                if (isAstriskPaper)
                {
                    if (astriskSeqIds.Contains(seqId))
                    {
                        continue;
                    }
                }
                var newMarker = new AudioMarker(marker);
                Markers.Insert(newMarker);
                AudioMarkers.Add(newMarker);
            }
            return Markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IList<AudioMarker>> GetAudioMarkersListAsync()
    {
        string _method = nameof(GetAudioMarkersListAsync);

        try
        {
            List<AudioMarker> values = new();
            if (Markers.Size != 0)
            {
                List<AudioMarker> markers = Markers.Values().ToList();
                foreach (var value in markers)
                {
                    values.Add(value);
                }
            }
            return values;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task DisconnectMediaElementAsync()
    {
        string _method = nameof(DisconnectMediaElementAsync);
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Stop and cleanup MediaElement when we navigate away
                mediaElement.Stop();
                mediaElement.Handler?.DisconnectHandler();
            });
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="markers"></param>
    /// <returns></returns>
    public async Task SetMarkersAsync(AudioMarkerSequence markers)
    {
        string _method = nameof(SetMediaElementAsync);
        try
        {
            this.Markers = markers;
            var audioMarkers = Markers.Values();
            foreach (var marker in audioMarkers)
            {
                this.AudioMarkers.Add(marker);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAudioStatusAsync(bool value)
    {
        string _method = nameof(SetAudioStatusAsync);
        try
        {
            if (value)
            {
                AudioStatus.SetAudioStatus(AudioFlag.AudioStatus.On);
                Preferences.Default.Set("audio-status", "on");
                await settingsService.Set("audio_status", "on");
            }
            else
            {
                AudioStatus.SetAudioStatus(AudioFlag.AudioStatus.Off);
                Preferences.Default.Set("audio_status", "off");
                await settingsService.Set("audio_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetAudioDownloadStatusAsync(bool value)
    {
        string _method = "SetAudioDownloadStatusAsync";
        try
        {
            if (value)
            {
                AudioDownloadStatus = true;
                Preferences.Default.Set("audio_download_status", "on");
                await settingsService.Set("audio_download_status", "on");
            }
            else
            {
                AudioDownloadStatus = false;
                Preferences.Default.Set("audio_download_status", "off");
                await settingsService.Set("audio_download_status", "off");
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="clearSearchBar"></param>
    /// <returns></returns>
    public async Task SetAudioStreamingStatusAsync(bool value)
    {
        string _method = nameof(SetAudioStreamingStatusAsync);
        try
        {
            if (value)
            {
                AudioStreamingStatus = true;
                Preferences.Default.Set("stream_audio", true);
                await settingsService.Set("stream_audio", true);
            }
            else
            {
                AudioStreamingStatus = false;
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> GetAudioStatusAsync()
    {
        string _method = nameof(GetAudioStatusAsync);
        try
        {
            bool _state = false;
            var state = AudioStatus.State;
            switch (state)
            {
                case AudioFlag.AudioStatus.Off:
                    _state = false;
                    break;
                case AudioFlag.AudioStatus.On:
                    _state = true;
                    break;
            }
            return _state;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetMediaPlaybackControlsAsync(bool value)
    {
        string _method = nameof(SetMediaPlaybackControlsAsync);
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                // Stop and cleanup MediaElement when we navigate away
                mediaElement.ShouldShowPlaybackControls = value;
            });
            this.ShowPlaybackControls = value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    public async Task SetPlaybackControlsStartTimeAsync(AudioMarker audioMarker)
    {
        string _method = nameof(SetPlaybackControlsStartTimeAsync);
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(audioMarker.StartTime);
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetShowMediaPlaybackControlsAsync(bool value)
    {
        string _method = nameof(SetShowMediaPlaybackControlsAsync);
        try
        {
            this.ShowPlaybackControls = value;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaStatePair"></param>
    /// <returns></returns>
    public async Task SetMediaStateAsync(MediaStatePair mediaStatePair)
    {
        string _method = nameof(SetMediaStateAsync);
        try
        {
            this.MediaState = mediaStatePair;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<MediaStatePair> GetMediaStateAsync()
    {
        string _method = "GetMediaStateAsync";
        try
        {
            MediaStatePair value = this.MediaState;
            return value;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public async Task SetMediaSourceAsync(string uri)
    {
        string _method = "SetMediaSourceAsync";
        try
        {
            mediaElement.Source = MediaSource.FromUri(uri);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    public async Task SetMediaSourceAsync(string action, string uri)
    {
        string _method = "SetMediaSourceAsync";
        try
        {
            switch (action)
            {
                case "loadOnlineMp3":
                    mediaElement.Source = MediaSource.FromUri(uri);
                    return;
                case "resetSource":
                    mediaElement.Source = null;
                    return;
                case "loadFromLocalFile":
                    mediaElement.Source = MediaSource.FromFile(uri);
                    break;
                case "loadLocalResource":
                    if (DeviceInfo.Platform == DevicePlatform.MacCatalyst
                        || DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.WinUI)
                    {
                        mediaElement.Source = MediaSource.FromResource(uri);
                    }
                    return;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    public async Task SetDurationAsync(TimeSpan duration)
    {
        string _method = nameof(SetDurationAsync);
        try
        {
            this.Duration = duration;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformName"></param>
    /// <returns></returns>
    public async Task SetPlatformAsync(string platformName)
    {
        string _method = nameof(SetPlatformAsync);
        try
        {
            this.Plattform = platformName;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperDto"></param>
    /// <returns></returns>
    public async Task SetPaperDtoAsync(PaperDto paperDto)
    {
        string _method = nameof(SetPaperDtoAsync);
        try
        {
            this.PaperDto = paperDto;
            this.PaperId = paperDto.Id;
            this.PaperNumber = paperDto.Id.ToString("0");
            this.PaperTitle = paperDto.Title;
            this.TimeSpanString = paperDto.TimeSpan;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    public async Task SetParagraphsAsync(List<Paragraph> paragraphs)
    {
        string _method = nameof(SetParagraphsAsync);
        try
        {
            foreach (var paragraph in paragraphs)
            {
                this.Paragraphs.Add(paragraph);
            }
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetSendToastAsync(bool value)
    {
        string _method = nameof(SetSendToastAsync);
        try
        {
            this.SendToastState = value;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    #endregion

    #region  Audio Gestures
    public async Task TappedGestureForPaperAsync(string value)
    {
        string _method = nameof(TappedGestureForPaperAsync);
        try
        {
            if (contentPage == null)
                return;

            string paperTitle = PaperTitle;
            string message = $"Playing {paperTitle}";

            // bool currentState = await audioService.PlayPauseAsync(value)

            //if (CurrentState == "None")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Playing";
            //    message = $"Plyaing {paperTitle}";
            //    await PlayAudio();
            //}
            //else if (CurrentState == "Playing")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Paused";
            //    await PauseAudio();
            //    message = $"Pausing {paperTitle}";
            //}
            //else if (CurrentState == "Paused" || CurrentState == "Stopped")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Playing";
            //    await PlayAudio();
            //    message = $"Resume Playing {paperTitle}";
            //}
            //else
            //{
            //    string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
            //    throw new Exception("Uknown State: " + errorMsg);
            //}

            //using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            //{
            //    ToastDuration duration = ToastDuration.Short;
            //    double fontSize = 14;
            //    var toast = Toast.Make(message, duration, fontSize);
            //    await toast.Show(cancellationTokenSource.Token);
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task DoubleTappedGestureForPaperAsync(string value)
    {
        string _method = nameof(DoubleTappedGestureForPaperAsync);
        try
        {
            if (contentPage == null)
                return;

            string paperTitle = PaperTitle;
            string message = $"Stopping {paperTitle}";

            //if (CurrentState == "None")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Stopped";
            //    message = $"Setting Status Stopped for {paperTitle}";
            //    await StopAudio();
            //}
            //else if (CurrentState == "Playing" || CurrentState == "Paused")
            //{
            //    CurrentState = PreviousState;
            //    CurrentState = "Stopped";
            //    await StopAudio();
            //}
            //else
            //{
            //    string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
            //    throw new Exception("Uknown State: " + errorMsg);
            //}

            //using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            //{
            //    ToastDuration duration = ToastDuration.Short;
            //    double fontSize = 14;
            //    var toast = Toast.Make(message, duration, fontSize);
            //    await toast.Show(cancellationTokenSource.Token);
            //}
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task TappedGestureAsync(string id)
    {
        string _method = nameof(TappedGestureAsync);
        try
        {
            if (contentPage == null)
                return;

            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;

            int paperId = Int32.Parse(id.Substring(1, 3));
            int sequenceId = Int32.Parse(id.Substring(5, 3));
            var audioMarker = AudioMarkers.Where(m => m.SequenceId == sequenceId).FirstOrDefault();

            var startTimeStr = audioMarker.StartTime.ToLongTimeString();
            var endTimeStr = audioMarker.EndTime.ToLongTimeString();
            string timeRange = startTimeStr + " - " + endTimeStr;

            //TimeSpan startTime = new TimeSpan(startTimeStr);
            //TimeSpan endTime = new TimeSpan(endTimeStr);

            Label currentLabel = (Label)contentPage.FindByName(id);
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
            if (CurrentState == "None" &&
                PreviousState == "None")
            {
                PreviousState = "Paused";
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
            }
            // Play State -> Tappeed Event -> 
            // || PreviousState = "Paused"
            else if (CurrentState == "Playing" &&
                     PreviousState == "Paused" ||
                     PreviousState == "None")
            {
                PreviousState = CurrentState;
                CurrentState = "Paused";
                await StateChangedAsync("Paused");
                await PauseAudioAsync();
                message = $"Pausing {pid} Timespan {timeRange}";
            }
            // Playing State -> Play Trigger
            else if (CurrentState == "Paused" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioAsync();
                message = $"Resuming {pid} Timespan {timeRange}";
            }
            // Play State -> Reach Marker Event -> 
            // || PreviousState = "Playing" 
            else if (CurrentState == "Stopped" &&
                     PreviousState == "Playing")
            {
                PreviousState = "Paused";
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
                message = $"Playing {pid} Timespan {timeRange}";
            }
            else
            {
                string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                throw new Exception("Uknown State: " + errorMsg);
            }

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task TappedGestureAsync(string id, bool value)
    {
        string _method = nameof(TappedGestureAsync);
        try
        {
            if (contentPage == null)
                return;

            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;
            SendToastState = value;

            int paperId = Int32.Parse(id.Substring(1, 3));
            int sequenceId = Int32.Parse(id.Substring(5, 3));
            var audioMarker = AudioMarkers.Where(m => m.SequenceId == sequenceId).FirstOrDefault();

            var startTimeStr = audioMarker.StartTime.ToLongTimeString();
            var endTimeStr = audioMarker.EndTime.ToLongTimeString();
            string timeRange = startTimeStr + " - " + endTimeStr;

            //TimeSpan startTime = new TimeSpan(startTimeStr);
            //TimeSpan endTime = new TimeSpan(endTimeStr);

            Label currentLabel = (Label)contentPage.FindByName(id);
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
            if (CurrentState == "None" &&
                PreviousState == "None")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
            }
            // Play State -> Tappeed Event -> 
            // || PreviousState = "Paused" */
            else if (CurrentState == "Playing" &&
                     PreviousState == "None" ||
                     PreviousState == "Paused")
            {
                PreviousState = CurrentState;
                CurrentState = "Paused";
                await StateChangedAsync("Paused");
                await PauseAudioAsync();
                message = $"Pausing {pid} Timespan {timeRange}";
            }
            // Playing State -> Play Trigger
            else if (CurrentState == "Paused" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioAsync();
                message = $"Resuming {pid} Timespan {timeRange}";
            }
            // Play State -> Reach Marker Event -> 
            // || PreviousState = "Playing" */
            else if (CurrentState == "Stopped" &&
                     PreviousState == "Playing")
            {
                PreviousState = CurrentState;
                CurrentState = "Playing";
                await StateChangedAsync("Playing");
                await PlayAudioRangeExAsync(audioMarker);
                message = $"Playing {pid} Timespan {timeRange}";
            }
            else
            {
                string errorMsg = $"Current State = {CurrentState} Previous State = {PreviousState}";
                throw new Exception("Uknown State: " + errorMsg);
            }

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task DoubleTappedGestureAsync(string id)
    {
        string _method = nameof(DoubleTappedGestureAsync);
        try
        {
            if (contentPage == null)
                return;

            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;

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

            await StopAudioAsync();
            await StateChangedAsync("Stopped");
            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task DoubleTappedGestureAsync(string id, bool value)
    {
        string _method = nameof(DoubleTappedGestureAsync);
        try
        {
            if (contentPage == null)
                return;

            CurrentState = MediaState.CurrentState;
            PreviousState = MediaState.PreviousState;
            SendToastState = value;

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

            await StopAudioAsync();

            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    #endregion

    #region   MediaElement Audio Methods
    public async Task<bool> PlayPauseAsync(string value)
    {
        string _method = nameof(PlayPauseAsync);
        try
        {
            bool retval = false;

            string _state = MediaState.CurrentState;
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);
            if (_state == "Playing")
            {

            }
            if (_state == "Paused")
            {

            }
            return retval;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    public async Task PlayAudioAsync()
    {
        string _method = nameof(PlayAudioAsync);
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            //string message = $"Playing {pid} Timespan {timeRange}";
            var paperId = PaperDto.Id;
            var paperTitle = PaperDto.Title;
            var pid = PaperDto.Pid;
            var timeSpan = PaperDto.TimeSpan;

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Play();
                }
            });

            string message = $"Playing Audio";
            if (SendToastState)
            {
                await SendToastAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task PauseAudioAsync()
    {
        string _method = nameof(PauseAudioAsync);
        try
        {
            string _state = "Paused";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Pause();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task StopAudioAsync()
    {
        string _method = nameof(StopAudioAsync);
        try
        {
            string _state = "Stopped";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (mediaElement != null)
                {
                    mediaElement.Stop();
                    mediaElement.MediaEnded();
                }
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task PlayAudioRangeAsync(string timeSpanRange)
    {
        string _method = nameof(PlayAudioRangeAsync);
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            char[] separators = { ':', '.' };
            string[] arry = timeSpanRange.Split('_');
            string[] sa = arry[0].Split(separators, StringSplitOptions.RemoveEmptyEntries);
            string[] ea = arry[1].Split(separators, StringSplitOptions.RemoveEmptyEntries);
            TimeSpan start = new TimeSpan(0, Int32.Parse(sa[0]), Int32.Parse(sa[1]), Int32.Parse(sa[2]), Int32.Parse(sa[3]));
            TimeSpan end = new TimeSpan(0, Int32.Parse(ea[0]), Int32.Parse(ea[1]), Int32.Parse(ea[2]), Int32.Parse(ea[3]));

            StartTime = start;
            EndTime = end;

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(start);
                mediaElement.Play();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task PlayAudioRangeExAsync(AudioMarker audioMarker)
    {
        string _method = nameof(PlayAudioRangeExAsync);
        try
        {
            string _state = "Playing";
            PreviousState = CurrentState;
            CurrentState = _state;
            MediaState.SetState(_state);

            TimeSpan start = audioMarker.StartTime;
            TimeSpan end = audioMarker.EndTime;

            StartTime = start;
            EndTime = end;

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                mediaElement.SeekTo(start);
                mediaElement.Play();
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task PositionChangedAsync(TimeSpan timeSpan)
    {
        string _method = nameof(PositionChangedAsync);
        try
        {
            Position = timeSpan;

            //var me = contentPage.FindByName("mediaElement") as IMediaElement;
            if (EndTime.ToShortTimeString() == timeSpan.ToShortTimeString())
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (mediaElement != null)
                    {
                        mediaElement.Stop();
                        mediaElement.MediaEnded();
                    }
                });
                var currentState = mediaElement.CurrentState.ToString();
                MediaElementMediaState.SetState(currentState);
                MediaState.SetState("Stopped");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task StateChangedAsync(string state)
    {
        string _method = nameof(StateChangedAsync);
        try
        {
            var currentState = state;
            MediaElementMediaState.SetState(state);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    public async Task DownloadAudioFileAsync(string fileName, string audioDir)
    {
        string _method = nameof(DownloadAudioFileAsync);
        try
        {
            // C:\Users\robre\AppData\Local\Packages\879ca98e-d45e-44b3-9be6-e6d900695058_9zz4h110yvjzm\LocalState

            // Setup Uri and File Path
            string uriBasePath = "https://s3.amazonaws.com/urantia/media/en/";
            string uriFullPath = uriBasePath + fileName;
            Uri uri = new Uri(uriFullPath);
            string fileNamePath = Path.Combine(audioDir, "000.mp3");

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(uri);
                var result = response.EnsureSuccessStatusCode();
                if (result.IsSuccessStatusCode)
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    var fileInfo = new FileInfo(fileNamePath);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    throw new Exception("File not found");
                }
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
        }
    }
    public async Task SendToastAsync(string message)
    {
        string _method = nameof(SendToastAsync);
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Cancel");
            return;
        }
    }
    #endregion
}
