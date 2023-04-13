namespace UBViews.Models;
public class PaperDto
{
    public int Id { get; set; }
    public bool ScrollTo { get; set; }
    public string Uid { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int PartId { get; set; }
    public string PartTitle { get; set; }
    public string TimeSpan { get; set; }
}
