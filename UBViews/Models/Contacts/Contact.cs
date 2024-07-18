namespace UBViews.Models.Contacts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Contact
{
    public int Id { get; set; }
    public bool AutoSendEmail { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    public string Email {  get; set; }

    public bool EqualTo(Contact dto)
    {
        bool isEqual = false;
        if (dto == null) { return false; }
        else
        {
            if (dto.Id == this.Id &&
                dto.FirstName == this.FirstName &&
                dto.AutoSendEmail == this.AutoSendEmail &&
                dto.LastName == this.LastName &&
                dto.DisplayName == this.DisplayName &&
                dto.Email == this.Email)
            {
                isEqual = true;
            }
        }
        return isEqual;
    }
}
