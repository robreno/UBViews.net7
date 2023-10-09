namespace UBViews.Helpers;

using System.ComponentModel;
using System.Xml.Linq;
using UBViews.Services;
public partial class XmlAppSettingsService : IAppSettingsService
{
    /// <summary>
    /// Private Data Members
    /// </summary>
    private const string _settingsFileName = "Settings.xml";
    private XDocument _settings;
    private XElement _settingsRoot;
    private string _content;
    private string _appDir;
    private bool _cacheDirty;

    /// <summary>
    /// File Service
    /// </summary>
    IFileService fileService;

    public XmlAppSettingsService(IFileService fileService)
    {
        InitializeSettings();
        this.fileService = fileService;
    }

    /// <summary>
    /// 
    /// </summary>
    private async void InitializeSettings()
    {
        try
        {
            // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task<string> LoadAppSettingsAsync(string fileName)
    {
        try
        {
            string targetFilePath = Path.Combine(_appDir, fileName);
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task SaveAppSettingsAsync(string fileName)
    {
        try
        {
            string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using var stream = File.Create(targetFilePath);
            CancellationToken token = new CancellationToken(false);
            await _settingsRoot.SaveAsync(stream, SaveOptions.None, token);
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Set<T>(string key, T value)
    {
        // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
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

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>

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
            _cacheDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    static private T ConvertStringValue<T>(string type, string value)
    {
        T newValue = GetTfromString<T>(value);
        return newValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="mystring"></param>
    /// <returns></returns>
    static private T GetTfromString<T>(string mystring)
    {
        var foo = TypeDescriptor.GetConverter(typeof(T));
        return (T)foo.ConvertFromInvariantString(mystring);
    }
}
