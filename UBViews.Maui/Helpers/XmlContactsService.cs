namespace UBViews.Helpers;

using System.Xml.Linq;

using UBViews.Services;
using UBViews.Models.AppData;

// C:\Users\robre\AppData\Local\Packages\UBViews_1s7hth42e283a\LocalState

public class XmlContactsService : IContactsService
{
    /// <summary>
    /// Private Data Members
    /// </summary>
    private const string _contactsFileName = "Contacts.xml";
    private XDocument _contactsXDoc;
    private XElement _contactsRoot;
    private string _content;
    private string _appDir;
    private bool _cacheDirty;
    private int _cacheCount;

    /// <summary>
    /// 
    /// </summary>
    IFileService fileService;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileService"></param>
    public XmlContactsService(IFileService fileService)
    {
        this.fileService = fileService;
        Task.Run( async () => await InitializeData());
    }

    #region Private Methods
    /// <summary>
    /// InitializeData
    /// </summary>
    /// <returns></returns>
    private async Task InitializeData()
    {
        try
        {
            // 
            _appDir = FileSystem.Current.AppDataDirectory;
            _content = await LoadContactsAsync(_contactsFileName);
            _contactsXDoc = XDocument.Parse(_content, LoadOptions.None);
            _contactsRoot = _contactsXDoc.Root;
            _cacheCount = Int32.Parse(_contactsRoot.Attribute("count").Value);
            _cacheDirty = false;
            return;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// LoadContactsAsync
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns>Contacts Xml content as string</returns>
    private async Task<string> LoadContactsAsync(string fileName)
    {
        try
        {
            string targetFile = System.IO.Path.Combine(_appDir, fileName);
            using FileStream inputStream = System.IO.File.OpenRead(targetFile);
            using StreamReader reader = new StreamReader(inputStream);
            string _content = reader.ReadToEnd();
            return _content;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    #endregion

    #region
    /// <summary>
    /// SaveContactAsyn
    /// </summary>
    /// <param name="contact"></param>
    /// <returns></returns>
    public async Task<int> SaveContactAsync(ContactDto contact)
    {
        try
        {
            var lastContact = _contactsRoot.LastNode as XElement;
            var nextId = Int32.Parse(lastContact.Attribute("id").Value);
            contact.Id = ++nextId;

            XElement newContact = new XElement("Contact",
                                    new XAttribute("id", contact.Id),
                                    new XAttribute("autoSend", contact.AutoSendEmail),
                                    new XElement("FirstName", contact.FirstName),
                                    new XElement("LastName", contact.LastName),
                                    new XElement("DisplayName", contact.DisplayName),
                                    new XElement("Email", contact.Email));

            _contactsRoot.Add(newContact);
            _cacheDirty = true;
            _cacheCount++;
            _contactsRoot.SetAttributeValue("count", _cacheCount);
            return nextId;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return 0;
        }
    }

    /// <summary>
    /// SaveContactsAsync
    /// </summary>
    /// <returns></returns>
    public async Task SaveContactsAsync()
    {
        try
        {
            if (_cacheDirty)
            {
                string targetFile = System.IO.Path.Combine(_appDir, _contactsFileName);
                _contactsRoot.Save(targetFile, SaveOptions.None);
                _cacheDirty = false;
                _cacheCount--;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
        }
    }

    /// <summary>
    /// GetContactsAsync
    /// </summary>
    /// <returns>List of ContactDto</returns>
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        try
        {
            List<ContactDto> contacts = new();
            var contactElms = _contactsRoot.Descendants("Contact");
            foreach (var elm in contactElms) 
            {
                var id = Int32.Parse(elm.Attribute("id").Value);
                var autoSend = "false" == elm.Attribute("autoSend").Value ? false : true;
                var firstName = elm.Element("FirstName").Value;
                var lastName = elm.Element("LastName").Value;
                var displayName = elm.Element("DisplayName").Value;
                var email = elm.Element("Email").Value;
                var newContact = new ContactDto()
                {
                    Id = id,
                    AutoSendEmail = autoSend,
                    FirstName = firstName,
                    LastName = lastName,
                    DisplayName = displayName,
                    Email = email
                };
                contacts.Add(newContact);
            }
            return contacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    #endregion
}
