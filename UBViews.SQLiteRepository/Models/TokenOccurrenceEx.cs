using SQLite;

namespace UBViews.SQLiteRepository.Models
{
    [Table("TokenOccurrences")]
    public class TokenOccurrenceEx
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int PostingId { get; set; }
        public int DocumentId { get; set; }
        public int SequenceId { get; set; }
        public int SectionId { get; set; }
        public int DocumentPosition { get; set; }
        public int TextPosition { get; set; }
        public string ParagraphId { get; set; }
    }
}
