namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UBViews.Services;

using UBViews.Models.Audio;

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

    private readonly string _className = "AudioService";
    internal AudioMarkerSequence Markers { get; set; } = new();
    #endregion

    IFileService fileService;

    public AudioService(IFileService fileService)
    {
        this.fileService = fileService;
    }

    #region  Public Methods
    public async Task<AudioMarker> GetAtAsync(int index)
    {
        string _methodName = "GetAtAsync";

        try
        {
            var marker = Markers.GetAt(index);
            return marker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
    public async Task ClearAsync()
    {
        string _methodName = "ClearAsync";

        try
        {
            Markers.Clear();
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task InsertAsync(AudioMarker mediaMarker)
    {
        string _methodName = "InsertAsync";

        try
        {
            await InsertAsync(mediaMarker);
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }
    public async Task<IList<AudioMarker>> GetAudioMarkersListAsync()
    {
        string _methodName = "GetAudioMarkersListAsync";

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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
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
                Markers.Insert(newMarker);
            }
            return Markers;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion
}
