namespace UBViews.Models.AppData;
public class QueryCommandDto
{
    public int Id { get; set; }
    public int Hits { get; set; }
    public string Type { get; set; }
    public string Terms { get; set; }
    public string Proximity { get; set; }
    public string Stemmed { get; set; }
    public string FilterId { get; set; }
    public string QueryString { get; set; }
}