using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Limit
{
    /// <summary>
    /// 自动跌水限制，用户连续下单跌水限制
    /// </summary>
    internal class AutoDropLimiter : ILimitCheck
    {
        private DropWaterManager _dwManager;
        public DropWaterManager DwManager
        {
            get
            {
                if (_dwManager == null)
                    _dwManager = new DropWaterManager();
                return _dwManager;
            }
        }
        private IList<LotteryCompany> _todayCompany;
        internal IList<LotteryCompany> TodayCompany
        {
            get
            {
                if (_todayCompany == null)
                    _todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
                return _todayCompany;
            }
        }
        private IDictionary<string, IEnumerable<BetAutoDropWater>> _autoDropDic;
        internal IDictionary<string, IEnumerable<BetAutoDropWater>> AutoDropDic
        {
            get
            {
                if (_autoDropDic == null)
                    _autoDropDic = new Dictionary<string, IEnumerable<BetAutoDropWater>>();
                return _autoDropDic;
            }
        }

        private IDictionary<string, IEnumerable<UserBetAutoDropWater>> _userAutoDropDic;
        internal IDictionary<string, IEnumerable<UserBetAutoDropWater>> UserAutoDropDic
        {
            get
            {
                if (_userAutoDropDic == null)
                    _userAutoDropDic = new Dictionary<string, IEnumerable<UserBetAutoDropWater>>();
                return _userAutoDropDic;
            }
        }

        /// <summary>
        /// 上一级的检查器
        /// </summary>
        /// <value>
        /// The base checker.
        /// </value>
        public LimitChecker BaseChecker { get; set; }

        public bool IsDrop { get; private set; }
        public bool Inject(BetOrder order)
        {
            IsDrop = false;
            var betedAmount = GetUserBetedAmountByOrder(order);
            var increaseAmount = order.Amount * (1 - (decimal)order.User.UserInfo.RateGroup.ShareRate);
            var totalAmount = betedAmount + increaseAmount;

            //计算要跌水的值

            //var drops = GetDropWater(order);
            var drops = GetAutoDropWater(order);
            double dropValue = 0; //drops.Where(it => it.Amount < totalAmount).Sum(it => it.DropValue);
            foreach (var d in drops)
                if (d.Amount < totalAmount) dropValue += d.DropValue;

            if (dropValue > 0)
            {
                IsDrop = true;
                order.DropWater += dropValue;
                order.Net += dropValue;
                double memberComm = (100 - order.Net).PercentageToDecimal(4);
                order.Commission = order.Turnover * (decimal)memberComm;
                order.NetAmount = order.Turnover - order.Commission;
                order.AncestorCommission.ForEach(it =>
                {
                    if (it.Role == Role.Company)
                    {
                        it.Commission += order.DropWater;
                    }
                    else
                    {
                        it.Commission -= order.DropWater;
                    }
                    it.CommAmount = (decimal)(it.Commission.PercentageToDecimal(4)) * order.Turnover;
                });
            }
            return true;
        }

        public void RollOrderLimit(BetOrder order)
        {
        }

        private IEnumerable<BetAutoDropWater> GetAutoDropWater(BetOrder order)
        {
            var dropDict = GetDropWater(order).ToDictionary(it => it.Amount, it => it);
            foreach (var drop in GetUserDropWater(order))
            {
                dropDict[drop.Amount] = new BetAutoDropWater
                {
                    Amount = drop.Amount,
                    DropValue = drop.DropValue,
                    CompanyType = drop.CompanyType,
                    CompanyTypeId = drop.CompanyTypeId,
                    GamePlayWayId = drop.GamePlayWayId
                };
            }
            return dropDict.Values.ToList();
        }

        private IEnumerable<BetAutoDropWater> GetDropWater(BetOrder order)
        {
            var company = TodayCompany.Find(it => it.CompanyId == order.CompanyId);
            if (company == null)
                throw new ArgumentException("找不到开奖公司,CompanyId:" + order.CompanyId);
            string key = string.Format("{0}_{1}", (int)company.CompanyType, order.GamePlayWayId);
            IEnumerable<BetAutoDropWater> drops;
            if (!AutoDropDic.TryGetValue(key, out drops))
            {
                drops = DwManager.GetAutoDrops(company.CompanyType, order.GamePlayWayId);
                AutoDropDic.Add(key, drops);
            }
            return drops;

        }

        private IEnumerable<UserBetAutoDropWater> GetUserDropWater(BetOrder order)
        {
            var company = TodayCompany.Find(it => it.CompanyId == order.CompanyId);
            if (company == null)
                throw new ArgumentException("找不到开奖公司,CompanyId:" + order.CompanyId);
            string key = string.Format("{0}_{1}_{2}", order.UserId, (int)company.CompanyType, order.GamePlayWayId);
            IEnumerable<UserBetAutoDropWater> drops;
            if (!UserAutoDropDic.TryGetValue(key, out drops))
            {
                drops = DwManager.GetAutoDrops(order.UserId, company.CompanyType, order.GamePlayWayId);
                UserAutoDropDic.Add(key, drops);
            }
            return drops;

        }

        private decimal GetUserBetedAmountByOrder(BetOrder order)
        {
            string key = BaseChecker.GetBetAmountDicKey(order);
            decimal totalAmount = 0;
            BetOrder target;
            if (BaseChecker.UserValidOrderDic.TryGetValue(key, out target))
                totalAmount += target.Amount;
            Func<BetOrder, bool> spec = it => it.Num == order.Num && it.CompanyId == order.CompanyId && it.GamePlayWayId == order.GamePlayWayId;
            foreach (var item in BaseChecker.BeInsertOrderList)
                if (spec(item))
                    totalAmount += item.Amount;
            return totalAmount;

            //decimal dbAmount = 0, beAddAmount = 0;
            //Func<BetOrder, bool> spec = it => it.Num == order.Num && it.CompanyId == order.CompanyId && it.GamePlayWayId == order.GamePlayWayId;
            //foreach (var item in BaseChecker.UserValidOrderList)
            //    if (spec(item))
            //        dbAmount += item.Amount;
            //foreach (var item in BaseChecker.BeInsertOrderList)
            //    if (spec(item))
            //        beAddAmount += item.Amount;
            //return dbAmount + beAddAmount;
        }

    }
}
