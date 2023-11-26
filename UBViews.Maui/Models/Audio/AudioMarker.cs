namespace UBViews.Models.Audio;

using System.Xml.Linq;
using System.Xml.XPath;

/// <summary>
/// AudioMarker class.
/// </summary>
public class AudioMarker
{
    #region    Private Data Members
    /// <summary>
    /// 
    /// </summary>
    private readonly string _className = "AudioMarker";

    /// <summary>
    /// 
    /// </summary>
    enum MarkerType { Title, Section, Paragraph }
    #endregion

    #region   Constructors
    public AudioMarker() { }

    /// <summary>
    /// AudioMarker Cstor
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="type"></param>
    /// <param name="paragraphId"></param>
    public AudioMarker(int sequenceId, TimeSpan startValue, TimeSpan endValue, string type, string paragraphId)
    {
        SequenceId = sequenceId;
        StartTime = startValue;
        EndTime = endValue;
        Type = type;
        ParagraphId = paragraphId;
    }

    /// <summary>
    /// AudioMarker CStor
    /// </summary>
    /// <param name="element"></param>
    public AudioMarker(XElement element)
    {
        char[] separators = { ':', '.' };
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        string pid = element.Attribute("pid").Value;
        string type = TypeFromPID(pid);
        string sid = element.Attribute("seqId").Value;
        string start = element.XPathSelectElement("Start").Value;
        string end = element.XPathSelectElement("End").Value;
        string[] startArry = start.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        string[] endArry = end.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        TimeSpan startTime = TimespanFromArray(startArry);
        TimeSpan endTime = TimespanFromArray(endArry);
        SequenceId = int.Parse(sid);
        StartTime = startTime;
        EndTime = endTime;
        Type = type;
        ParagraphId = pid;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
    }
    #endregion

    #region   Internal Methods
    /// <summary>
    /// 
    /// </summary>
    /// <param name="sa"></param>
    /// <returns></returns>
    internal TimeSpan TimespanFromArray(string[] sa)
    {
        string _methodName = "TimespanFromArray";
        try
        {
            TimeSpan newTimeSpan = new TimeSpan(0,
            int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
            return newTimeSpan;
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
    /// <param name="pid"></param>
    internal string TypeFromPID(string pid)
    {
        string _methodName = "TimespanFromArray";
        try
        {
            string[] arry = pid.Split(new char[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
            int secId = int.Parse(arry[1]);
            int parId = int.Parse(arry[2]);

            int type = -1;
            if (secId == 0 && parId == 0)
            {
                type = 0;
            }
            else if (secId > 0 && parId == 0)
            {
                type = 1;
            }
            else
            {
                type = 2;
            }

            string str = string.Empty;
            switch (type)
            {
                case (int)MarkerType.Title:
                    str = "Title";
                    break;
                case (int)MarkerType.Section:
                    str = "Section";
                    break;
                case (int)MarkerType.Paragraph:
                    str = "Paragraph";
                    break;
            }
            return str;
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }
    #endregion

    #region  Public Methods
    /// <summary>
    /// Create a AudioMarker from TimeSpan with metadata
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="type"></param>
    /// <param name="paragraphId"></param>
    /// <returns></returns>
    public AudioMarker Create(int sequenceId, TimeSpan startValue, TimeSpan endValue, string type, string paragraphId)
    {
        string _methodName = "Create";
        try
        {
            SequenceId = sequenceId;
            StartTime = startValue;
            EndTime = endValue;
            Type = type;
            ParagraphId = paragraphId;
            return this;
        }
        catch (Exception ex)
        {
            string innerMessage = $"Exception raised in {_className}.{_methodName} => {ex.Message}";
            throw new Exception(innerMessage);
        }
    }

    /// <summary>
    /// CreateAsync
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="type"></param>
    /// <param name="paragraphId"></param>
    /// <returns></returns>
    public async Task<AudioMarker> CreateAsync(int sequenceId, TimeSpan startValue, TimeSpan endValue, string type, string paragraphId)
    {
        string _methodName = "CreateAsync";
        try
        {
            SequenceId = sequenceId;
            StartTime = startValue;
            EndTime = endValue;
            Type = type;
            ParagraphId = paragraphId;
            return this;
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
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public AudioMarker GetNewAudioMarker(TimeSpan start, TimeSpan end)
    {
        string _methodName = "GetNewAudioMarker";
        try
        {
            AudioMarker newMarker = new AudioMarker()
            {
                SequenceId = this.SequenceId,
                ParagraphId = this.ParagraphId,
                Type = this.Type,
                StartTime = start,
                EndTime = end
            };
            return newMarker;
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
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    public async Task<AudioMarker> GetNewAudioMarkerAsync(TimeSpan start, TimeSpan end)
    {
        string _methodName = "GetNewAudioMarkerAsync";
        try
        {
            AudioMarker newMarker = new AudioMarker()
            {
                SequenceId = this.SequenceId,
                ParagraphId = this.ParagraphId,
                Type = this.Type,
                StartTime = start,
                EndTime = end
            };
            return newMarker;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return null;
        }
    }
    #endregion

    #region  Public Properties
    /// <summary>
    /// Gets the type of the media marker.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets the PargraphId associated with the marker.
    /// </summary>
    public string ParagraphId { get; set; }

    /// <summary>
    /// Gets the SequenceId associated with the marker.
    /// </summary>
    public int SequenceId { get; set; }

    /// <summary>
    /// Gets the offset in the media timeline where the marker occurs.
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Gets the offset in the media timeline where the marker occurs.
    /// </summary>
    public TimeSpan EndTime { get; set; }
    #endregion
}

