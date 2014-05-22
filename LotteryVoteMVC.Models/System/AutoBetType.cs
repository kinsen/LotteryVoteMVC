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
        /// <summary>
        /// 00-49
        /// </summary>
        Small = 6,
        /// <summary>
        /// 50-99
        /// </summary>
        Big = 7,
        /// <summary>
        /// 30-79
        /// </summary>
        Center = 8,
        /// <summary>
        /// 单头（十位是奇数）
        /// </summary>
        OddHead = 9,
        /// <summary>
        /// 双头（十位是偶数）
        /// </summary>
        EvenHead = 10,
        /// <summary>
        /// 单尾（奇数）
        /// </summary>
        OddLast = 11,
        /// <summary>
        /// 双尾（偶数）
        /// </summary>
        EvenLast = 12
    }
}
