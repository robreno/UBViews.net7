using UBViews.Repositories.Models;

namespace UBViews.Services
{
    public interface ISubscriberService
    {
        //Task LoadDatabaseAsync(string dbPath);
        Task<List<Subscriber>> GetSubscribersAsync();
        Task<int> SaveSubscriberAsync(Subscriber subscriber);
    }
}
