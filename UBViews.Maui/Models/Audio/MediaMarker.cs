using System.Xml.Linq;
using System.Xml.XPath;

namespace UBViews.Models.Audio;

/// <summary>
/// PlaybackMediaMarker class.
/// </summary>
public sealed class MediaMarker
{
    /// <summary>
    /// MarkerType enum
    /// </summary>
    enum MarkerType { Section, Paragraph }

    /// <summary>
    /// PlaybackMediaMarker class Cstor
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="type"></param>
    /// <param name="paragraphId"></param>
    public MediaMarker(int sequenceId, TimeSpan startValue, TimeSpan endValue, string type, string paragraphId)
    {
        SequenceId = sequenceId;
        StartTime = startValue;
        EndTime = endValue;
        Type = type;
        ParagraphId = paragraphId;
    }

    public MediaMarker(XElement element)
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

    /// <summary>
    /// Create a PlaybackMediaMarker from TimeSpan with metadata
    /// </summary>
    /// <param name="sequenceId"></param>
    /// <param name="startValue"></param>
    /// <param name="endValue"></param>
    /// <param name="type"></param>
    /// <param name="paragraphId"></param>
    /// <returns></returns>
    public MediaMarker Create(int sequenceId, TimeSpan startValue, TimeSpan endValue, string type, string paragraphId)
    {
        SequenceId = sequenceId;
        StartTime = startValue;
        EndTime = endValue;
        Type = type;
        ParagraphId = paragraphId;
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sa"></param>
    /// <returns></returns>
    internal TimeSpan TimespanFromArray(string[] sa)
    {
        TimeSpan newTimeSpan = new TimeSpan(0,
            int.Parse(sa[0]), int.Parse(sa[1]), int.Parse(sa[2]), int.Parse(sa[3]));
        return newTimeSpan;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pid"></param>
    internal string TypeFromPID(string pid)
    {
        string[] arry = pid.Split(new char[] { ':', '.' }, StringSplitOptions.RemoveEmptyEntries);
        int type = int.Parse(arry[2]);
        string str = string.Empty;
        switch (type)
        {
            case (int)MarkerType.Section:
                str = "Section";
                break;
            case (int)MarkerType.Paragraph:
                str = "Paragraph";
                break;
        }
        return str;
    }

    /// <summary>
    /// Gets the type of the media marker.
    /// </summary>
    public string Type { get; private set; }

    /// <summary>
    /// Gets the PargraphId associated with the marker.
    /// </summary>
    public string ParagraphId { get; private set; }

    /// <summary>
    /// Gets the SequenceId associated with the marker.
    /// </summary>
    public int SequenceId { get; private set; }

    /// <summary>
    /// Gets the offset in the media timeline where the marker occurs.
    /// </summary>
    public TimeSpan StartTime { get; private set; }

    /// <summary>
    /// Gets the offset in the media timeline where the marker occurs.
    /// </summary>
    public TimeSpan EndTime { get; private set; }
}
