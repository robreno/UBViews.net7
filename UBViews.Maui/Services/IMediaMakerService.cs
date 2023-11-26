namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.Audio;
public interface IMediaMakerService
{
    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    Task<AudioMarker> GetAt(int index);

    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    Task Clear();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="audioMarker"></param>
    Task Insert(AudioMarker audioMarker);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<IList<AudioMarker>> Values();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<AudioMarkerSequence> LoadAudioMarkers(int paperId);
}
