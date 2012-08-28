using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MailChimp;

namespace MailChimpService.MailChimp
{
    public class MailChimp
    {
        #region Constants
        private const string IdField = "USERID";
        #endregion

        #region Private Enum
        private enum Status
        {
            subscribed,
            unsubscribed
        }
        #endregion

        #region Member Variables
        private string _apiKey;
        private string _configApiKey;
        private string _listId;
        private string _listName;
        private int _retryCount;
        private static List<MCMergeVar> _listColumns;
        private static ApiWrapper _wrapper;
        private StringBuilder _reportBuilder;
        #endregion

        #region Properties
        public string Report { get { return _reportBuilder.ToString(); } }
        private static List<MCMergeVar> FixedColumns
        {
            get
            {
                return new List<MCMergeVar>
                           {
                              new MCMergeVar
                                       {
                                               name = "User Id",
                                               req = true,
                                               tag = "USERID"
                                       },
                                new MCMergeVar
                                       {
                                               name = "Email Address",
                                               req = true, 
                                               tag = "EMAIL"
                                     
                                       },
                            };
            }
        }

        private string apiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    var retryCount = 0;
                    while (retryCount < _retryCount)
                    {
                        try
                        {
                            _wrapper.setCurrentApiKey(_configApiKey);
                            _apiKey = _configApiKey;

                            if (string.IsNullOrEmpty(_apiKey))
                            {
                                throw new ConfigurationErrorsException(
                                        "The provided username and/or password is incorrect, or the provided API key is incorrect.");
                            }
                            retryCount = _retryCount;
                        }
                        catch
                        {
                            retryCount++;
                            if (retryCount >= _retryCount)
                            {
                                throw;
                            }
                        }
                    }
                }
                return _apiKey;
            }
        }

        private bool isLoggedIn
        {
            get { return !string.IsNullOrEmpty(apiKey); }
        }

        private string listId
        {
            get
            {
                if (string.IsNullOrEmpty(_listId))
                {
                    if (isLoggedIn)
                    {
                        foreach (var list in _wrapper.lists())
                        {
                            if (list.name == _listName)
                            {
                                _listId = list.id;
                                UpdateListColumns(_listId);
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(_listId))
                        {
                            throw new ConfigurationErrorsException("List name is not set correctly in the configuration file.");
                        }
                    }
                }
                return _listId;
            }
        }
        #endregion

        #region Constructor
        public MailChimp()
        {
            var config = MailChimpConfiguration.Load();

            _wrapper = new ApiWrapper();
            _configApiKey = config.APIKey;
            _listName = config.ListName;
            _retryCount = config.RetryCount;
            _listColumns = config.ListColumns;
        }

        public MailChimp(
            string apiKey,
            string listName,
            int retryCount,
            IEnumerable<MailChimpColumn> listColumns)
        {
            _wrapper = new ApiWrapper();
            _configApiKey = apiKey;
            _listName = listName;
            _retryCount = retryCount;
            _listColumns = new List<MCMergeVar>();

            foreach (var column in listColumns)
            {
                _listColumns.Add(new MCMergeVar
                {
                    name = column.Name,
                    req = column.Required,
                    tag = column.Tag
                });
            }
        }
        #endregion

        #region Public Methods
        public IList<Guid> Update(IEnumerable<IMailChimpContact> contacts)
        {   
            _reportBuilder = new StringBuilder();

                var retval = new List<Guid>();
            try
            {
             
                if (isLoggedIn)
                {
                    IList<MCMemberInfo> chimpMemberInfos = GetMailChimpListMembers();

                    var interests = contacts.SelectMany(c => c.Groups).Distinct();

                    if (interests != null && interests.Count() > 0)
                    {
                        UpdateInterestGroups(interests);
                    }

                    var deletes = ContactsToDelete(contacts, chimpMemberInfos);
                    var createsAndUpdates = ContactsToCreate(contacts, chimpMemberInfos).Union(ContactsToUpdate(contacts, chimpMemberInfos));

                    if (deletes != null && deletes.Count() > 0)
                    {
                        var results = _wrapper.listBatchUnsubscribe(listId, deletes.ToArray(), true, false, false);
                        ProcessResults(results);
                    }
                    if (createsAndUpdates != null && createsAndUpdates.Count() > 0)
                    {
                        var results = _wrapper.listBatchSubscribe(listId, createsAndUpdates.ToArray(), false, true, true);
                        ProcessResults(results);
                    }

                    retval = UnsubscribedMemberIds(chimpMemberInfos);
                }
            }
            catch (Exception ex)
            {

            }
            return retval;
        }

        private void ProcessResults(MCBatchResult results)
        {
            _reportBuilder.Append(string.Format("Deletes succeeded: {0}\n", results.success_count));
            _reportBuilder.Append(string.Format("Deletes errors: {0}\n", results.error_count));
            if (results.error_count > 0)
                foreach (var error in results.errors)
                {
                    _reportBuilder.Append(string.Format("Code: {0}\nRow: {1}\nMessage: {2}\n\n",
                                                        error.code,
                                                        error.row,
                                                        error.message));
                }
        }

        #endregion

        #region Private Funcs
        /// <summary>
        /// Delete contacts that are not subscribed, contacts that do not exist anymore, contacts that have a changed email address.
        /// </summary>
        private static readonly Func<IEnumerable<IMailChimpContact>, IEnumerable<MCMemberInfo>, IEnumerable<MCMemberInfo>> ContactsToDelete =
        (contacts, memberInfos) =>
            MapIContactsToMemberInfos(
                    contacts.Where(contact => contact.Subscribed == false)
                            .Join(memberInfos,
                                  contact => contact.UserId.ToString(),
                                  GetIdFieldFromMergeVars,
                                  (contact, info) => contact))
                    .Union(memberInfos.Where(info =>
                        !contacts.Any(contact => contact.UserId.ToString() == GetIdFieldFromMergeVars(info))))
                    .Union(memberInfos.Where(info => info.status == Status.unsubscribed.ToString()))
                    .Union(contacts.Where(contact => contact.Subscribed)
                                   .Join(memberInfos,
                                         contact => contact.UserId.ToString(),
                                         GetIdFieldFromMergeVars,
                                         (contact, info) => new
                                         {
                                             contact,
                                             info
                                         })
                                   .Where(pair => pair.contact.UserId != long.Parse(pair.info.merges[11].val))
                                   .Select(pair => pair.info));

        /// <summary>
        /// Create contacts are contacts that are subscribed and dont exist in MailChimp, or contacts with changed email address.
        /// </summary>
        private static readonly Func<IEnumerable<IMailChimpContact>, IEnumerable<MCMemberInfo>, IEnumerable<MCMemberInfo>> ContactsToCreate =
            (contacts, memberInfos) =>
                MapIContactsToMemberInfos(
                    contacts.Where(contact => contact.Subscribed &&
                                              !memberInfos.Any(info => GetIdFieldFromMergeVars(info) == contact.UserId.ToString()))
                            .Union(contacts.Where(contact => contact.Subscribed)
                                           .Join(memberInfos.Where(info => info.status == Status.subscribed.ToString()),
                                                 contact => contact.UserId.ToString(),
                                                 GetIdFieldFromMergeVars,
                                                 (contact, info) => new
                                                 {
                                                     contact,
                                                     info
                                                 })
                                           .Where(pair => pair.contact.UserId != long.Parse(pair.info.merges[11].val))
                                           .Select(pair => pair.contact)));

        /// <summary>
        /// Contacts to update are the contacts that are subscribed on both datasets and that contain differences in the fields.
        /// </summary>
        private static readonly Func<IEnumerable<IMailChimpContact>, IEnumerable<MCMemberInfo>, IEnumerable<MCMemberInfo>> ContactsToUpdate =
            (contacts, memberInfos) =>
                MapIContactsToMemberInfos(
                    contacts.Where(contact => contact.Subscribed)
                            .Join(memberInfos.Where(info => info.status == Status.subscribed.ToString()),
                                  contact => contact.UserId.ToString(),
                                  GetIdFieldFromMergeVars,
                                  (contact, info) => new { contact, info })
                            .Where(pair => DifferenceInMergeVars(pair.info.merges, pair.contact) &&
                                           pair.contact.UserId == long.Parse(pair.info.merges[11].val))
                            .Select(pair => pair.contact)
                 );

        /// <summary>
        /// Maps IContacts to a memberInfos
        /// </summary>
        private static readonly Func<IEnumerable<IMailChimpContact>, IEnumerable<MCMemberInfo>> MapIContactsToMemberInfos =
            contacts =>
                contacts.Select(contact => new MCMemberInfo
                {
                    email = contact.Email,
                    email_type = "",
                    status = Status.subscribed.ToString(),
                    merges = GetMergeVars(contact)
                });

        /// <summary>
        /// Gets the member ids of the unsubscribed members. members can unsubscribe through the email link
        /// </summary>
        private static readonly Func<IEnumerable<MCMemberInfo>, List<Guid>> UnsubscribedMemberIds =
                infos => infos.Where(info => info.status == Status.unsubscribed.ToString() &&
                                             !string.IsNullOrEmpty(GetIdFieldFromMergeVars(info)))
                              .Select(info => new Guid(GetIdFieldFromMergeVars(info)))
                              .ToList();
        #endregion

        #region Private Methods
        private void UpdateInterestGroups(IEnumerable<string> interests)
        {
            var actualInterests = _wrapper.listInterestGroups(listId);
            interests.Except(actualInterests.groups).ForEach(interest => _wrapper.listInterestGroupAdd(listId, interest));
            actualInterests.groups.Except(interests).ForEach(interest => _wrapper.listInterestGroupDel(listId, interest));
        }

        private IList<MCMemberInfo> GetMailChimpListMembers()
        {
            var chimpMembersUnsubscribed = _wrapper.listMembers(listId, Status.unsubscribed.ToString());
            var chimpMembersSubscribed = _wrapper.listMembers(listId, Status.subscribed.ToString());
            var chimpMembers = chimpMembersSubscribed.Union(chimpMembersUnsubscribed);

            var chimpMemberInfos = new List<MCMemberInfo>();
            foreach (var member in chimpMembers)
            {
                var memberInfo = _wrapper.listMemberInfo(listId, member.email);
                chimpMemberInfos.Add(memberInfo);
            }
            return chimpMemberInfos;
        }

        private static bool DifferenceInMergeVars(IEnumerable<MCMergeVar> merges, IMailChimpContact contact)
        {
            var mergeVars = GetMergeVars(contact);
            var result = true;

            mergeVars.ForEach(var => result = result && merges.Any(merge => merge.name == var.name && merge.val == var.val));

            return !result;
        }

        private static MCMergeVar[] GetMergeVars(IMailChimpContact contact)
        {
            var list = new List<MCMergeVar>
                       {
                               new MCMergeVar { req = true, tag = IdField, val = contact.UserId.ToString() },
                       };

            if (contact.Groups.Count > 0)
            {
                list.Add(new MCMergeVar { req = false, tag = "INTERESTS", val = string.Join(", ", contact.Groups.ToArray()) });
            }

            foreach (var mergeVar in _listColumns)
            {
                if (contact.FieldValues.ContainsKey(mergeVar.name))
                {
                    list.Add(new MCMergeVar
                    {
                        req = mergeVar.req,
                        name = mergeVar.name,
                        tag = mergeVar.tag,
                        val = contact.FieldValues[mergeVar.name] ?? ""
                    });
                }
            }
            return list.ToArray();
        }

        private static string GetIdFieldFromMergeVars(MCMemberInfo info)
        {
            return info.merges.Where(mergevar => mergevar.name == IdField)
                              .Select(mergevar => mergevar.val)
                              .FirstOrDefault();
        }

        private static void UpdateListColumns(string list)
        {
            var actualColumns = _wrapper.listMergeVars(list);
            var columns = _listColumns;
            columns.AddRange(FixedColumns);

            actualColumns.Except(columns).ForEach(item => _wrapper.listMergeVarDel(list, item.tag));
            columns.Except(actualColumns).ForEach(item => _wrapper.listMergeVarAdd(list, item.tag, item.name, item.req));
        }
        #endregion
    }

}