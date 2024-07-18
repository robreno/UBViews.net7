using SQLite;

namespace UBViews.Repositories.Models;

[Table("PostingLists")]
public class PostingList    
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Lexeme { get; set; }
}
