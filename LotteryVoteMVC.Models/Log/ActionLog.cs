using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 操作日志
    /// </summary>
    public class ActionLog
    {
        public const string TABLENAME = "tb_ActionLog";
        public const string LOGID = "LogId";
        public const string USERID = "UserId";
        public const string TARGETUSERID = "TargetUserId";
        public const string ACTION = "Action";
        public const string DETAIL = "Detail";
        public const string IPADDRESS = "IPAddress";
        public const string CREATETIME = "CreateTime";

        public DataRow DataRow { get; set; }
        public int LogId { get; set; }
        public int UserId { get; set; }
        public int TargetUserId { get; set; }
        public string Action { get; set; }
        public string Detail { get; set; }
        public string IPAddress { get; set; }
        public DateTime CreateTime { get; set; }

        private User _target;
        public User TargetUser
        {
            get
            {
                if (_target == null && DataRow != null)
                    _target = ModelParser<User>.ParseModel(DataRow);
                return _target;
            }
            set
            {
                _target = value;
            }
        }
    }
}
