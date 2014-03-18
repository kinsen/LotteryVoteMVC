using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core.Bet
{
    internal class NumQuantityCounterFactory
    {
        private static object _lockHelper = new object();
        private static NumQuantityCounterFactory _factory;

        private IDictionary<string, NumQuantityCounter> _counterDic;
        internal IDictionary<string, NumQuantityCounter> CounterDic
        {
            get
            {
                if (_counterDic == null)
                {
                    lock (_lockHelper)
                    {
                        if (_counterDic == null)
                            _counterDic = new Dictionary<string, NumQuantityCounter>();
                    }
                }
                return _counterDic;
            }
        }

        private NumQuantityCounterFactory()
        { }

        public static NumQuantityCounterFactory GetFactory
        {
            get
            {
                if (_factory == null)
                {
                    lock (_lockHelper)
                    {
                        if (_factory == null)
                            _factory = new NumQuantityCounterFactory();
                    }
                }
                return _factory;
            }
        }

        public NumQuantityCounter GetCounter(GameType gameType, CompanyType companyType, IList<CompanyTypeSupportNumLen> companySupportNumLen)
        {
            var key = string.Format("{0}_{1}", (int)gameType, companyType);
            if (!CounterDic.ContainsKey(key))
            {
                CounterDic.Add(key, BuildCounter(gameType, companySupportNumLen));
            }
            return CounterDic[key];
        }

        private NumQuantityCounter BuildCounter(GameType gameType, IList<CompanyTypeSupportNumLen> companySupportNumLen)
        {
            NumQuantityCounter counter;
            switch (gameType)
            {
                case GameType.TwoDigital: counter = new TwoDigitNumQuantityCounter(); break;
                case GameType.ThreeDigital: counter = new ThreeDigitNumQuantityCounter(); break;
                case GameType.FourDigital: counter = new FourDigitNumQuantityCounter(); break;
                case GameType.FiveDigital: counter = new FiveDigitNumQuantityCounter(); break;
                case GameType.PL2: counter = new PL2NumQuantityCounter(); break;
                case GameType.PL3: counter = new PL3NumQuantityCounter(); break;
                case GameType.A_B_PL2:
                case GameType.C_A_PL2:
                case GameType.B_C_PL2: counter = new A_B_PL2NumQuantityCounter(); break;
                default: throw new NotSupportedException(string.Format("不支持游戏类型:{0}", gameType));
            }
            counter.CompanySupportNumLen = companySupportNumLen;
            return counter;
        }
    }
}
