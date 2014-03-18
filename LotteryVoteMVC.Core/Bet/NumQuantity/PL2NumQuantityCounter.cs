using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core.Bet
{
    internal class PL2NumQuantityCounter : NumQuantityCounter
    {
        public override int CountNumQuantity(string num, PlayWay playWay)
        {
            if (!num.IsPL2())
                throw new ArgumentException(string.Format("num:{0} not a pl2 number", num));
            if (this.CompanySupportNumLen == null || this.CompanySupportNumLen.Count == 0)
                throw new ApplicationException("公司支持号码长度不能为空!");
            //var nums = num.Split('#');
            //if (nums[0] == nums[1])
            //    throw new ArgumentException("包组过关2应该使用两个不同数字!");
            return CompanySupportNumLen.Sum(it => it.Count) * 2;
        }

        public override string GetRealyBetNum(string num)
        {
            return num;
        }
    }
}
