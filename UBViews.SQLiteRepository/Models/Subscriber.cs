using SQLite;

namespace UBViews.SQLiteRepository.Models
{
    public class Subscriber
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateSubscribed { get; set; }
    }
}
