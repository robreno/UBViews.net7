using UBViews.Services;

namespace UBViews.Models;

public class ImageFile : IFile
{
    public int Id { get; set; }
    public string GroupId { get; set; }
    public string Path { get; set; }
    public string ImageName { get; set; }
    public string FileName { get; set; }
    public string Theme { get; set; }
    public string Title { get; set; }
}
