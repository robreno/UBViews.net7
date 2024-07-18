namespace UBViews.Models.Query;

public class PostingListOccurrencesDto
{
    public int Id { get; set; }
    public string Lexeme { get; set; }
    public List<TokenOccurrenceDto> TokenOccurrences { get; set; } = new();
}
