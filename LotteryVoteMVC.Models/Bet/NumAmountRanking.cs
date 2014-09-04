using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    public class NumAmountRanking
    {
        public const string USERID = "UserId";
        public const string USERNAME = "UserName";
        public const string AMOUNT = "Amount";

        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Amount { get; set; }

        public NumAmountRanking() { }

        public NumAmountRanking(DataRow row)
        {
            this.UserId = row.Field<int>(USERID);
            this.UserName = row.Field<string>(USERNAME);
            this.Amount = row.Field<decimal>(AMOUNT);
        }
    }
}
