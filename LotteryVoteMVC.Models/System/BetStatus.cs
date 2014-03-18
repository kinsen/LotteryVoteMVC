using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 注单状态
    /// </summary>
    public enum BetStatus
    {
        /// <summary>
        /// 失效
        /// </summary>
        Invalid = 0,
        /// <summary>
        /// 有效
        /// </summary>
        Valid = 1,
        /// <summary>
        /// 已结算
        /// </summary>
        Settled = 2
    }
}
