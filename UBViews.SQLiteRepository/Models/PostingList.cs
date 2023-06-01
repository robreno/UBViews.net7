using SQLite;

namespace UBViews.SQLiteRepository.Models
{
    [Table("PostingLists")]
    public class PostingList    
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Lexeme { get; set; }
    }
}
