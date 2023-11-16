namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;

public interface IContactsService
{
    Task SaveContactsAsync();
    Task<int> SaveContactAsync(ContactDto contact);
    Task<List<ContactDto>> GetContactsAsync();
    Task<List<ContactDto>> GetAutoSendContactsAsync();
    Task<List<string>> GetAutoSendEmailListAsync();
    Task<ContactDto> GetContactByIdAsync(string id);
    Task<ContactDto> GetContactByDisplayNameAsync(string displayName);
    Task<int> UpdateContactAsync(ContactDto contact);
    Task<int> DeleteContactAsync(ContactDto contact);
    Task<bool> DisplayNameExistsAsync(string displayName);
}
