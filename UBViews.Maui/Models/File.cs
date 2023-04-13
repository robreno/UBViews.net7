using UBViews.Services;

namespace LexParser.Models;

public class File : IFile
{
    public int Id { get; set; }
    public string Path { get; set; }
    public string FileName { get; set; }
}
