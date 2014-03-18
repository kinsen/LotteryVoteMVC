using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public enum Role
    {
        /// <summary>
        /// 公司用户
        /// </summary>
        Company = 1,
        /// <summary>
        /// 超级总代用户
        /// </summary>
        Super = 2,
        /// <summary>
        /// 总代用户
        /// </summary>
        Master = 3,
        /// <summary>
        /// 代理用户
        /// </summary>
        Agent = 4,
        /// <summary>
        /// 会员用户
        /// </summary>
        Guest = 5,
        /// <summary>
        /// 影子用户
        /// </summary>
        Shadow = 6,
        /// <summary>
        /// 管理用户，用户对整个系统做配置使用
        /// </summary>
        Manager = 7
    }
}
