namespace UBViews.Services;

using UBViews.Models.AppData;
using System.Xml.Linq;

public interface IContactsService
{
    Task<int> SaveContactAsync(ContactDto contact);
    Task SaveContactsAsync();
    Task<List<ContactDto>> GetContactsAsync();
}
