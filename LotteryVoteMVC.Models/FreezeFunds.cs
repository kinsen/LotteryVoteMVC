using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 资金冻结
    /// </summary>
    public class FreezeFunds
    {
        public const string TABLENAME = "tb_FreezeFunds";
        public const string FREEZEID = "FreezeId";
        public const string USERID = "UserId";
        public const string AMOUNT = "Amount";
        public const string STATUS = "Status";
        public const string CREATETIME = "CreateTime";

        public int FreezeId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public BetStatus Status { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
