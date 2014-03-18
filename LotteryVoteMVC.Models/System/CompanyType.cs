using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 公司类型
    /// </summary>
    public enum CompanyType
    {
        /// <summary>
        /// 荷来
        /// </summary>
        [EnumDescription("Hanoi")]
        Hanoi = 1,
        /// <summary>
        /// 18A
        /// </summary>
        [EnumDescription("18A")]
        EighteenA = 2,
        /// <summary>
        /// 18B
        /// </summary>
        [EnumDescription("18B")]
        EighteenB = 3,
        /// <summary>
        /// 18C
        /// </summary>
        [EnumDescription("18C")]
        EighteenC = 4,
        /// <summary>
        /// 18D
        /// </summary>
        [EnumDescription("18D")]
        EighteenD = 5
    }
}
