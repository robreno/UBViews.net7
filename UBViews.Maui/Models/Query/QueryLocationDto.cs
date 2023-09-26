namespace UBViews.Models.Query;
public class QueryLocationDto
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<TermLocationDto> TermOccurrences { get; set; } = new();
}
