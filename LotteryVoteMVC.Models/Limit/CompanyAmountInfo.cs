using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class CompanyAmountInfo
    {
        public LotteryCompany Company { get; set; }
        public decimal CompanyBetAmount { get; set; }
        public IDictionary<GamePlayWay, decimal> GamePlayWayBetAmount { get; set; }
    }
}
