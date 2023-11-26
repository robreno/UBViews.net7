namespace UBViews.Models.Audio;

using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// An ordered collection of PlaybackMediaMarker objects.
/// </summary>
public class AudioMarkerSequence
{
    #region  Private Data
    private readonly string _className = "AudioMarkerSequence";

    /// <summary>
    /// SortedList of MedidaMarkers
    /// </summary>
    SortedList<int, AudioMarker> markers = new();

    /// <summary>
    /// Returns the number of items in the sequence.
    /// </summary>
    public int Size => markers.Count;
    #endregion

    #region  Public Methods
    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public AudioMarker GetAt(int index)
    {
        string _methodName = "GetAt";

        try
        {
            return markers[index];
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// Returns the number of items in the sequence.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _methodName = "GetAtAsync";

        try
        {
            return markers[index];
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seqId"></param>
    /// <returns></returns>
    public AudioMarker GetBySeqId(int seqId)
    {
        string _methodName = "GetBySeqId";

        try
        {
            if (seqId < 1)
            {
                return null;
            }

            int index = seqId - 1;
            return markers[index];
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// GetBySeqIdAsync
    /// </summary>
    /// <param name="seqId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<AudioMarker> GetBySeqIdAsync(int seqId)
    {
        string _methodName = "GetBySeqIdAsync";

        try
        {
            if (seqId < 1)
            {
                return null;
            }

            int index = seqId - 1;
            return markers[index];
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// Clear Removes all elements from the sequence.
    /// </summary>
    public void Clear()
    {
        string _methodName = "Clear";

        try
        {
            markers.Clear();
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// ClearAsync
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task ClearAsync()
    {
        string _methodName = "ClearAsync";

        try
        {
            markers.Clear();
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// Insert ...
    /// </summary>
    /// <param name="mediaMarker"></param>
    public void Insert(AudioMarker mediaMarker)
    {
        string _methodName = "Insert";

        try
        {
            markers.Add(Size, mediaMarker);
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mediaMarker"></param>
    /// <returns></returns>
    public async Task InsertAsync(AudioMarker mediaMarker)
    {
        string _methodName = "InsertAsync";

        try
        {
            markers.Add(Size, mediaMarker);
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public IList<AudioMarker> Values()
    {
        string _methodName = "Values";

        try
        {
            return markers.Values;
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<IList<AudioMarker>> ValuesAsync()
    {
        string _methodName = "ValuesAsync";

        try
        {
            return markers.Values;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
} 
