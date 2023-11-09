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
}
