using System.ComponentModel;
using System.Xml.Linq;
using UBViews.Services;

namespace UBViews.Helpers;

public partial class XmlAppSettingsService : IAppSettingsService
{
    private readonly string _settingsFileName = "Settings.xml";
    private XDocument _settings;
    private XElement _settingsRoot;
    private string _content;
    private string _appDir;
    private bool _cacheDirty;

    public XmlAppSettingsService()
    {
        InitializeSettings();
    }

    private async void InitializeSettings()
    {
        try
        {
            // C:\Users\robre\AppData\Local\Packages\aa91c2eb-6265-48b2-8835-b94bb1c7b79b_9zz4h110yvjzm\LocalState
            _appDir = FileSystem.Current.AppDataDirectory;
            _content = await LoadAppSettingsAsync(_settingsFileName);
            _settings = XDocument.Parse(_content, LoadOptions.None);
            _settingsRoot = _settings.Root;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }
    private async Task<string> LoadAppSettingsAsync(string filename)
    {
        try
        {
            string targetFilePath = Path.Combine(_appDir, filename);
            using FileStream inputStream = File.OpenRead(targetFilePath);
            using StreamReader reader = new StreamReader(inputStream);
            string contents = reader.ReadToEnd();
            return contents;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    private async Task SaveAppSettingsAsync(string filename)
    {
        try
        {
            string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, filename);
            using var stream = File.Create(targetFilePath);
            CancellationToken token = new CancellationToken(false);
            await _settingsRoot.SaveAsync(stream, SaveOptions.DisableFormatting, token);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    public async Task Clear()
    {
        try
        {
            _settingsRoot.RemoveNodes();
            _settingsRoot.SetAttributeValue("count", 0);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    public async Task Clear(string key)
    {
        try
        {
            var item = _settingsRoot.Descendants("Setting")
                                    .Where(setting => setting.Attribute("name").Value == key)
                                    .FirstOrDefault();
            if (item == null)
            {
                return;
            }
            else
            {
                item.Remove();
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    public async Task<bool> ContainsKey(string key)
    {
        try
        {
            var item = _settingsRoot.Descendants("Setting")
                                    .Where(setting => setting.Attribute("name").Value == key)
                                    .FirstOrDefault();
            if (item == null)
            {
                return false;
            }
            return true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return default(bool);
        }
    }
    public async Task<T> Get<T>(string key, T defaultValue)
    {
        try
        {
            var item = _settingsRoot.Descendants("Setting")
                                    .Where(setting => setting.Attribute("name").Value == key)
                                    .FirstOrDefault();
            if (item == null)
            {
                return defaultValue;
            }
            else
            {
                var value = item.Attribute("value").Value;
                T newT = GetTfromString<T>(value);
                return newT;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return default(T);
        }
    }

    public async Task Set<T>(string key, T value)
    {
        try
        {
            var item = _settingsRoot.Descendants("Setting")
                                    .Where(setting => setting.Attribute("name").Value == key)
                                    .FirstOrDefault();

            if (item == null)
            {
                throw new Exception("Setting Error item null");
            }

            var strValue = item.Attribute("value").Value;
            var T_value = GetTfromString<T>(strValue);

            if (T_value.Equals(value))
            {
                return;
            }

            item.SetAttributeValue("value", value);
            await SaveAppSettingsAsync(_settingsFileName);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    public async Task SetCache<T>(string key, T value)
    {
        try
        {
            var item = _settingsRoot.Descendants("Setting")
                                    .Where(setting => setting.Attribute("name").Value == key)
                                    .FirstOrDefault();

            if (item == null)
            {
                throw new Exception("Setting Error item null");
            }

            var strValue = item.Attribute("value").Value;
            var T_value = GetTfromString<T>(strValue);

            if (T_value.Equals(value))
            {
                return;
            }

            item.SetAttributeValue("value", value);
            _cacheDirty = true;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    [RelayCommand]
    public async Task SaveCache()
    {
        try
        {
            if (_cacheDirty == false)
            {
                return;
            }
            await SaveAppSettingsAsync(_settingsFileName);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    static private T ConvertStringValue<T>(string type, string value)
    {
        T newValue = GetTfromString<T>(value);
        return newValue;
    }

    static private T GetTfromString<T>(string mystring)
    {
        var foo = TypeDescriptor.GetConverter(typeof(T));
        return (T)foo.ConvertFromInvariantString(mystring);
    }
}
