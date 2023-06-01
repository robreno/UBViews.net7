namespace UBViews.SQLiteRepository.Dtos;
public class QueryLocation
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<TermLocation> TermOccurrences { get; set; } = new();
}
