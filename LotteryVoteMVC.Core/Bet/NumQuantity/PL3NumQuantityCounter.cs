using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class PL3NumQuantityCounter : NumQuantityCounter
    {
        public override int CountNumQuantity(string num, PlayWay playWay)
        {
            if (!num.IsPL3())
                throw new ArgumentException(string.Format("num:{0} not a pl3 number", num));
            if (this.CompanySupportNumLen == null || this.CompanySupportNumLen.Count == 0)
                throw new ApplicationException("公司支持号码长度不能为空!");
            //var nums = num.Split('#');
            //foreach (var n in nums)
            //{
            //    if (nums.Count(it => it == n) > 1)
            //        throw new ArgumentException("包组过关3应该使用3个不相同的号码!");
            //}
            return CompanySupportNumLen.Sum(it => it.Count) * 3;
        }
        public override string GetRealyBetNum(string num)
        {
            return num;
        }
    }
}
