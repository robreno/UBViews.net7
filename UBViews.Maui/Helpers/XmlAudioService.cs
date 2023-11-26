namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using UBViews.Services;

using UBViews.Models.Audio;

public partial class XmlAudioService : IAudioService
{
    #region  Private Data
    /// <summary>
    /// 
    /// </summary>
    private IFileService fileService;

    private Dictionary<int, List<int>> _astriskDic = new Dictionary<int, List<int>>()
    {
        { 31, new List<int> { 92 } },
        { 56, new List<int> { 92 } },
        { 120, new List<int> { 41 } },
        { 134, new List<int> { 70 } },
        { 196, new List<int> { 78 } },
        { 144, new List<int> { 70, 84, 97, 113, 132, 146 } }
    };

    private readonly string _className = "XmlAudioService";

    //Dictionary<string, string> _tildes = new Dictionary<string, string>(
    //    {
    //        { "", ""},
    //    });

    internal AudioMarkerSequence audioMarkerSequence { get; set; } = new();
    #endregion

    #region  Constructor
    public XmlAudioService(IFileService fileService) 
    {
        this.fileService = fileService;
    }
    #endregion

    #region  Public Methods
    /// <summary>
    /// Get MediaMarker at index position.
    /// </summary>
    /// <param name="index"></param>
    /// <returns>AudioMarker at index or null</returns>
    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _methodName = "GetAtAsync";

        try
        {
            AudioMarker marker;
            if (audioMarkerSequence.Size == 0)
                return null;

            marker = audioMarkerSequence.GetAt(index);

            return marker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// Removes all elements from the sequence.
    /// </summary>
    public async Task ClearAsync()
    {
        string _methodName = "ClearAsync";

        try
        {
            audioMarkerSequence.Clear();
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
    /// <param name="audioMarker"></param>
    public async Task InsertAsync(AudioMarker audioMarker)
    {
        string _methodName = "InsertAsync";

        try
        {
            audioMarkerSequence.Insert(audioMarker);
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
    public async Task<IList<AudioMarker>> GetAudioMarkersListAsync()
    {
        string _methodName = "GetAudioMarkersListAsync";

        try
        {
            List<AudioMarker> values = new();
            if (audioMarkerSequence.Size != 0)
            {
                List<AudioMarker> markers = audioMarkerSequence.Values().ToList();
                foreach (var value in markers)
                {
                    values.Add(value);
                }
            }
            return values;
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
    /// <returns></returns>
    public async Task<AudioMarkerSequence> LoadAudioMarkersAsync(int paperId)
    {
        string _methodName = "LoadAudioMarkersAsync";

        try
        {
            List<int> astriskSeqIds = new List<int>();
            bool isAstriskPaper = _astriskDic.TryGetValue(paperId, out astriskSeqIds);
            var fileName = paperId.ToString("000") + ".audio.xml";
            var content = await fileService.LoadAsset("AudioMarkers", fileName);
            var xDoc = XDocument.Parse(content);
            var root = xDoc.Root;
            var markers = root.Descendants("Marker");
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
                audioMarkerSequence.Insert(newMarker);
            }
            return audioMarkerSequence;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
}
