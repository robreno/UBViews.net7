namespace UBViews.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UBViews.Models.AppData;
using UBViews.Models.Ubml;

public interface IEmailService
{
    #region  Enums
    public enum EmailType { PlainText, Html };
    public enum SendMode { AutoSend, RecipientList };
    #endregion

    #region   Interface
    Task<bool> IsValidEmail(string email);
    Task<int> ContactsCount();
    Task<List<ContactDto>> GetContactsAsync();
    Task<List<ContactDto>> GetAutoSendContactsAsync();
    Task<List<string>> GetAutoSendEmailListAsync();
    Task ShareParagraph(Paragraph paragraph);
    Task EmailParagraph(Paragraph paragraph, EmailType type, SendMode mode);
    Task<string> CreatePlainTextBodyAsync(Paragraph paragraph);
    Task<string> CreateHtmlBodyAsync(Paragraph paragraph);
    #endregion
}
