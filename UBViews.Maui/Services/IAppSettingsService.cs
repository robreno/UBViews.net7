namespace UBViews.Services;

public interface IAppSettingsService
{
    Task<bool> IsDirty();
    Task Clear();
    Task Clear(string key);
    Task<bool> ContainsKey(string key);
    Task<T> Get<T>(string key, T defaultValue);
    Task Set<T>(string key, T value);
    Task SetCache<T>(string key, T value);
    Task SaveCache();
}
