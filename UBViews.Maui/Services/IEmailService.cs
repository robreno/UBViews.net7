using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UBViews.Services;
public interface IEmailService
{
    Task<string> CreateEmailText(string pretext, string postText, string subject, List<string> recipients, EmailBodyFormat bodyFormat = EmailBodyFormat.PlainText);
}
