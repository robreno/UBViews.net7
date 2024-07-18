using SQLite;

namespace UBViews.Repositories.Models;

[Table("TokenStems")]
public class TokenStem
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Lexeme { get; set; }
    public string Stemmed { get; set; }
}
