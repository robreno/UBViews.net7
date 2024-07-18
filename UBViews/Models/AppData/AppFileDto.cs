namespace UBViews.Models.AppData;
public class AppFileDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
    public string Path { get; set; }
    public long Length { get; set; }
    public string Size { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
}
