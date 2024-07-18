namespace UBViews.Models.Query;
public class TermLocationDto
{
    public int Id { get; set; }
    public int QueryResultId { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int DocumentPosition { get; set; }
    public int TextPosition { get; set; }
    public int TextLength { get; set; }
    public string Term { get; set; }
    public string ParagraphId { get; set; }
}
