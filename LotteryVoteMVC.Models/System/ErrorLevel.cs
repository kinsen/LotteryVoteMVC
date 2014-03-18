using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public enum ErrorLevel
    {
        /// <summary>
        /// 系统错误
        /// </summary>
        Application = 1,
        /// <summary>
        /// 数据错误
        /// </summary>
        InvalidData = 2,
        /// <summary>
        /// 用户错误
        /// </summary>
        User = 3
    }
}
