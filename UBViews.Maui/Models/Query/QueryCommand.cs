namespace UBViews.Models.Query;
public class QueryCommand
{
    public int Id { get; set; }
    public string Type { get; set; }
    public string Terms { get; set; }
    public string Proximity { get; set; }
    public string Stemmed { get; set; }
    public string FilterId { get; set; }
    public string QueryString { get; set; }
}