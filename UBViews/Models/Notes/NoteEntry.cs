namespace UBViews.Models.Notes;

public class NoteEntry
{
    public int Id { get; set; }
    public string Type { get; set; }
    public int PaperId { get; set; }
    public int SequenceId { set; get; }
    public string LocationId { get; set; }
    public string Pid { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime DateEdited { get; set; }
    public string Author { get; set; }
    public string Subject { get; set; }
    public string Text { get; set; }
    public string Style { get; set; }
    public List<NoteRun> NoteRuns { get; set; } = new();
    public bool EqualTo(NoteEntry dto)
    {
        bool isEqual = false;
        if (dto == null) { return false; }
        else
        {
            if (dto.Id == this.Id &&
                dto.Type == this.Type &&
                dto.PaperId == this.PaperId &&
                dto.SequenceId == this.SequenceId &&
                dto.LocationId == this.LocationId &&
                dto.Pid == this.Pid &&
                dto.DateCreated == this.DateCreated &&
                dto.DateEdited == this.DateEdited &&
                dto.Author == this.Author &&
                dto.Subject == this.Subject &&
                dto.Text == this.Text &&
                dto.Style == this.Style &&
                dto.NoteRuns == this.NoteRuns)
            {
                isEqual = true;
            }
        }
        return isEqual;
    }
}
