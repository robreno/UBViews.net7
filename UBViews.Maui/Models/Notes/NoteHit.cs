namespace UBViews.Models.Notes;

using UBViews.Models.Ubml;

public class NoteHit
{
    public string Id { get; set; }
    public string Subject { get; set; }
    public int Hit { get; set; } = 0;
    public int PaperId { get; set; } = 0;
    public int SequenceId { get; set; } = 0;
    public string LocationId { get; set; } = null;
    public string Pid { get; set; }
    public bool Selected { get; set; }
    public bool IsDirty { get; set; }
}
