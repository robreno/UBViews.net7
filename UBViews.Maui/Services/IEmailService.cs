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
    Task<bool> CanSendEmailAsync();
    Task<bool> IsValidEmailAsync(string email);
    Task<int> ContactsCountAsync();
    Task<List<ContactDto>> GetContactsAsync();
    Task<List<ContactDto>> GetAutoSendContactsAsync();
    Task<List<string>> GetAutoSendEmailListAsync();
    Task ShareParagraphAsync(Paragraph paragraph);
    Task ShareParagraphsAsync(List<Paragraph> paragarphs);
    Task EmailParagraphAsync(Paragraph paragraph, EmailType type, SendMode mode);
    Task EmailParagraphsAsync(List<Paragraph> paragraphs, EmailType type, SendMode mode);
    Task<string> CreatePlainTextBodyAsync(Paragraph paragraph);
    Task<string> CreatePlainTextBodyAsync(List<Paragraph> paragraphs);
    Task<string> CreateHtmlBodyAsync(Paragraph paragraph);
    #endregion
}
