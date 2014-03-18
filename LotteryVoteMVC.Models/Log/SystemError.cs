using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class SystemError
    {
        public const string TABLENAME = "tb_SystemError";
        public const string ID = "Id";
        public const string ERRORDATE = "ErrorDate";
        public const string ERRORLEVEL = "ErrorLevel";
        public const string ERRORMESSAGE = "ErrorMessage";
        public const string STACKTRACK = "StackTrack";
        public const string PAGEURL = "PageUrl";
        public const string REMARKS = "Remarks";
        public const string IPFIELD = "IP";

        public int Id { get; set; }
        public DateTime ErrorDate { get; set; }
        public ErrorLevel ErrorLevel { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrack { get; set; }
        public string PageUrl { get; set; }
        public string Remarks { get; set; }
        public string IP { get; set; }
    }
}
