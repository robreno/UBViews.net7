namespace UBViews.Models.Ubml;

using System.Text;

using UBViews.Models.Notes;

public class Paragraph
{
    private string endTime;
    public int PaperId { get; set; }
    public int SeqId { get; set; }
    public string PaperSeqId { get; set; }
    public string Uid { get; set; }
    public string Pid { get; set; }
    public string Type { get; set; }
    public string ParaStyle { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get => endTime; set => endTime = value; }
    public string Text { get; set; }
    public List<Run> Runs { get; set; } = new();
    public List<NoteEntry> Notes { get; set; } = new();
    public string CreatePlainTextBody()
    {
        string _body = string.Empty;
        List<Run> runsList = new();
        Run pidRun = new Run { Style = "{StaticResource PID}", Text = Pid };
        Run spcRun = new Run { Style = "{StaticResource RegularSpaceSpan}", Text = " " };
        runsList.Add(pidRun);
        runsList.Add(spcRun);
        foreach(Run run in Runs)
        {
            runsList.Add(run);
        }

        StringBuilder sb = new StringBuilder();
        var txt = string.Empty;
        foreach (Run run in runsList)
        {
            var runStyle = run.Style;
            var runText = run.Text;
            switch (runStyle)
            {
                case "{StaticResource PID}":
                    sb.Append(runText);
                    break;
                case "{StaticResource RegularSpaceSpan}":
                    sb.Append(runText);
                    break;
                case "{StaticResource OpeningSpan}":
                case "{StaticResource SmallCapsSpan}":
                case "{StaticResource AllSmallCapsSpan}":
                    txt = runText.ToUpper();
                    sb.Append(txt);
                    break;
                case "{StaticResource ItalicSpan}":
                    txt = "_" + runText + "_";
                    sb.Append(txt);
                    break;
                case "{StaticResource SmallCapsItalicSpan}":
                case "{StaticResource AllSmallCapsItalicSpan}":
                    txt = runText.ToUpper();
                    txt = "_" + txt + "_";
                    sb.Append(txt);
                    break;
                // Default to RegularSpan
                default:
                    sb.Append(runText);
                    break;
            }
        }
        _body = sb.ToString();
        return _body;
    }
    public string CreateHtmlTextBody()
    {
        string _body = string.Empty;
        List<Run> runsList = new();
        Run pidRun = new Run { Style = "{StaticResource PID}", Text = Pid };
        Run spcRun = new Run { Style = "{StaticResource RegularSpaceSpan}", Text = " " };
        runsList.Add(pidRun);
        runsList.Add(spcRun);
        foreach (Run run in Runs)
        {
            runsList.Add(run);
        }

        StringBuilder sb = new StringBuilder();
        var txt = string.Empty;
        foreach (Run run in runsList)
        {
            var runStyle = run.Style;
            var runText = run.Text;
            switch (runStyle)
            {
                case "{StaticResource PID}":
                    sb.Append(runText);
                    break;
                case "{StaticResource RegularSpaceSpan}":
                    sb.Append(runText);
                    break;
                case "{StaticResource OpeningSpan}":
                case "{StaticResource SmallCapsSpan}":
                case "{StaticResource AllSmallCapsSpan}":
                    txt = runText.ToUpper();
                    sb.Append(txt);
                    break;
                case "{StaticResource ItalicSpan}":
                    txt = "<i>" + runText + "</i>";
                    sb.Append(txt);
                    break;
                case "{StaticResource SmallCapsItalicSpan}":
                case "{StaticResource AllSmallCapsItalicSpan}":
                    txt = runText.ToUpper();
                    txt = "<i>" + txt + "</i>";
                    break;
                // Default to RegularSpan
                default:
                    sb.Append(runText);
                    break;
            }
        }
        _body = sb.ToString();
        return _body;
    }
    public List<Run> GetQuoteRuns()
    {
        string _body = string.Empty;
        List<Run> runsList = new();
        Run pidRun = new Run { Style = "{StaticResource PID}", Text = Pid };
        Run spcRun = new Run { Style = "{StaticResource RegularSpaceSpan}", Text = " " };
        runsList.Add(pidRun);
        runsList.Add(spcRun);
        foreach (Run run in Runs)
        {
            runsList.Add(run);
        }
        return runsList;
    }
}
