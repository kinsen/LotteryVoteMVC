using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class LoginLog
    {
        public const string TABLENAME = "tb_LoginLog";
        public const string LOGINID = "LoginId";
        public const string USERID = "UserId";
        public const string IPFIELD = "IP";
        public const string LASTLOGINTIME = "LastLoginTime";

        public int LoginId { get; set; }
        public int UserId { get; set; }
        public string IP { get; set; }
        public DateTime LastLoginTime { get; set; }
    }
}
