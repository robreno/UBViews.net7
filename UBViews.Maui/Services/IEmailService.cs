namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.AppData;
public interface IEmailService
{
    Task<List<ContactDto>> GetContactsAsync();
    Task SetPreTextAsync(string preText);
    Task SetPostTextAsync(string postText);
    Task SetRecipientsAsync(List<string> recipients);
    Task SetSubjectAsync(string subject);
    Task<string> CreateEmailTextAsync(string pretext, string postText, string subject, List<string> recipients, EmailBodyFormat bodyFormat = EmailBodyFormat.PlainText);
    Task<string> CreatePlainTextEmailAsync(string pretext, string postText, string subject, List<string> recipients);
    Task<string> CreateHtmlTextEmailAsync(string pretext, string postText, string subject, List<string> recipients);
}
