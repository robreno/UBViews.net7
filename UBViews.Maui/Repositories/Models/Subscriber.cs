using SQLite;

namespace UBViews.Repositories.Models;
public class Subscriber
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateSubscribed { get; set; }
}
