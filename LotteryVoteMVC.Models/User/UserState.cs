using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 用户状态
    /// </summary>
    public enum UserState
    {
        /// <summary>
        /// 正常
        /// </summary>
        Active = 0,
        /// <summary>
        /// 暂停
        /// </summary>
        Suspended = 1
        /// <summary>
        /// 未激活
        /// </summary>
        //Inactive = 2
    }
}
