using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CookComputing.XmlRpc;
using System.Collections;


namespace MailChimp
{
    public class ApiWrapper
    {
        // Fields
        private IMC api = XmlRpcProxyGen.Create<IMC>();
        private string apikey;
        private string datacenter;
        private bool secure;

        // Methods
        public ApiWrapper()
        {
            this.api.UserAgent = "MailChimp.Net/1.3";
            this.api.Timeout = 0x15f90;
            this.secure = false;
            this.datacenter = "us1";
            this.buildEndPoint();
        }

        public string apikeyAdd(string username, string password, string apikey)
        {
            return this.api.apikeyAdd(username, password, apikey);
        }

        public bool apikeyExpire(string username, string password, string apikey)
        {
            return this.api.apikeyExpire(username, password, apikey);
        }

        public MCApiKey[] apikeys(string username, string password, string apikey)
        {
            return this.apikeys(username, password, apikey, false);
        }

        public MCApiKey[] apikeys(string username, string password, string apikey, bool expired)
        {
            return this.api.apikeys(username, password, apikey, expired);
        }

        private void buildEndPoint()
        {
            string str;
            if (this.secure)
            {
                str = "https://";
            }
            else
            {
                str = "http://";
            }
            this.api.Url = str + this.datacenter + ".api.mailchimp.com/1.1/";
        }


        public bool listSubscribe(string id, string email_address, MCMergeVar[] merges, string email_type)
        {
            return this.listSubscribe(id, email_address, merges, email_type, true);
        }

        public bool listSubscribe(string id, string email_address, MCMergeVar[] merges, string email_type, bool double_optin)
        {
            XmlRpcStruct mv = this.mergeArrayToStruct(merges);
            return this.api.listSubscribe(this.apikey, id, email_address, mv, email_type, double_optin);
        }

        public bool listUnsubscribe(string id, string email_address)
        {
            return this.listUnsubscribe(id, email_address, false);
        }

        public bool listUnsubscribe(string id, string email_address, bool delete_member)
        {
            return this.listUnsubscribe(id, email_address, delete_member, true);
        }

        public bool listUnsubscribe(string id, string email_address, bool delete_member, bool send_goodbye)
        {
            return this.listUnsubscribe(id, email_address, delete_member, send_goodbye, true);
        }

        public bool listUnsubscribe(string id, string email_address, bool delete_member, bool send_goodbye, bool send_notify)
        {
            return this.api.listUnsubscribe(this.apikey, id, email_address, delete_member, send_goodbye, send_notify);
        }

        public bool listUpdateMember(string listId, string email_address, MCMergeVar[] merges)
        {
            return this.listUpdateMember(listId, email_address, merges, "html");
        }

        public bool listUpdateMember(string listId, string email_address, MCMergeVar[] merges, string email_type)
        {
            return this.listUpdateMember(listId, email_address, merges, email_type, true);
        }

        public bool listUpdateMember(string listId, string email_address, MCMergeVar[] merges, string email_type, bool replace_interests)
        {
            XmlRpcStruct mv = this.mergeArrayToStruct(merges);
            return this.api.listUpdateMember(this.apikey, listId, email_address, mv, email_type, replace_interests);
        }

        public MCBatchResult listBatchSubscribe(string id, MCMemberInfo[] batch)
        {
            return this.listBatchSubscribe(id, batch, true);
        }

        public MCBatchResult listBatchSubscribe(string id, MCMemberInfo[] batch, bool double_optin)
        {
            return this.listBatchSubscribe(id, batch, double_optin, false);
        }

        public MCBatchResult listBatchSubscribe(string id, MCMemberInfo[] batch, bool double_optin, bool update_existing)
        {
            return this.listBatchSubscribe(id, batch, double_optin, update_existing, true);
        }

        public MCBatchResult listBatchSubscribe(string id, MCMemberInfo[] batch, bool double_optin, bool update_existing, bool replace_interests)
        {
            XmlRpcStruct struct2 = new XmlRpcStruct();
            int index = 0;
            foreach (MCMemberInfo info in batch)
            {
                XmlRpcStruct struct3 = new XmlRpcStruct();
                struct3.Add("EMAIL", info.email);
                struct3.Add("EMAIL_TYPE", info.email_type);
                foreach (MCMergeVar var in info.merges)
                {
                    struct3.Add(var.tag, var.val);
                }
                struct2.Add(index + string.Empty, struct3);
                index++;
            }
            DummyMCBatchResult result = this.api.listBatchSubscribe(this.apikey, id, struct2, double_optin, update_existing, replace_interests);
            MCBatchResult result2 = new MCBatchResult
            {
                success_count = result.success_count,
                error_count = result.error_count,
                errors = new MCEmailResult[result.errors.Length]
            };
            index = 0;
            foreach (DummyMCEmailResult result3 in result.errors)
            {
                MCEmailResult result4 = new MCEmailResult
                {
                    code = result3.code,
                    message = result3.message
                };
                XmlRpcStruct row = (XmlRpcStruct)result3.row;
                MCMemberInfo info2 = new MCMemberInfo
                {
                    merges = new MCMergeVar[row.Count]
                };
                int num5 = 0;
                IEnumerator enumerator = row.Keys.GetEnumerator();
                try
                {
                    while (enumerator.MoveNext())
                    {
                        string current = (string)enumerator.Current;
                        switch (current)
                        {
                            case "EMAIL":
                                info2.email = (string)row[current];
                                break;

                            case "EMAIL_TYPE":
                                info2.email_type = (string)row[current];
                                break;
                        }
                        info2.merges[num5] = new MCMergeVar();
                        info2.merges[num5].tag = current;
                        info2.merges[num5].val = (string)row[current];
                        num5++;
                    }
                }
                finally
                {
                    try
                    {
                        IDisposable disposable1 = enumerator as IDisposable;
                        if (disposable1 == null)
                        {
                        }
                        disposable1.Dispose();
                    }
                    catch { }
                }
                result4.row = info2;
                result2.errors[index] = result4;
                index++;
            }
            return result2;
        }


        public MCBatchResult listBatchUnsubscribe(string id, MCMemberInfo[] batch)
        {
            return this.listBatchUnsubscribe(id, batch, false);
        }

        public MCBatchResult listBatchUnsubscribe(string id, MCMemberInfo[] batch, bool delete_member)
        {
            return this.listBatchUnsubscribe(id, batch, delete_member, true);
        }

        public MCBatchResult listBatchUnsubscribe(string id, MCMemberInfo[] batch, bool delete_member, bool send_goodbye)
        {
            return this.listBatchUnsubscribe(id, batch, delete_member, send_goodbye, false);
        }

        public MCBatchResult listBatchUnsubscribe(string id, MCMemberInfo[] batch, bool delete_member, bool send_goodbye, bool send_notify)
        {
            XmlRpcStruct struct2 = new XmlRpcStruct();
            int index = 0;
            foreach (MCMemberInfo info in batch)
            {
                struct2.Add(index + string.Empty, info.email);
                index++;
            }
            DummyMCBatchResult result = this.api.listBatchUnsubscribe(this.apikey, id, struct2, delete_member, send_goodbye, send_notify);
            MCBatchResult result2 = new MCBatchResult
            {
                success_count = result.success_count,
                error_count = result.error_count,
                errors = new MCEmailResult[result.errors.Length]
            };
            index = 0;
            foreach (DummyMCEmailResult result3 in result.errors)
            {
                MCEmailResult result4 = new MCEmailResult
                {
                    code = result3.code,
                    message = result3.message
                };
                MCMemberInfo info2 = new MCMemberInfo();
                result4.row = info2;
                result4.row.email = result3.email;
                result2.errors[index] = result4;
                index++;
            }
            return result2;
        }
       
        private XmlRpcStruct mergeArrayToStruct(MCMergeVar[] merges)
        {
            XmlRpcStruct struct2 = new XmlRpcStruct();
            foreach (MCMergeVar var in merges)
            {
                if (var.tag != null)
                {
                    struct2.Add(var.tag, var.val);
                }
            }
            return struct2;
        }

        public bool listInterestGroupAdd(string listId, string group_name)
        {
            return this.api.listInterestGroupAdd(this.apikey, listId, group_name);
        }

        public bool listInterestGroupDel(string listId, string group_name)
        {
            return this.api.listInterestGroupDel(this.apikey, listId, group_name);
        }


        public MCInterestGroups listInterestGroups(string listId)
        {
            return this.api.listInterestGroups(this.apikey, listId);
        }


        public MCListMember[] listMembers(string id)
        {
            return this.listMembers(id, "subscribed");
        }

        public MCListMember[] listMembers(string id, string status)
        {
            return this.listMembers(id, status, 0, 100);
        }

        public MCListMember[] listMembers(string id, string status, int start, int limit)
        {
            try
            {
                return this.api.listMembers(this.apikey, id, status, start, limit);
            }
            catch (XmlRpcTypeMismatchException)
            {
                return new MCListMember[0];
            }
        }

        public MCMemberInfo listMemberInfo(string listId, string email_address)
        {
            DummyMCMemberInfo info = this.api.listMemberInfo(this.apikey, listId, email_address);
            XmlRpcStruct merges = (XmlRpcStruct)info.merges;
            MCMergeVar[] varArray = new MCMergeVar[merges.Count];
            MCMemberInfo info2 = new MCMemberInfo
            {
                email = info.email,
                email_type = info.email_type,
                status = info.status,
                timestamp = info.timestamp
            };
            int index = 0;
            IEnumerator enumerator = merges.Keys.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    string current = (string)enumerator.Current;
                    varArray[index] = new MCMergeVar { tag = current, val = (string)merges[current] };
                    index++;
                }
            }
            finally
            {
                try
                {
                    IDisposable disposable1 = enumerator as IDisposable;
                    if (disposable1 == null)
                    {
                    }
                    disposable1.Dispose();
                }
                catch { }
            }
            info2.merges = varArray;
            return info2;
        }


        public bool listMergeVarAdd(string listId, string tag, string name)
        {
            return this.listMergeVarAdd(listId, tag, name, false);
        }

        public bool listMergeVarAdd(string listId, string tag, string name, bool req)
        {
            return this.api.listMergeVarAdd(this.apikey, listId, tag, name, req);
        }


        public bool listMergeVarAddOptions(string listId, string tag, string name, XmlRpcStruct options)
        {
            return this.api.listMergeVarAddOptions(this.apikey, listId, tag, name, options);
        }

        public bool listMergeVarDel(string listId, string tag)
        {
            return this.api.listMergeVarDel(this.apikey, listId, tag);
        }

        public MCMergeVar[] listMergeVars(string listId)
        {
            return this.api.listMergeVars(this.apikey, listId);
        }


        public MCList[] lists()
        {
            try
            {
                return this.api.lists(this.apikey);
            }
            catch (XmlRpcTypeMismatchException)
            {
                return new MCList[0];
            }
        }


        public void setCurrentApiKey(string apikey)
        {
            string str = string.Empty;
            string str2 = "us1";
            int num = 0;
            char[] separator = new char[] { '-' };
            foreach (string str3 in apikey.Split(separator))
            {
                switch (num)
                {
                    case 0:
                        str = str3;
                        break;

                    case 1:
                        str2 = str3;
                        break;
                }
                num++;
            }
            this.datacenter = str2;
            this.apikey = str;
            this.buildEndPoint();
        }



        public int campaignSegmentTest(string id, MCSegmentOpts segment_opts)
        {
            return this.api.campaignSegmentTest(this.apikey, id, segment_opts);
        }

        public bool campaignSendNow(string cid)
        {
            return this.api.campaignSendNow(this.apikey, cid);
        }



        public string ping()
        {
            return this.api.ping(this.apikey);
        }
    }


    [XmlRpcUrl("http://api.mailchimp.com/1.1/")]
    public interface IMC : IXmlRpcProxy
    {
        // Methods
        [XmlRpcMethod("apikeyAdd")]
        string apikeyAdd(string username, string password, string apikey);
        [XmlRpcMethod("apikeyExpire")]
        bool apikeyExpire(string username, string password, string apikey);
        [XmlRpcMethod("apikeys")]
        MCApiKey[] apikeys(string username, string password, string apikey, bool expired);
        [XmlRpcMethod("lists")]
        MCList[] lists(string apikey);
        [XmlRpcMethod("listSubscribe")]
        bool listSubscribe(string apikey, string id, string email_address, Hashtable mv, string email_type, bool double_optin);
        [XmlRpcMethod("listUnsubscribe")]
        bool listUnsubscribe(string apikey, string id, string email_address, bool delete_member, bool send_goodbye, bool send_notify);
        [XmlRpcMethod("listBatchSubscribe")]
        DummyMCBatchResult listBatchSubscribe(string apikey, string id, Hashtable batch, bool double_optin, bool update_existing, bool replace_interests);
        [XmlRpcMethod("listBatchUnsubscribe")]
        DummyMCBatchResult listBatchUnsubscribe(string apikey, string id, Hashtable batch, bool delete_member, bool send_goodbye, bool send_notify);
        [XmlRpcMethod("listInterestGroupAdd")]
        bool listInterestGroupAdd(string apikey, string id, string group_name);
        [XmlRpcMethod("listInterestGroupDel")]
        bool listInterestGroupDel(string apikey, string id, string group_name);
        [XmlRpcMethod("listInterestGroups")]
        MCInterestGroups listInterestGroups(string apikey, string id);
        [XmlRpcMethod("listMembers")]
        MCListMember[] listMembers(string apikey, string id, string status, int start, int limit);
        [XmlRpcMethod("listMemberInfo")]
        DummyMCMemberInfo listMemberInfo(string apikey, string id, string email);
        [XmlRpcMethod("listMergeVarAddOptions")]
        bool listMergeVarAddOptions(string apikey, string id, string tag, string name, XmlRpcStruct options);
        [XmlRpcMethod("listMergeVarAdd")]
        bool listMergeVarAdd(string apikey, string id, string tag, string name, bool req);
        [XmlRpcMethod("listMergeVarDel")]
        bool listMergeVarDel(string apikey, string id, string tag);
        [XmlRpcMethod("listMergeVars")]
        MCMergeVar[] listMergeVars(string apikey, string id);
        [XmlRpcMethod("listUpdateMember")]
        bool listUpdateMember(string apikey, string id, string email, Hashtable mv, string email_type, bool replace_interests);
        [XmlRpcMethod("campaignUpdate")]
        bool campaignUpdateMCSO(string apikey, string cid, string name, MCSegmentOpts segment_opts);
        [XmlRpcMethod("campaignSegmentTest")]
        int campaignSegmentTest(string apikey, string id, MCSegmentOpts segment_opts);
        [XmlRpcMethod("campaignSendNow")]
        bool campaignSendNow(string apikey, string cid);



 

        [XmlRpcMethod("ping")]
        string ping(string apikey);
    }




}

 

 


 
 


 
