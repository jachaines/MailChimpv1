using System;
using System.Configuration;
using MailChimp;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MailChimpService.MailChimp
{
    internal class MailChimpConfiguration : ConfigurationSection
    {
        #region Properties
        [ConfigurationProperty("APIKey", IsRequired = false)]
        public string APIKey
        {
            get { return this["APIKey"].ToString(); }
            set { this["APIKey"] = value; }
        }
        [ConfigurationProperty("UserName", IsRequired = false)]
        public string UserName
        {
            get { return this["UserName"].ToString(); }
            set { this["UserName"] = value; }
        }

        [ConfigurationProperty("Password", IsRequired = false)]
        public string Password
        {
            get { return this["Password"].ToString(); }
            set { this["Password"] = value; }
        }

        [ConfigurationProperty("ListName", IsRequired = true)]
        public string ListName
        {
            get { return this["ListName"].ToString(); }
            set { this["ListName"] = value; }
        }

        [ConfigurationProperty("RetryCount", IsRequired = true)]
        public int RetryCount
        {
            get { return (int)this["RetryCount"]; }
            set { this["RetryCount"] = value; }
        }

        [ConfigurationProperty("Columns", IsRequired = false)]
        [ConfigurationCollection(typeof(Column))]
        public ColumnCollection Columns
        {
            get { return (ColumnCollection)this["Columns"]; }
        }

        public List<MCMergeVar> ListColumns
        {
            get
            {
                var retval = new List<MCMergeVar>();
                foreach (Column element in Columns)
                {
                    retval.Add(new MCMergeVar
                                   {
                                       name = element.name,
                                       req = Convert.ToBoolean(element.required),
                                       tag = element.tag
                                   });
                }
                return retval;
            }
        }
        #endregion

        #region Static Methods
        public static MailChimpConfiguration Load()
        {
            return (MailChimpConfiguration)ConfigurationManager.GetSection("MailChimpConfiguration");
        }
        #endregion
    }
    internal class Column : ConfigurationElement
    {
        [ConfigurationProperty("name", IsRequired = true)]
        public string name
        {
            get { return this["name"].ToString(); }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("tag", IsRequired = true)]
        public string tag
        {
            get { return this["tag"].ToString(); }
            set { this["tag"] = value; }
        }

        [ConfigurationProperty("required", IsRequired = true)]
        public string required
        {
            get { return this["required"].ToString(); }
            set { this["required"] = value; }
        }
    }
    internal class ColumnCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Column();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Column)element).name;
        }
    }

}
