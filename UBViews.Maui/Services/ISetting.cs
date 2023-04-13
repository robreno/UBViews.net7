namespace UBViews.Services
{
    public interface IAppSettings
    {
        Task Clear();
        Task Clear(string key);
        Task ContainsKey(string key);
        Task<T> Get<T>(string key, T defaultValue);
        Task Set<T>(string key, T value);
    }
}
