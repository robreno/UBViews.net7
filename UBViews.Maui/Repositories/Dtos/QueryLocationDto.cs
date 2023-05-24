namespace UBViews.Repositories.Dtos;
public class QueryLocationDto
{
    public string Id { get; set; }
    public string Pid { get; set; }
    public List<TermOccurenceDto> TermOccurrences { get; set; } = new();
}
