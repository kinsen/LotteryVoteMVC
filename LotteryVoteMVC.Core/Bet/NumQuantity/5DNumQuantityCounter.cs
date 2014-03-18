using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Core.Bet
{
    internal class FiveDigitNumQuantityCounter : DigitNumQuantityCounter
    {
        protected override void CheckNumLength(string num)
        {
            if (num.Length != 5)
                throw new ArgumentException("num's length not equals 5");
        }

        protected override int GetHead()
        {
            throw new NotImplementedException();
        }

        protected override int GetRoll()
        {
            return CompanySupportNumLen.Where(it => it.NumLen.Length >= 5).Sum(it => it.Count);
        }

        public override string GetRealyBetNum(string num)
        {
            if (num.Length > 5)
                return num.Substring(num.Length - 5);
            else
            {
                int n = int.Parse(num);
                return n.ToString("D5");
            }
        }
    }
}
