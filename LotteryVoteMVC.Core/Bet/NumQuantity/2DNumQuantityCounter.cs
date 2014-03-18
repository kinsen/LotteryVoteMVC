using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class TwoDigitNumQuantityCounter : DigitNumQuantityCounter
    {
        protected override int GetHead()
        {
            var suportNum = CompanySupportNumLen.Find(it => it.NumLen.Length == 2);
            if (suportNum == null)
                throw new ApplicationException("该公司类型不支持2D号码!");
            //LogConsole.Info("GetHead:" + suportNum.CompanyType);
            return suportNum.Count;
        }

        protected override void CheckNumLength(string num)
        {
            if (num.Length != 2)
            {
                throw new ArgumentException("num's length not equals 2!Num:" + num);
            }
        }


        public override string GetRealyBetNum(string num)
        {
            if (num.Length > 2)
                return num.Substring(num.Length - 2);
            else
            {
                int n = int.Parse(num);
                return n.ToString("D2");
            }
        }
    }
}
