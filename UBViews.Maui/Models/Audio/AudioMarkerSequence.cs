using System.Collections.Generic;
using System.Xml.Linq;

namespace UBViews.Models.Audio;

/// <summary>
/// An ordered collection of PlaybackMediaMarker objects.
/// </summary>
sealed class AudioMarkerSequence
{
    /// <summary>
    /// SortedList of MedidaMarkers
    /// </summary>
    SortedList<int, AudioMarker> markers = new();

    /// <summary>
    /// Returns the number of items in the sequence.
    /// </summary>
    public int Size => markers.Count;

    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AudioMarker GetAt(int index)
    {
        return markers[index];
    }

    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    public void Clear()
    {
        markers.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaMarker"></param>
    public void Insert(AudioMarker mediaMarker)
    {
        markers.Add(Size, mediaMarker);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IList<AudioMarker> Values()
    {
        return markers.Values;
    }
}

