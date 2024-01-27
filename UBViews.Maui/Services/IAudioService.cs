namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;

public interface IAudioService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <param name="mediaElement"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task InitializeDataAsync(ContentPage contentPage, IMediaElement mediaElement, PaperDto dto);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> IsInitializedAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <returns></returns>
    Task SetContentPageAsync(ContentPage contentPage);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaElement"></param>
    /// <returns></returns>
    Task SetMediaElementAsync(IMediaElement mediaElement);

    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Task<AudioMarker> GetAtAsync(int index);

    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    Task ClearAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    Task InsertAsync(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IList<AudioMarker>> ValuesAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IList<AudioMarker>> GetAudioMarkersListAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task DisconnectMediaElementAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="markers"></param>
    /// <returns></returns>
    Task SetMarkersAsync(AudioMarkerSequence markers);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetAudioStatusAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetAudioDownloadStatusAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetAudioStreamingStatusAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> GetAudioStatusAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetMediaPlaybackControlsAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    Task SetPlaybackControlsStartTimeAsync(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetShowMediaPlaybackControlsAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaStatePair"></param>
    /// <returns></returns>
    Task SetMediaStateAsync(MediaStatePair mediaStatePair);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<MediaStatePair> GetMediaStateAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task SetMediaSourceAsync(string uri);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    Task SetMediaSourceAsync(string action, string uri);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task SetDurationAsync(TimeSpan duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformName"></param>
    /// <returns></returns>
    Task SetPlatformAsync(string platformName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperDto"></param>
    /// <returns></returns>
    Task SetPaperDtoAsync(PaperDto paperDto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    Task SetParagraphsAsync(List<Paragraph> paragraphs);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetSendToastAsync(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task TappedGestureForPaperAsync(string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task DoubleTappedGestureForPaperAsync(string value);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task TappedGestureAsync(string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="sendToast"></param>
    /// <returns></returns>
    Task TappedGestureAsync(string value, bool sendToast);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DoubleTappedGestureAsync(string id);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<bool> PlayPauseAsync(string value);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task PlayAudioAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task PauseAudioAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task StopAudioAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpanRange"></param>
    /// <returns></returns>
    Task PlayAudioRangeAsync(string timeSpanRange);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    Task PlayAudioRangeExAsync(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    Task PositionChangedAsync(TimeSpan timeSpan);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    Task StateChangedAsync(string state);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    Task DownloadAudioFileAsync(string fileName, string audioDir);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendToastAsync(string message);
}
