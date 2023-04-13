using UBViews.Services;
namespace UBViews.Models.Ubml;
public class PartFile : IFile
{
    public int Id { get; set; }
    public string PartId { get; set; }
    public string Heading { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
}
