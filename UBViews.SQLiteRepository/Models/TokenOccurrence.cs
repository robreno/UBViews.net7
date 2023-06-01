using SQLite;

namespace UBViews.SQLiteRepository.Models
{
    [Table("TokenOccurrences")]
    public class TokenOccurrence
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PostingId { get; set; }
        public int DocumentId { get; set; }
        public int SequenceId { get; set; }
        public int DocumentPosition { get; set; } // TODO: regen db, as spelling error on field in database
        public int TextPosition { get; set; }
    }
}
