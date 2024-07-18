namespace UBViews.Models.AppData;
public class QueryResultLocationsDto
{
    public int Id { get; set; }
    public int Hits { get; set; }
    public string Type { get; set; }
    public string Terms { get; set; }
    public string Proximity { get; set; }
    public string QueryString { get; set; }
    public string QueryExpression { get; set; }
    public List<QueryLocationDto> QueryLocations { get; set; } = new();
}
