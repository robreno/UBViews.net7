using UBViews.SQLiteRepository.Models;

namespace UBViews.SQLiteRepository.Services
{
    public interface ISubscriberService
    {
        //Task LoadDatabaseAsync(string dbPath);
        Task<List<Subscriber>> GetSubscribersAsync();
        Task<int> SaveSubscriberAsync(Subscriber subscriber);
    }
}
