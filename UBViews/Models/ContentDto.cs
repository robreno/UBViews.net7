namespace UBViews.Models;
public class ContentDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public List<SectionTitleDto> SectionTitles { get; set; } = new();
}
