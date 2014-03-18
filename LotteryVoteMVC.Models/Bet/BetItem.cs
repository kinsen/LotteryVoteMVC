using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class BetItem
    {
        /// <summary>
        /// 号码.
        /// </summary>
        /// <value>
        /// The num.
        /// </value>
        public string Num { get; set; }
        /// <summary>
        /// 下注所在的公司.
        /// </summary>
        /// <value>
        /// The company list.
        /// </value>
        public int[] CompanyList { get; set; }
        /// <summary>
        /// 赌注
        /// </summary>
        /// <value>
        /// The warger list.
        /// </value>
        public List<WagerItem> WargerList { get; set; }
    }
    public class WagerItem
    {
        public int GamePlayTypeId { get; set; }
        public decimal Wager { get; set; }
        public bool IsFullPermutation { get; set; }
    }
}
