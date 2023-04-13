namespace UBViews.Models.Query;
public class QueryLocation
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<TermOccurence> TermOccurrences { get; set; }
}
