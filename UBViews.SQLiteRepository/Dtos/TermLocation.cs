namespace UBViews.SQLiteRepository.Dtos;
public class TermLocation
{
    public string Term { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int DocumentPosition { get; set; }
    public int TextPosition { get; set; }
    public int Length { get; set; }
}
