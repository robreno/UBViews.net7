namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models;
using UBViews.Models.Ubml;
using UBViews.Models.Audio;

public interface IAudioService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="contentPage"></param>
    /// <returns></returns>
    Task SetContentPage(ContentPage contentPage);

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
    Task<IList<AudioMarker>> GetAudioMarkersListAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId);

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
    /// <returns></returns>
    Task<bool> GetAudioStatus();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetMediaPlaybackControls(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    Task SetPlaybackControlsStartTime(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetShowMediaPlaybackControls(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="duration"></param>
    /// <returns></returns>
    Task SetDuration(TimeSpan duration);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="platformName"></param>
    /// <returns></returns>
    Task SetPlatform(string platformName);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paperDto"></param>
    /// <returns></returns>
    Task SetPaperDto(PaperDto paperDto);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    Task SetParagraphs(List<Paragraph> paragraphs);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task SetSendToast(bool value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task TappedGestureForPaper(string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task DoubleTappedGestureForPaper(string value);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    Task TappedGesture(string value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="sendToast"></param>
    /// <returns></returns>
    Task TappedGesture(string value, bool sendToast);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    Task DoubleTappedGesture(string id);

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
    Task PlayAudio();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task PauseAudio();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task StopAudio();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpanRange"></param>
    /// <returns></returns>
    Task PlayAudioRange(string timeSpanRange);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    /// <returns></returns>
    Task PlayAudioRangeEx(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="timeSpan"></param>
    /// <returns></returns>
    Task PositionChanged(TimeSpan timeSpan);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    Task StateChanged(string state);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task SendToast(string message);
}
