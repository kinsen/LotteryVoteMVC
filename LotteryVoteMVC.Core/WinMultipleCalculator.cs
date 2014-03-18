using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Bet;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 赢倍数计算器
    /// </summary>
    public class WinMultipleCalculator
    {
        private static WinMultipleCalculator _calc;
        private static object _lockHelper = new object();

        private WinMultipleCalculator() { }

        /// <summary>
        /// 获取输赢倍数计算器
        /// </summary>
        /// <returns></returns>
        public static WinMultipleCalculator GetCalculator()
        {
            if (_calc == null)
            {
                lock (_lockHelper)
                {
                    if (_calc == null)
                        _calc = new WinMultipleCalculator();
                }
            }
            return _calc;
        }

        /// <summary>
        /// 根据注单计算.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        public int Calculate(BetOrder order)
        {
            var calculator = MultipleCalculatorFactory.GetFactory().BuildCalculator(order);
            return calculator.Calculate(order);
        }

        public void ResetAllCalculate()
        { 
        }
    }
}
