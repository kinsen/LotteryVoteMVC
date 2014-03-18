using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 号码信息
    /// </summary>
    public class BetNumInfo
    {
        public string Num { get; set; }
        public decimal Amount { get; set; }
        /// <summary>
        /// 号码数据内容
        /// CompanyId,<GameplayWayId,Amount>
        /// </summary>
        /// <value>
        /// The contents.
        /// </value>
        public IDictionary<int, List<BetNumContent>> Contents { get; set; }
    }
    /// <summary>
    /// 号码数据内容
    /// </summary>
    public class BetNumContent
    {
        public int GamePlayWayId { get; set; }
        public decimal Amount { get; set; }
    }
}
