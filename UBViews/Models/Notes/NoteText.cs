namespace UBViews.Models.Notes;

using System.Text;

public class NoteText
{
    public string Style { get; set; }
    public string Text { get; set; }
    public List<NoteRun> NoteRuns { get; set; } = new();
}
