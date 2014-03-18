using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 玩法
    /// </summary>
    public enum PlayWay
    {
        /// <summary>
        /// 头
        /// </summary>
        Head = 1,
        /// <summary>
        /// 尾
        /// </summary>
        Last = 2,
        /// <summary>
        /// 头尾
        /// </summary>
        HeadAndLast = 3,
        /// <summary>
        /// 包组
        /// </summary>
        Roll = 4,
        /// <summary>
        /// 包组7
        /// </summary>
        Roll7 = 5
    }
}
