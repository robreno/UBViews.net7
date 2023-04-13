using System.Collections.Generic;
using System.Xml.Linq;

namespace UBViews.Models.Audio;

/// <summary>
/// An ordered collection of PlaybackMediaMarker objects.
/// </summary>
sealed class MediaMarkerSequence //: IEnumerable<PlaybackMediaMarker>, IEnumerable, IEquatable<PlaybackMediaMarkerSequence>
{
    /// <summary>
    /// SortedList of MedidaMarkers
    /// </summary>
    SortedList<int, MediaMarker> markers = new();

    /// <summary>
    /// Returns the number of items in the sequence.
    /// </summary>
    public int Size => markers.Count;

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
    public void Insert(MediaMarker mediaMarker)
    {
        markers.Add(mediaMarker.SequenceId, mediaMarker);
    }

    /// <summary>
    /// Iterator pointing at the first media marker in the sequence.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<TimeSpan, MediaMarker>> First()
    {
        return (IEnumerable<KeyValuePair<TimeSpan, MediaMarker>>)markers.GetEnumerator();
    }
}
