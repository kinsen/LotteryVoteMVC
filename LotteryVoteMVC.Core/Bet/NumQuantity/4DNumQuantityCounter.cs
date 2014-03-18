using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Core.Bet
{
    internal class FourDigitNumQuantityCounter : DigitNumQuantityCounter
    {
        protected override void CheckNumLength(string num)
        {
            if (num.Length != 4)
                throw new ArgumentException("num's length not equals 4");
        }

        protected override int GetHead()
        {
            throw new NotSupportedException();
        }
        protected override int GetHeadAndLast()
        {
            throw new NotSupportedException();
        }
        protected override int GetRoll()
        {
            return CompanySupportNumLen.Where(it => it.NumLen.Length >= 4).Sum(it => it.Count);
        }

        public override string GetRealyBetNum(string num)
        {
            if (num.Length > 4)
                return num.Substring(num.Length - 4);
            else
            {
                int n = int.Parse(num);
                return n.ToString("D4");
            }
        }
    }
}
