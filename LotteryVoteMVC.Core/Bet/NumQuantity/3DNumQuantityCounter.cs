using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class ThreeDigitNumQuantityCounter : DigitNumQuantityCounter
    {

        protected override int GetHead()
        {
            var suportNum = CompanySupportNumLen.Find(it => it.NumLen.Length == 3);
            if (suportNum == null)
                throw new ApplicationException("该公司类型不支持3D号码!");
            return suportNum.Count;
        }

        protected override void CheckNumLength(string num)
        {
            if (num.Length != 3)
                throw new ArgumentException("num's length not equals 3");
        }

        protected override int GetRoll()
        {
            var numLenOf2D = CompanySupportNumLen.Find(it => it.NumLen.Length == 2);
            if (numLenOf2D == null)
                throw new ApplicationException("该公司类型不支持2D号码!");
            return base.GetRoll() - numLenOf2D.Count;
        }

        public override string GetRealyBetNum(string num)
        {
            if (num.Length > 3)
                return num.Substring(num.Length - 3);
            else
            {
                int n = int.Parse(num);
                return n.ToString("D3");
            }
        }
    }
}
