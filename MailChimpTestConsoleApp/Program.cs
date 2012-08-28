using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MailChimpService.MailChimp;

namespace MailChimpTestConsoleApp
{
    internal class AxisUser : IMailChimpContact
    {
        #region Properties
        public Int64 UserId { get; private set; }
        public Int64 ArtistId { get; private set; }
        public IDictionary<string, string> FieldValues { get; private set; }
        public bool Subscribed { get; private set; }
        public string Email { get; private set; }
        public List<string> Groups { get; private set; }
        public bool HtmlEmail { get; private set; }
        #endregion

        #region Constructor
        public AxisUser(Int64 Userid, IDictionary<string, string> values, string email, List<string> group, bool htmlemail, bool subscribed)
        {
            this.UserId = Userid;
            this.FieldValues = values;
            this.Email = email;
            this.Groups = group;
            this.HtmlEmail = htmlemail;
            this.Subscribed = subscribed;
        }
        #endregion
    }

    class Program
    {
        static void Main(string[] args)
        {
            var myContact = new List<IMailChimpContact>
            {
                  new AxisUser (12345, 
                            new Dictionary<string, string>
                            {
                                {"ArtistID", "12"},
                                {"First Name", "freddy"},
                                {"Last Name", "flintstone"},
                                {"Payment Date", "12/12/12"},
                                {"TransactionID","666"},
                                {"Amount","23.50"},
                                {"ExportNo","6"},
                            },
                            "jason@axisweb.org",
                            //dr["Email"].ToString(),
                            new List<string> { "new member" }, 
                            true,
                            true
                        )
            };
            try
            {
                List<MailChimpColumn> myColumns = new List<MailChimpColumn>();
                myColumns.Add(new MailChimpColumn("ArtistID", "ARTISTID", false));
                myColumns.Add(new MailChimpColumn("First Name", "FNAME", false));
                myColumns.Add(new MailChimpColumn("Last Name", "LNAME", false));
                myColumns.Add(new MailChimpColumn("Payment Date", "PDATE", false));
                myColumns.Add(new MailChimpColumn("TransactionID", "TRANSID", false));
                myColumns.Add(new MailChimpColumn("Amount", "AMOUNT", false));
                myColumns.Add(new MailChimpColumn("ExportNo", "EXPORTNO", false));

                var chimp = new MailChimp("78af380c41e7d59376d2ec11abf15295-us5", "Axis Payments", 3, myColumns);
                var result = chimp.Update(myContact);
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e);
                Console.ReadLine();
            }
        }
    }
}
