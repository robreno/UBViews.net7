using SQLite;

namespace UBViews.SQLiteRepository.Models
{
    [Table("QueryCommand")]
    public class QueryCommand
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string QueryString { get; set; }
        public string ReverseQueryString { get; set; }
    }
}
