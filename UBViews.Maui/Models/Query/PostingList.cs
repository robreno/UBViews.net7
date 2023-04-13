namespace UBViews.Models.Query;
public class PostingList
{
    public int TokenID { get; set; }
    public string StableID { get; set; }
    public string Lexeme { get; set; }
    public List<TermOccurence> TermOccurrences { get; set; }
}
