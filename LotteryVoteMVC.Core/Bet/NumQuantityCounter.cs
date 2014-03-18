using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    public abstract class NumQuantityCounter
    {
        /// <summary>
        /// 公司支持号码长度.
        /// </summary>
        /// <value>
        /// The company support num len.
        /// </value>
        public IList<CompanyTypeSupportNumLen> CompanySupportNumLen { get; set; }
        /// <summary>
        /// 计算号码数量.
        /// </summary>
        /// <param name="num">号码.</param>
        /// <param name="isFullPermutation">是否全排列下注（化）.</param>
        /// <param name="playWay">玩法.</param>
        /// <returns></returns>
        public abstract int CountNumQuantity(string num, PlayWay playWay);
        /// <summary>
        /// 获取真实投注的号码，例如1234，在2D下是34，3D下是234
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public abstract string GetRealyBetNum(string num);

        protected int CountFullPermutation(string numStr, int quantity)
        {
            int num = int.Parse(numStr);
            return (int)MathHelper.CountPermutation(num) * quantity;
        }
    }
}
