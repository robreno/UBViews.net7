namespace UBViews.Models.AppData;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ContactDto
{
    public int Id { get; set; }
    public bool AutoSendEmail { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public bool EqualTo(ContactDto dto)
    {
        bool isEqual = false;
        if (dto == null) { return false; }
        else
        {
            if (dto.AutoSendEmail == AutoSendEmail &&
                dto.FirstName == FirstName &&
                dto.LastName == LastName &&
                dto.DisplayName == DisplayName &&
                dto.Email == Email)
            {
                isEqual = true;
            }
        }
        return isEqual;
    }
}
