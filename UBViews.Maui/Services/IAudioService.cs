namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.Audio;

public interface IAudioService
{
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
}
