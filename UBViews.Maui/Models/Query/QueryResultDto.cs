namespace UBViews.Models.Query;
public class QueryResultDto
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Terms { get; set; }
    public string Proximity { get; set; }
    public string QueryString { get; set; }
    public string QueryExpression { get; set; }
}
