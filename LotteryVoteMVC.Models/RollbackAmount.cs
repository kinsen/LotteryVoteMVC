using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class RollbackAmount
    {
        public const string TABLENAME = "tb_RollbackAmount";
        public const string ID = "Id";
        public const string USERID = "UserId";
        public const string AMOUNT = "Amount";
        public const string TIMETOKEN = "TimeToken";

        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string TimeToken { get; set; }
    }
}
