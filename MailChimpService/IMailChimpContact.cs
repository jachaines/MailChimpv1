using System;
using System.Collections.Generic;

namespace MailChimpService.MailChimp
{
    public interface IMailChimpContact
    {
        Int64 UserId { get; }
        IDictionary<string, string> FieldValues { get; }
        bool Subscribed { get; }
        string Email { get; }
        List<string> Groups { get; }
    }
}