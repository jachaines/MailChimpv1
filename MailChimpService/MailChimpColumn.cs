namespace MailChimpService.MailChimp
{
    public class MailChimpColumn
    {
        public MailChimpColumn(string name, string tag, bool required)
        {
            Name = name;
            Tag = tag;
            Required = required;
        }

        public string Name
        {
            get;
            set;
        }

        public string Tag
        {
            get;
            set;
        }

        public bool Required
        {
            get;
            set;
        }
    }
}