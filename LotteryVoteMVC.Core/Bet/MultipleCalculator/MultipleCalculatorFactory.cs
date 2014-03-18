using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Text.RegularExpressions;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Core.Bet
{
    internal class MultipleCalculatorFactory
    {
        private static MultipleCalculatorFactory _factory;
        private static object _lockHelper = new object();
        private MultipleCalculatorFactory() { }

        private IDictionary<int, MultipleCalculator> _calcDic;
        protected IDictionary<int, MultipleCalculator> CalcDic
        {
            get
            {
                if (_calcDic == null)
                {
                    lock (_lockHelper)
                    {
                        if (_calcDic == null)
                            _calcDic = new Dictionary<int, MultipleCalculator>();
                    }
                }
                return _calcDic;
            }
        }

        public static MultipleCalculatorFactory GetFactory()
        {
            if (_factory == null)
            {
                lock (_lockHelper)
                {
                    if (_factory == null)
                    {
                        _factory = new MultipleCalculatorFactory();
                    }
                }
            }
            return _factory;
        }

        public MultipleCalculator BuildCalculator(BetOrder order)
        {
            if (!CalcDic.ContainsKey(order.GamePlayWayId))
                CalcDic.Add(order.GamePlayWayId, CreateMultipleCalculator(order.GamePlayWayId));
            return CalcDic[order.GamePlayWayId];
        }
        private MultipleCalculator CreateMultipleCalculator(int gpwId)
        {
            MultipleCalculator calculator;
            var gpw = LotterySystem.Current.FindGamePlayWay(gpwId);
            if (gpw == null)
                throw new ApplicationException("找不到游戏玩法!GPWId:" + gpwId);
            switch (gpw.GameType)
            {
                case GameType.TwoDigital: calculator = new TwoDigitMultipleCalculator(); break;
                case GameType.ThreeDigital: calculator = new ThreeDigitMultipleCalculator(); break;
                case GameType.FourDigital: calculator = new FourDigitMultipleCalculator(); break;
                case GameType.FiveDigital: calculator = new FiveDigitMultipleCalculator(); break;
                case GameType.PL2: calculator = new PL2MultipleCalculator(); break;
                case GameType.PL3: calculator = new PL3MultipleCalculator(); break;
                case GameType.A_B_PL2:
                case GameType.C_A_PL2:
                case GameType.B_C_PL2: calculator = new A_BPL2MultipleCalculator(); break;
                default: throw new ApplicationException("Invalid digit!");
            }
            return calculator;
        }
        [Obsolete]
        private MultipleCalculator CreateMultipleCalculator(string num)
        {
            //TODO:可能存在多线程操作问题
            MultipleCalculator calculator;
            int numLen = num.Length;
            switch (numLen)
            {
                case 2: calculator = new TwoDigitMultipleCalculator(); break;
                case 3: calculator = new ThreeDigitMultipleCalculator(); break;
                case 4: calculator = new FourDigitMultipleCalculator(); break;
                case 5:
                    if (new Regex(@"^\d{5}$").IsMatch(num))
                        calculator = new FiveDigitMultipleCalculator();
                    else
                        calculator = new PL2MultipleCalculator();
                    break;
                case 8: calculator = new PL3MultipleCalculator(); break;
                default: throw new ApplicationException("Invalid digit!");
            }
            return calculator;
        }
    }
}
