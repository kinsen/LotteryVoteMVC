using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 自动下注（12生肖，单单，单双，双单，双双）
    /// </summary>
    public enum AutoBetType
    {
        /// <summary>
        /// 十二生肖
        /// </summary>
        TwelveZodiac = 1,
        /// <summary>
        /// 单单
        /// </summary>
        OddOdd = 2,
        /// <summary>
        /// 单双
        /// </summary>
        OddEven = 3,
        /// <summary>
        /// 双单
        /// </summary>
        EvenOdd = 4,
        /// <summary>
        /// 双双
        /// </summary>
        EvenEven = 5,
        Small = 6,
        Big = 7
    }
}
