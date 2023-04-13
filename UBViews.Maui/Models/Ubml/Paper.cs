using System.Collections.ObjectModel;

namespace UBViews.Models.Ubml;

public class Paper
{
    public int Id { get; set; }
    public string PartId { get; set; }
    public string PartTitle { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string Author { get; set; }
    public string Title { get; set; }
    public ObservableCollection<Paragraph> Paragraphs { get; set; } = new ObservableCollection<Paragraph>();
}
