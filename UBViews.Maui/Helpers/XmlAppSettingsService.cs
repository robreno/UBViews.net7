namespace UBViews.Helpers;

using System.ComponentModel;
using System.Xml.Linq;
using System.Collections.ObjectModel;

using UBViews.Services;
using UBViews.Models.Settings;

public partial class XmlAppSettingsService : IAppSettingsService
{
    #region Private Data Members
    /// <summary>
    /// Private Data Members
    /// </summary>
    private const string _settingsFileName = "Settings.xml";
    private string _appDir;
    private string _contents;
    private XDocument _settings;
    private XElement _settingsRoot;

    private bool _initialized = false;
    private bool _dataInitialized = false;
    private bool _cacheDirty = false;
    //private bool _settingsDirty = false;

    ObservableCollection<Setting> Settings = new();

    /// <summary>
    /// File Service
    /// </summary>
    IFileService fileService;

    readonly string _class = "XmlAppSettingsService";
    #endregion

    #region Constructor
    public XmlAppSettingsService(IFileService fileService)
    {
        this.fileService = fileService;
    }
    #endregion

    #region Private Initialization Methods
    private async Task InitializeDataAsync()
    {
        string _method = "InitializeDataAsync";
        try
        {
            // Sets _initialzed to true if successful
            var loadContentResult = await LoadContentAsync();
            if (!loadContentResult)
            {
                throw new Exception("Initialization Exception: Settings file failed to load.");
            }
            // Sets _dataInitialized to true if successful
            var loadDataResult = await InitializeSettingsAsync();
            if (!loadDataResult)
            {
                throw new Exception("Initialization Exception: Loading date failed.");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    private async Task<bool> InitializeSettingsAsync()
    {
        string _method = "InitializeSettingsAsync";
        try
        {
            if (!_initialized)
            {
                throw new Exception("Data Initialization Exception.");
            }

            var settings = _settingsRoot.Descendants("Setting");
            foreach (var setting in settings)
            {
                var id = setting.Attribute("id").Value;
                var type = setting.Attribute("type").Value;
                var name = setting.Attribute("name").Value;
                var value = setting.Attribute("value").Value;

                Setting newSetting = new Setting()
                {
                    Id = Int32.Parse(id),
                    Type = type,
                    Name = name,
                    Value = value
                };
                Settings.Add(newSetting);
            }
            _dataInitialized = true;
            return _dataInitialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    public async Task InitializeSettings()
    {
        string _method = "InitializeSettings";
        try
        {
            // C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState
            _appDir = FileSystem.Current.AppDataDirectory;
            _contents = await LoadAppSettingsAsync(_settingsFileName);
            _settings = XDocument.Parse(_contents, LoadOptions.None);
            _settingsRoot = _settings.Root;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion

    #region Public Interface Methods
    /// <summary>
    ///  IsDirty
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsDirty()
    {
        string _method = "IsDirty";
        try
        {
            return _cacheDirty;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }

    /// <summary>
    /// Clear
    /// </summary>
    /// <returns></returns>
    public async Task Clear()
    {
        string _method = "Clear()";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

            _settingsRoot.RemoveNodes();
            _settingsRoot.SetAttributeValue("count", 0);
            _cacheDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// Clear
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task Clear(string key)
    {
        string _method = "Clear(key)";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// ContainsKey
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public async Task<bool> ContainsKey(string key)
    {
        string _method = "ContainsKey(key)";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return default(bool);
        }
    }

    /// <summary>
    /// Get<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public async Task<T> Get<T>(string key, T defaultValue)
    {
        string _method = "Get<T>(key, defaultValue)";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return default(T);
        }
    }

    /// <summary>
    /// Set<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task Set<T>(string key, T value)
    {
        string _method = "Set<T>(key, value)";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

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
            _cacheDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// SetCache<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task SetCache<T>(string key, T value)
    {
        string _method = "SetCache<T>(key, value)";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }

    /// <summary>
    /// SaveCache
    /// </summary>
    /// <returns></returns>
    public async Task SaveCache()
    {
        string _method = "SaveCache";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

            if (_cacheDirty == false)
            {
                return;
            }
            await SaveAppSettingsAsync(_settingsFileName);
            _cacheDirty = false;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region Private Load/Save File Helper Methods
    /// <summary>
    /// LoadContentAsync
    /// </summary>
    /// <returns></returns>
    private async Task<bool> LoadContentAsync()
    {
        string _method = "LoadContentExAsync";
        try
        {
            _appDir = FileSystem.AppDataDirectory;
            string targetFilePath = Path.Combine(_appDir, _settingsFileName);
            using FileStream inputStream = File.OpenRead(targetFilePath);
            using StreamReader reader = new StreamReader(inputStream);
            _contents = reader.ReadToEnd();
            _settings = XDocument.Parse(_contents);
            _settingsRoot = _settings.Root;
            _initialized = true;
            return _initialized;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task<string> LoadAppSettingsAsync(string fileName)
    {
        string _method = "LoadAppSettingsAsync";
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
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }

    /// <summary>
    /// SaveAppSettingsAsync
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    private async Task SaveAppSettingsAsync(string fileName)
    {
        string _method = "SaveAppSettingsAsync";
        try
        {
            if (!_initialized)
            {
                await InitializeDataAsync();
            }

            string targetFilePath = Path.Combine(FileSystem.Current.AppDataDirectory, fileName);
            using var stream = File.Create(targetFilePath);
            CancellationToken token = new CancellationToken(false);
            if (_settingsRoot != null)
            {
                await _settingsRoot.SaveAsync(stream, SaveOptions.None, token);
                _cacheDirty = false;
            }
            else
            {
                throw new Exception("Initialization exception: settings root was null.");
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region Private Helper Methods
    private async Task<XElement> GetSettingElementByIdAsync(string id)
    {
        string _method = "GetSettingElmByIdAsync";

        try
        {
            var _settingElm = _settingsRoot.Descendants("Setting")
                                           .Where(c => c.Attribute("id").Value == id)
                                           .FirstOrDefault();
            return _settingElm;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return null;
        }
    }
    private async Task<bool> EqualToAsync(XElement oldSetting, Setting newSetting)
    {
        string _method = "EqualToAsync";
        try
        {
            var isEqual = false;

            if (oldSetting != null)
            {
                var id = Int32.Parse(oldSetting.Attribute("id").Value);
                var type = oldSetting.Attribute("type").Value;
                var name = oldSetting.Attribute("name").Value;
                var value = oldSetting.Attribute("value").Value;

                var _oldSetting = new Setting
                {
                    Id = id,
                    Type = type,
                    Name = name,
                    Value = value
                };

                isEqual = _oldSetting.EqualTo(newSetting);
            }
            return isEqual;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return false;
        }
    }
    private async Task<int> UpdateSettingAsync(Setting newSetting)
    {
        string _method = "UpdateSettingAsync";

        try
        {
            string message = string.Empty;
            var id = newSetting.Id;
            //var id = settingId.ToString("0");

            var oldSetting = await GetSettingElementByIdAsync(id.ToString("0"));
            var isEqual = await EqualToAsync(oldSetting, newSetting);

            if (!isEqual)
            {
                XElement _setting = new XElement("Setting",
                                        new XAttribute("id", newSetting.Id),
                                        new XAttribute("type", newSetting.Type),
                                        new XAttribute("name", newSetting.Name),
                                        new XAttribute("value", newSetting.Value));

                oldSetting.ReplaceWith(_setting);

                _cacheDirty = true;
                //await SaveSettingsAsync();

                message = "Successfully Updated!";
                await App.Current.MainPage.DisplayAlert($"Update Contact", message, "Ok");
            }
            return id;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task<int> SaveSettingAsync(Setting setting)
    {
        string _method = "SaveSettingAsync";

        try
        {
            string message = string.Empty;
            XElement newSetting = new XElement("Setting",
                                    new XAttribute("id", setting.Id),
                                    new XAttribute("type", setting.Type),
                                    new XAttribute("name", setting.Name),
                                    new XAttribute("value", setting.Value));

            // update setting here 
            var targets = _settingsRoot.Descendants("Setting");
            XElement target = null;
            if (targets != null)
            {
                target = targets.Where(e => e.Attribute("id").Value == setting.Id.ToString("0"))
                                .FirstOrDefault();

                target.Attribute("value").Value = setting.Value;
            }
            await SaveSettingsAsync();

            message = "Successfully Saved!";
            await App.Current.MainPage.DisplayAlert($"Save Setting", message, "Ok");

            return setting.Id;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
            return 0;
        }
    }
    private async Task SaveSettingsAsync()
    {
        string _method = "SaveSettingsAsync";

        try
        {
            if (_cacheDirty)
            {
                string targetFile = System.IO.Path.Combine(_appDir, _settingsFileName);
                _settingsRoot.Save(targetFile, SaveOptions.None);
                _cacheDirty = false;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_class}.{_method} => ", ex.Message, "Ok");
        }
    }
    #endregion

    #region Private Static Methods
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
    #endregion
}
