using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using CookComputing.XmlRpc;

namespace MailChimp
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DummyMCMemberInfo
    {
        public string email;
        public string email_type;
        public object merges;
        public string status;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCApiKey
    {
        public string apikey;
        public string created_at;
        public string expired_at;
    }

    [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MCMergeVar
    {
        [XmlRpcMissingMapping(MappingAction.Error)]
        public string tag;
        public string name;
        public bool req;
        public string val;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCBatchResult
    {
        public int success_count;
        public int error_count;
        public MCEmailResult[] errors;
    }

    [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MCEmailResult
    {
        public int code;
        public string message;
        public MCMemberInfo row;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCMemberInfo
    {
        public string email;
        public string email_type;
        public MCMergeVar[] merges;
        public string status;
        [XmlRpcMissingMapping(MappingAction.Ignore)]
        public string timestamp;
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct DummyMCBatchResult
    {
        public int success_count;
        public int error_count;
        public DummyMCEmailResult[] errors;
    }


    [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct DummyMCEmailResult
    {
        public int code;
        public string message;
        public object row;
        public string email;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCInterestGroups
    {
        public string name;
        public string form_field;
        public string[] groups;
    }

    [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MCListMember
    {
        public string email;
        public string timestamp;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCList
    {
        public string id;
        public int web_id;
        public string name;
        public string date_created;
        public double member_count;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MCCampaign
    {
        public string id;
        public string web_id;
        public string list_id;
        public double folder_id;
        public string title;
        public string type;
        public string create_time;
        public string send_time;
        public int emails_sent;
        public string status;
        public string from_name;
        public string from_email;
        public string subject;
        public string to_email;
        public string archive_url;
        public string inline_css;
    }

    [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MCSegmentCond
    {
        public string field;
        public string op;
        public string value;
    }

 
   [StructLayout(LayoutKind.Sequential), XmlRpcMissingMapping(MappingAction.Ignore)]
    public struct MCSegmentOpts
    {
        public string match;
        public MCSegmentCond[] conditions;
    }


 

 






 



}
