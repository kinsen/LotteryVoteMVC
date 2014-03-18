using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class LoginFailedLog
    {
        public const string TABLENAME = "tb_LoginFailedLog";
        public const string LOGID = "LogId";
        public const string USERID = "UserId";
        public const string IPField = "IP";
        public const string CREATEDATE = "CreateDate";

        public int LogId { get; set; }
        public int UserId { get; set; }
        public string IP { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
