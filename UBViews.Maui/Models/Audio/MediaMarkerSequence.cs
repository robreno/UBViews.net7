using System.Collections.Generic;
using System.Xml.Linq;

namespace UBViews.Models.Audio;

/// <summary>
/// An ordered collection of PlaybackMediaMarker objects.
/// </summary>
sealed class MediaMarkerSequence //: IEnumerable<PlaybackMediaMarker>, IEnumerable, IEquatable<PlaybackMediaMarkerSequence>
{
    #region  Private Data
    /// <summary>
    /// 
    /// </summary>
    private readonly string _class = "MediaMarkerSequence";

    /// <summary>
    /// SortedList of MedidaMarkers
    /// </summary>
    SortedList<int, MediaMarker> markers = new();

    /// <summary>
    /// Returns the number of items in the sequence.
    /// </summary>
    public int Size => markers.Count;
    #endregion

    #region  Public Methods
    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    public void Clear()
    {
        string _method = "Clear";
        try
        {
            markers.Clear();
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_class}.{_method} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task ClearAsync()
    {
        string _method = "ClearAsync";
        try
        {
            markers.Clear();
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// Insert
    /// </summary>
    /// <param name="mediaMarker"></param>
    public void Insert(MediaMarker mediaMarker)
    {
        string _method = "Insert";
        try
        {
            markers.Add(mediaMarker.SequenceId, mediaMarker);
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_class}.{_method} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaMarker"></param>
    /// <returns></returns>
    public async Task InsertAsync(MediaMarker mediaMarker)
    {
        string _method = "InsertAsync";
        try
        {
            markers.Add(mediaMarker.SequenceId, mediaMarker);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// Iterator pointing at the first media marker in the sequence.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<KeyValuePair<TimeSpan, MediaMarker>> First()
    {
        string _method = "First";
        try
        {
            return (IEnumerable<KeyValuePair<TimeSpan, MediaMarker>>)markers.GetEnumerator();
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_class}.{_method} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<KeyValuePair<TimeSpan, MediaMarker>>> FirstAsync()
    {
        string _method = "FirstAsync";
        try
        {
            return (IEnumerable<KeyValuePair<TimeSpan, MediaMarker>>)markers.GetEnumerator();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
}
