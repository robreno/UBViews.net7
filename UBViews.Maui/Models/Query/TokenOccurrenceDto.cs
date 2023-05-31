namespace UBViews.Models.Query;
public class TokenOccurrenceDto
{
    public int PostingId { get; set; }
    public int DocumentId { get; set; }
    public int SequenceId { get; set; }
    public int DocumentPosition { get; set; } // TODO: regen db, as spelling error on field in database DocummentPosition
    public int TextPosition { get; set; }
}
