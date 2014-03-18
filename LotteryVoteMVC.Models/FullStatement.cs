using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class FullStatement
    {
        public const string PROCEDURE = "CalcFullStatement";
        public const string USERID = "UserId";
        public const string USERNAME = "UserName";
        public const string ORDERCOUNT = "OrderCount";
        public const string BETTURNOVER = "BetTurnover";
        public const string PARENTCOMMISSION = "ParentCommission";
        public const string TOTALCOMMISSION = "TotalCommission";
        public const string WINLOST = "WinLost";

        public int UserId { get; set; }
        public string UserName { get; set; }
        public int OrderCount { get; set; }
        public decimal BetTurnover { get; set; }
        public decimal ParentCommission { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal WinLost { get; set; }
    }
}
