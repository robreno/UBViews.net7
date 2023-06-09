namespace UBViews.Models.Query;
public class XmlQueryLocationDto
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<XmlTermOccurrenceDto> TermOccurrences { get; set; } = new();
}
