using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core.Limit
{
    internal class UpperLimiter : ILimitCheck
    {
        private static object _lockHelper = new object();

        public bool IsDrop { get; private set; }

        public LimitChecker BaseChecker { get; set; }

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

        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        private IDictionary<string, IList<DropWater>> _todayDropWater;
        protected IDictionary<string, IList<DropWater>> TodayDropWater
        {
            get
            {
                if (_todayDropWater == null)
                    _todayDropWater = new Dictionary<string, IList<DropWater>>();
                return _todayDropWater;
            }
        }

        private IList<LotteryCompany> _todayCompany;
        public IList<LotteryCompany> TodayCompany
        {
            get
            {
                if (_todayCompany == null || _todayCompany.Count == 0)
                    _todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
                return _todayCompany;
            }
        }

        public bool Inject(BetOrder order)
        {
            IsDrop = false;
            bool returnValue;
            if (returnValue = !Extended.IsStopAcceptBet(order.CompanyId, order.GamePlayWayId))
            {
                lock (_lockHelper)
                {
                    var upperLimit = GetUpperLimit(order);
                    if (returnValue = (!upperLimit.StopBet && Check(order, upperLimit)))
                    {
                        TransformOrder(order, upperLimit);
                        var increaseAmount = order.Amount * (1 - (decimal)order.User.UserInfo.RateGroup.ShareRate);
                        upperLimit.TotalBetAmount += increaseAmount;
                        UpperLimitManager.GetManager().UpdateLimit(upperLimit);
                    }
                }
            }
            return returnValue;
        }
        public void RollOrderLimit(BetOrder order)
        {
            if (order.User == null)
                order.User = UserManager.GetUser(order.UserId);
            var increaseAmount = order.Amount * (1 - (decimal)order.User.UserInfo.RateGroup.ShareRate);

            var upperLimit = GetUpperLimit(order);
            if (upperLimit.TotalBetAmount < increaseAmount)
                throw new InvalidDataException("Error Data!Total bet amount less than order's amount!");

            var dropWaters = GetDropWater(order.Num, order.CompanyId, order.GamePlayWayId);

            upperLimit.TotalBetAmount -= increaseAmount;        //注单总金额减少

            var previous = dropWaters.Where(it => it.Amount > upperLimit.TotalBetAmount && it.Amount < upperLimit.NextLimit).        //找到上一个跌水规则
                GroupBy(it => it.Amount).
                OrderByDescending(it => it.Key);

            foreach (var previou in previous)
            {
                DropWater dropwater = null;
                int previousDropCount = 0;
                if (previous != null)
                {
                    foreach (var drop in previou)
                    {
                        previousDropCount++;
                        if (dropwater != null && dropwater.DropType == DropType.Manual) continue;
                        dropwater = drop;
                    }
                    if (previousDropCount > 2)
                        throw new InvalidDataException(string.Format("号码:{0}的跌水记录大于2!CompanyId:{1} , GamePlayWayId:{2}",
                            upperLimit.Num, upperLimit.CompanyId, upperLimit.GamePlayWayId));
                }
                if (dropwater != null && dropwater.Amount > upperLimit.TotalBetAmount)      //如果跌水规则的金额 > 下注总金额
                {
                    upperLimit.NextLimit = dropwater.Amount;
                    upperLimit.DropValue -= dropwater.DropValue;
                    //防止出现负数
                    if (upperLimit.DropValue < 0)
                        upperLimit.DropValue = 0;
                }
            }
            UpperLimitManager.GetManager().UpdateLimit(upperLimit);
        }

        /// <summary>
        /// 获取注单相对应的上限限制记录.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <returns></returns>
        protected BetUpperLimit GetUpperLimit(BetOrder order)
        {
            //获取指定公司，玩法，号码的上限限制
            var upperLimit = UpperLimitManager.GetManager().GetLimit(order);
            if (upperLimit == null)
                throw new ApplicationException("找不到上限!");
            return upperLimit;
        }

        /// <summary>
        /// 检查该注单是否可下.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="upperLimit">The upper limit.</param>
        /// <returns></returns>
        protected bool Check(BetOrder order, BetUpperLimit upperLimit)
        {
            if (order.User == null)
                order.User = UserManager.GetUser(order.UserId);
            var increaseAmount = order.Amount * (1 - (decimal)order.User.UserInfo.RateGroup.ShareRate);

            decimal totalBetAmount = increaseAmount + upperLimit.TotalBetAmount;
            if (totalBetAmount > upperLimit.UpperLlimit)        //最终投注金额大于投注上限金额
            {
                //TODO:错误信息呈现设计，存在多个插入失败
                //ErrorMessage = "Can't Bet!";
                return false;
            }
            while (totalBetAmount > upperLimit.NextLimit)          //总投注金额小于投注上限，大于本次跌水金额限制（增加跌水）
            {
                UpdateUpperLimit(upperLimit);

                //本次跌水限制金额==总下注金额时则说明没有跌水限制了
                if (upperLimit.NextLimit == upperLimit.TotalBetAmount) break;
            }

            return true;
        }

        protected IList<DropWater> GetDropWater(string num, int companyId, int gameplaywayId)
        {
            string key = string.Format("{0}_{1}_{2}", num, companyId, gameplaywayId);
            if (!TodayDropWater.ContainsKey(key))
            {
                LotteryCompany company = TodayCompany.Find(it => it.CompanyId == companyId);
                var dropWaters = DwManager.GetTodayNumsDropWater(num, gameplaywayId, company);
                TodayDropWater.Add(key, dropWaters.ToList());
            }
            return TodayDropWater[key];
        }
        private void UpdateUpperLimit(BetUpperLimit upperLimit)
        {
            var dropWaters = GetDropWater(upperLimit.Num, upperLimit.CompanyId, upperLimit.GamePlayWayId);
            var updropWaters = dropWaters.Where(it => it.Amount == upperLimit.NextLimit);   //获取下一个跌水限制
            DropWater updrop = null;
            //查找本次跌水
            foreach (var drop in updropWaters)
            {
                if (updrop != null && updrop.DropType == DropType.Manual) continue;
                updrop = drop;
            }
            if (updrop == null) LogConsole.Error(string.Format("号码:{0} 不存在金额为{1}的跌水限制!CompanyId:{2} , GamePlayWayId:{3}",
                upperLimit.Num, upperLimit.NextLimit, upperLimit.CompanyId, upperLimit.GamePlayWayId));
            else
                upperLimit.DropValue += updrop.DropValue;       //累加跌水值

            var minDrops = dropWaters.Where(it => it.Amount > upperLimit.NextLimit).GroupBy(it => it.Amount).OrderBy(it => it.Key).FirstOrDefault();
            int minDropCount = 0;
            DropWater dropWater = null;
            if (minDrops != null)
            {
                foreach (var drop in minDrops)
                {
                    minDropCount++;
                    if (dropWater != null && dropWater.DropType == DropType.Manual) continue;
                    dropWater = drop;
                }
                if (minDropCount > 2)
                    throw new ApplicationException(string.Format("号码:{0}的跌水记录大于2!CompanyId:{1} , GamePlayWayId:{2}",
                        upperLimit.Num, upperLimit.CompanyId, upperLimit.GamePlayWayId));
            }

            if (minDropCount == 0 || dropWater == null)
                upperLimit.NextLimit = upperLimit.UpperLlimit;
            else
                upperLimit.NextLimit = dropWater.Amount;
        }
        private void TransformOrder(BetOrder order, BetUpperLimit upperLimit)
        {
            if (upperLimit.DropValue > 0)
            {
                IsDrop = true;
                order.DropWater += upperLimit.DropValue;
                order.Net += upperLimit.DropValue;
                double memberComm = (100 - order.Net).PercentageToDecimal(4);
                order.Commission = order.Turnover * (decimal)memberComm;
                order.NetAmount = order.Turnover - order.Commission;
                order.AncestorCommission.ForEach(it =>
                {
                    if (it.Role == Role.Company)
                    {
                        it.Commission += upperLimit.DropValue;
                    }
                    else
                    {
                        it.Commission -= upperLimit.DropValue;
                    }
                    it.CommAmount = (decimal)it.Commission.PercentageToDecimal(4) * order.Turnover;
                });
            }
        }
    }
}
