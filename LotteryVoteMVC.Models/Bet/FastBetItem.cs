using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 快速下注项
    /// </summary>
    public class FastBetItem
    {
        public string[] NumList { get; set; }
        public int[] Companys { get; set; }
        public int[] GamePlayWays { get; set; }
        public decimal Wager { get; set; }
        public bool IsFullPermutation { get; set; }
    }
}
