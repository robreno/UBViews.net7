namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Services;
using UBViews.Models.AppData;

public class EmailService : IEmailService
{
    private string _preText = "I thought of you when I read this quote from The Urantia Book by The Urantia Foundation - ";
    private string _postText = "UBViews: The Urantia Book Viewer & Search Engine – Agondonter Media.";
    private string _subject = string.Empty;
    private List<string> _recipients = new List<string>();
    private List<ContactDto> _contacts = new List<ContactDto>();
    private int _tries = 0;
    private int _maxTries = 3;
    private bool _contactsInitialized = false;

    IContactsService contactService;

    public EmailService(IContactsService contactService)
    {
        this.contactService = contactService;
    }

    #region Private Methods
    private async Task InitializeContacts()
    {
        try
        {
            _contacts = await contactService.GetContactsAsync();
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }
    #endregion

    public async Task<List<ContactDto>> GetContactsAsync()
    {
        try
        {
            if (!_contactsInitialized) 
            {
                await InitializeContacts();
                _contactsInitialized = true;
            }
            return _contacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    public async Task SetPreTextAsync(string preText)
    {
        try
        {
            this._preText += preText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    public async Task SetPostTextAsync(string postText)
    {
        try
        {
            this._postText += postText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    public async Task SetRecipientsAsync(List<string> recipients)
    {
        try
        {
            foreach (var recipient in recipients)
            {
                _recipients.Add(recipient);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    public async Task SetSubjectAsync(string subject)
    {
        try
        {
            _subject = subject;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    public async Task<string> CreateEmailTextAsync(string pretext, string postText, string subject, List<string> recipients, EmailBodyFormat bodyFormat = EmailBodyFormat.PlainText)
    {
        try
        {
            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    public Task<string> CreateHtmlTextEmailAsync(string pretext, string postText, string subject, List<string> recipients)
    {
        throw new NotImplementedException();
    }

    public Task<string> CreatePlainTextEmailAsync(string pretext, string postText, string subject, List<string> recipients)
    {
        throw new NotImplementedException();
    }
}
