namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;

public interface IContactsService
{
    Task SaveContactsAsync();
    Task<int> SaveContactAsync(ContactDto contact);
    Task<List<ContactDto>> GetContactsAsync();
}
