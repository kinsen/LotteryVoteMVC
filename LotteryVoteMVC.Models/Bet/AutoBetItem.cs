using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class AutoBetItem
    {
        public AutoBetType BetType { get; set; }
        public int[] CompanyList { get; set; }
        public IList<KeyValuePair<PlayWay, decimal>> WagerList { get; set; }
    }
}
