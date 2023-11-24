namespace UBViews.Helpers;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using Microsoft.Maui.ApplicationModel.Communication;

using UBViews.Models.AppData;
using UBViews.Services;
using UBViews.Models.Ubml;

public class EmailService : IEmailService
{
    #region  Private Data Members
    private const string _htmlEmailTemplate = $"<html><body><p id=\"pre\"></p><blockquote></blockquote><p id=\"post\"></p></body></html>";
    private const string _shareTitle = "Quote From Urantia Book";
    private const string _shareSubject = "Sharing a quote from The Urantia Book";
    private const string _preText = "I thought of you when I read this quote from The Urantia Book by The Urantia Foundation - ";
    private const string _preTextHtml = "I thought of you when I read this quote from <i>The Urantia Book</i> by The Urantia Foundation - ";
    private const string _postText = "UBViews: The Urantia Book Viewer & Search Engine – Agondonter Media.";

    private string _subject = string.Empty;
    private List<string> _recipients = new List<string>();
    private List<ContactDto> _contacts = new List<ContactDto>();
    //private int _tries = 0;
    //private int _maxTries = 3;
    private bool _contactsInitialized = false;
    private bool _recipientsInitialized = false;
    private bool _isInitialized = false;

    // /^[a-zA-Z0-9.!#$%&’*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*$/
    private static string validEmailPattern1 = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
    //private static string validEmailPattern2 = @"^[0-9a-zA-Z] ([-.\w]*[0 - 9a - zA - Z_ +])*@([0 - 9a - zA - Z][-\w]*[0 - 9a - zA - Z]\.)+[a-zA-Z]{2,9}$";
    //private static string validEmailPattern3 = @"^[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z_+])*@([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,9}$";
    private Regex _rgxEmail1 = new Regex(validEmailPattern1);

    private readonly string _className = "EmailService";
    #endregion

    #region  Services
    IContactsService contactService;
    IAppSettingsService settingsService;
    #endregion

    #region  Constructors
    public EmailService(IContactsService contactService, IAppSettingsService settingsService)
    {
        this.contactService = contactService;
        this.settingsService = settingsService;
    }
    #endregion

    #region Private Methods
    private async Task InitializeContacts()
    {
        string _methodName = "InitializeContacts";

        try
        {
            _contacts = await contactService.GetContactsAsync();
            if (_contacts != null)
            {
                _contactsInitialized = true;
            }

            _recipients = await contactService.GetAutoSendEmailListAsync();
            if (_recipients != null)
            {
                _recipientsInitialized = true;
            }

            if (_contacts != null && _recipients != null)
            {
                _isInitialized = true;
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert($"Exception raised in {_className}.{_methodName} => ", ex.Message, "Ok");
            return;
        }
    }
    #endregion

    #region Private Email Methods
    /// <summary>
    /// ShareParagarphText
    /// </summary>
    /// <param name="text"></param>
    /// <param name="title"></param>
    /// <param name="subject"></param>
    /// <returns></returns>
    private async Task ShareParagarphText(string text, string title = _shareTitle, string subject = _shareSubject)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareTextRequest
            {
                Title = title,
                Subject = subject,
                Text = text
            });
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// ShareUri
    /// </summary>
    /// <param name="title"></param>
    /// <param name="uri"></param>
    /// <param name="share"></param>
    /// <returns></returns>
    private async Task ShareUri(string title, string uri, IShare share)
    {
        await share.RequestAsync(new ShareTextRequest
        {
            Title = title,
            Uri = uri
        });
    }

    /// <summary>
    /// SendToast
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private async Task SendToast(string message)
    {
        try
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                ToastDuration duration = ToastDuration.Short;
                double fontSize = 14;
                var toast = Toast.Make(message, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            return;
        }
    }
    //private Task<string> CreateHtmlBodyAsync(string pretext, string postText, string subject, List<string> recipients)
    #endregion

    #region  Interface Implementations
    /// <summary>
    /// GetAutoRecipients
    /// </summary>
    /// <returns></returns>
    public async Task<bool> CanSendEmail()
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            bool autoSendFlag = false;
            int recipientsCount = 0;

            recipientsCount = _recipients.Count;
            autoSendFlag = await settingsService.Get("auto_send_email", false);

            bool _canSendEmail = true;
            string promptMessage = string.Empty;
            string autoSendAction = $" set \'Auto Send Email\'";
            string emptyContactsAction = $" add contact and set to AutoSend.";

            if (recipientsCount == 0)
            {
                if (autoSendFlag == false)
                {
                    promptMessage = $"You have no contacts and 'Auto Send Email' is not set.\r" +
                                    $"Please go to Settigs and {autoSendAction} and {emptyContactsAction}.";
                }
                if (autoSendFlag == true)
                {
                    promptMessage = $"You have no contacts.\r" +
                                    $"Please go to Settigs => Contacts and {emptyContactsAction}.";
                }
                await App.Current.MainPage.DisplayAlert("Share Email", promptMessage, "Cancel");
                _canSendEmail = false;
            }
            else if (recipientsCount > 0 && autoSendFlag == false)
            {
                promptMessage = $"'Auto Send Email' is not set. " +
                                $"Please go to Settigs and {autoSendAction}.";

                await App.Current.MainPage.DisplayAlert("Share Email", promptMessage, "Cancel");
                _canSendEmail = false;
            }
            return _canSendEmail;
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error!", ex.Message, "OK");
            return false;
        }
    }

    /// <summary>
    /// IsValidEmail
    /// </summary>
    /// <param name="emailAddress"></param>
    /// <returns></returns>
    public async Task<bool> IsValidEmail(string emailAddress)
    {
        try
        {
            bool isValidEmail = false;
            isValidEmail = _rgxEmail1.Match(emailAddress).Success;
            return isValidEmail;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return false;
        }
    }

    /// <summary>
    /// ContactsCount
    /// </summary>
    /// <returns></returns>
    public async Task<int> ContactsCount()
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            var contacts = await GetContactsAsync();
            var count = contacts.Count();
            return count;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return 0;
        }

    }

    /// <summary>
    /// GetContactsAsync
    /// </summary>
    /// <returns></returns>
    public async Task<List<ContactDto>> GetContactsAsync()
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            if (!_contactsInitialized)
            {
                await InitializeContacts();
            }
            return _contacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// GetAutoSendContactsAsync
    /// </summary>
    /// <returns></returns>
    public async Task<List<ContactDto>> GetAutoSendContactsAsync()
    {
        try
        {
            if(!_isInitialized)
            {
                await InitializeContacts();
            }

            List<ContactDto> contacts = new List<ContactDto>();
            if (_contactsInitialized)
            {
                foreach (var contact in _contacts)
                {
                    if (contact.AutoSendEmail)
                    {
                        contacts.Add(contact);
                    }
                }
            }
            return contacts;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// GetAutoSendEmailListAsync
    /// </summary>
    /// <returns></returns>
    public async Task<List<string>> GetAutoSendEmailListAsync()
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            List<string> recipients = new();
            if (_contactsInitialized)
            {
                foreach (var contact in _contacts)
                {
                    if (contact.AutoSendEmail)
                    {
                        recipients.Add(contact.Email);
                    }
                }
            }
            return recipients;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// ShareParagraph
    /// </summary>
    /// <param name="paragraph"></param>
    /// <returns></returns>
    public async Task ShareParagraph(Paragraph paragraph)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            var bodyText = await CreatePlainTextBodyAsync(paragraph);
            await ShareParagarphText(bodyText);
            await SendToast($"Paragraph {paragraph.Pid} shared!");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// ShareParagraphs
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    public async Task ShareParagraphs(List<Paragraph> paragraphs)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string bodyText = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(_preText);
            sb.AppendLine();
            sb.AppendLine();
            foreach (var paragraph in paragraphs)
            {
                var text = paragraph.CreatePlainTextBody();
                sb.Append(text);
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(_postText);
            bodyText = sb.ToString();
            await ShareParagarphText(bodyText);
            await SendToast($"Paragraphs shared!");
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// EmailParagraph
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="type"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public async Task EmailParagraph(Paragraph paragraph, IEmailService.EmailType type, IEmailService.SendMode mode)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            var _body = string.Empty;
            List<string> autoSendRecipients = new();

            autoSendRecipients = await GetAutoSendEmailListAsync();

            if (autoSendRecipients.Count == 0)
            {
                var contactsCount = await ContactsCount();
                string promptMessage = string.Empty;
                string secondAction = string.Empty;

                secondAction = " add or set contact(s) to AutoSend.";
                promptMessage = $"You have no contacts ({contactsCount}) or none are set to auto send.\r" +
                                $"Please go to the Settigs => Contacts page and {secondAction}.";

                await App.Current.MainPage.DisplayAlert("Email Paragraph", promptMessage, "Cancel");
                return;
            }

            switch (type)
            {
                case IEmailService.EmailType.PlainText:
                    _body = await CreatePlainTextBodyAsync(paragraph);
                    break;
                case IEmailService.EmailType.Html:
                    _body = await CreateHtmlBodyAsync(paragraph);
                    break;
                default:
                    // Default to plain text
                    _body = await CreatePlainTextBodyAsync(paragraph);
                    break;
            }

            // Send Email
            if (Email.Default.IsComposeSupported)
            {
                string subject = "UBViews Quote of the day ...";

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = _body,
                    BodyFormat = (type.Equals(IEmailService.EmailType.PlainText) ? EmailBodyFormat.PlainText : EmailBodyFormat.Html),
                    To = autoSendRecipients
                };

                await Email.Default.ComposeAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// EmailParagraphs
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <param name="type"></param>
    /// <param name="mode"></param>
    /// <returns></returns>
    public async Task EmailParagraphs(List<Paragraph> paragraphs, IEmailService.EmailType type, IEmailService.SendMode mode)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            var canSendEmail = await CanSendEmail();
            if (!canSendEmail)
            {
                return;
            }

            var _bodyText = string.Empty;
            switch (type)
            {
                case IEmailService.EmailType.PlainText:
                    _bodyText = await CreatePlainTextBodyAsync(paragraphs);
                    break;
                case IEmailService.EmailType.Html:
                    //_bodyText = await CreateHtmlBodyAsync(paragraphs);
                    break;
                default:
                    // Default to plain text
                    _bodyText = await CreatePlainTextBodyAsync(paragraphs);
                    break;
            }

            // Send Email
            if (Email.Default.IsComposeSupported)
            {
                string subject = "UBViews Quote of the day ...";

                var message = new EmailMessage
                {
                    Subject = subject,
                    Body = _bodyText,
                    BodyFormat = (type.Equals(IEmailService.EmailType.PlainText) ? EmailBodyFormat.PlainText : EmailBodyFormat.Html),
                    To = _recipients
                };

                await Email.Default.ComposeAsync(message);
            }
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return;
        }
    }

    /// <summary>
    /// CreatePlainTextBodyAsync
    /// </summary>
    /// <param name="paragraph"></param>
    /// <returns></returns>
    public async Task<string> CreatePlainTextBodyAsync(Paragraph paragraph)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            string body = paragraph.CreatePlainTextBody();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine(_preText);
            sb.AppendLine("");

            sb.Append(body);

            sb.AppendLine("");
            sb.AppendLine("");
            sb.AppendLine(_postText);

            emailText = sb.ToString();

            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreatePlainTextBodyAsync
    /// </summary>
    /// <param name="paragraphs"></param>
    /// <returns></returns>
    public async Task<string> CreatePlainTextBodyAsync(List<Paragraph> paragraphs)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string bodyText = string.Empty;
            StringBuilder sb = new StringBuilder();
            sb.Append(_preText);
            sb.AppendLine();
            sb.AppendLine();
            foreach (var paragraph in paragraphs)
            {
                var text = paragraph.CreatePlainTextBody();
                sb.Append(text);
                sb.AppendLine();
                sb.AppendLine();
            }
            sb.Append(_postText);
            bodyText = sb.ToString();

            return bodyText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreateHtmlBodyAsync
    /// </summary>
    /// <param name="paragraph"></param>
    /// <returns></returns>
    public async Task<string> CreateHtmlBodyAsync(Paragraph paragraph)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            string body = paragraph.CreateHtmlTextBody();

            var e = XElement.Parse(_htmlEmailTemplate);

            var blockquote = e.Descendants("blockquote").FirstOrDefault();
            var pre = e.Descendants("p").Where(e => e.Attribute("id").Value == "pre").FirstOrDefault();
            var post = e.Descendants("p").Where(p => p.Attribute("id").Value == "post").FirstOrDefault();
            pre.Value = _preTextHtml;
            post.Value = _postText;

            blockquote.Value = body;
            emailText = e.ToString();

            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreateEmailText
    /// </summary>
    /// <param name="paragraph"></param>
    /// <returns></returns>
    public async Task<string> CreateEmailText(Paragraph paragraph)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreateEmailTextAsync
    /// </summary>
    /// <param name="pretext"></param>
    /// <param name="postText"></param>
    /// <param name="subject"></param>
    /// <param name="recipients"></param>
    /// <param name="bodyFormat"></param>
    /// <returns></returns>
    public async Task<string> CreateEmailTextAsync(string pretext, string postText, string subject, List<string> recipients, EmailBodyFormat bodyFormat = EmailBodyFormat.PlainText)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreateHtmlTextEmailAsync
    /// </summary>
    /// <param name="pretext"></param>
    /// <param name="postText"></param>
    /// <param name="subject"></param>
    /// <param name="recipients"></param>
    /// <returns></returns>
    public async Task<string> CreateHtmlTextEmailAsync(string pretext, string postText, string subject, List<string> recipients)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }

    /// <summary>
    /// CreatePlainTextEmailAsync
    /// </summary>
    /// <param name="paragraph"></param>
    /// <param name="pretext"></param>
    /// <param name="postText"></param>
    /// <returns></returns>
    public async Task<string> CreatePlainTextEmailAsync(Paragraph paragraph, string pretext = _preText, string postText = _postText)
    {
        try
        {
            if (!_isInitialized)
            {
                await InitializeContacts();
            }

            string emailText = string.Empty;
            return emailText;
        }
        catch (Exception ex)
        {
            await App.Current.MainPage.DisplayAlert("Exception raised =>", ex.Message, "Cancel");
            return null;
        }
    }
    #endregion
}
