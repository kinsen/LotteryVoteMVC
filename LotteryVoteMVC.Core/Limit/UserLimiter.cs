using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Limit
{
    internal class UserLimiter : ILimitCheck
    {
        private LimitManager _limitManager;
        private UserLimitManager _userLimitManager;
        public LimitManager LimitManager
        {
            get
            {
                if (_limitManager == null)
                    _limitManager = new LimitManager();
                return _limitManager;
            }
        }
        public UserLimitManager UserLimitManager
        {
            get
            {
                if (_userLimitManager == null)
                    _userLimitManager = new UserLimitManager();
                return _userLimitManager;
            }
        }
        public GroupLimitManager GroupLimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<GroupLimitManager>();
            }
        }
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        private IList<LotteryCompany> _todayCompany;
        protected IList<LotteryCompany> TodayCompany
        {
            get
            {
                if (_todayCompany == null)
                    _todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
                return _todayCompany;
            }
        }

        private IDictionary<int, IList<BetLimit>> _betLimits;
        protected IDictionary<int, IList<BetLimit>> BetLimitDic
        {
            get
            {
                if (_betLimits == null)
                    _betLimits = new Dictionary<int, IList<BetLimit>>();
                return _betLimits;
            }
        }

        private IDictionary<int, IList<GameBetLimit>> _gameBetLimits;
        protected IDictionary<int, IList<GameBetLimit>> GameBetLimitDic
        {
            get
            {
                if (_gameBetLimits == null)
                    _gameBetLimits = new Dictionary<int, IList<GameBetLimit>>();
                return _gameBetLimits;
            }
        }

        private IDictionary<int, IList<RateGroupBetLimit>> _rateGroupBetLimits;
        protected IDictionary<int, IList<RateGroupBetLimit>> RateGroupBetLimits
        {
            get
            {
                if (_rateGroupBetLimits == null)
                    _rateGroupBetLimits = new Dictionary<int, IList<RateGroupBetLimit>>();
                return _rateGroupBetLimits;
            }
        }

        private IDictionary<int, IList<RateGroupGameBetLimit>> _rateGroupGameLimits;
        protected IDictionary<int, IList<RateGroupGameBetLimit>> RateGroupGameLimits
        {
            get
            {
                if (_rateGroupGameLimits == null)
                    _rateGroupGameLimits = new Dictionary<int, IList<RateGroupGameBetLimit>>();
                return _rateGroupGameLimits;
            }
        }

        private IDictionary<int, User> _userDict;
        protected IDictionary<int, User> UserDict
        {
            get
            {
                if (_userDict == null)
                    _userDict = new Dictionary<int, User>();
                return _userDict;
            }
        }

        public bool IsDrop
        {
            get { throw new NotImplementedException(); }
        }
        /// <summary>
        /// 上一级的检查器
        /// </summary>
        /// <value>
        /// The base checker.
        /// </value>
        public LimitChecker BaseChecker { get; set; }

        public string ErrorMessage { get; private set; }
        public bool Inject(BetOrder order)
        {
            return CheckBetLimit(order) && CheckGameLimit(order);
        }
        protected IList<BetLimit> GetLimitByUser(int userId)
        {
            IList<BetLimit> limits = null;
            if (!BetLimitDic.TryGetValue(userId, out limits))
                BetLimitDic.Add(userId, limits = UserLimitManager.GetLimits(userId).ToList());
            return limits;
        }
        protected IList<RateGroupBetLimit> GetRateGroupLimitByUser(int userId)
        {
            User user = null;
            if (!UserDict.TryGetValue(userId, out user))
                UserDict.Add(userId, user = UserManager.GetUser(userId));
            IList<RateGroupBetLimit> limits = null;
            var groupId = UserDict[userId].UserInfo.RateGroupId;
            if (!RateGroupBetLimits.TryGetValue(groupId, out limits))
                RateGroupBetLimits.Add(groupId, limits = GroupLimitManager.GetLimits(groupId).ToList());
            return limits;
        }
        protected IList<GameBetLimit> GetGameLimitByUser(int userId)
        {
            IList<GameBetLimit> limits = null;
            if (!GameBetLimitDic.TryGetValue(userId, out limits))
                GameBetLimitDic.Add(userId, limits = UserLimitManager.GetGameLimits(userId).ToList());
            return limits;
        }
        protected IList<RateGroupGameBetLimit> GetRateGroupGameLimitByUser(int userId)
        {
            User user = null;
            if (!UserDict.TryGetValue(userId, out user))
                UserDict.Add(userId, user = UserManager.GetUser(userId));
            IList<RateGroupGameBetLimit> limits;
            var groupId = user.UserInfo.RateGroupId;
            if (!RateGroupGameLimits.TryGetValue(groupId, out limits))
                RateGroupGameLimits.Add(groupId, limits = GroupLimitManager.GetGameLimits(groupId).ToList());
            return limits;
        }
        /// <summary>
        /// Checks the bet limit.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="orderList">The order list.</param>
        /// <returns></returns>
        protected bool CheckBetLimit(BetOrder order)
        {
            var gameplayway = LotterySystem.Current.GamePlayWays.Find(it => it.Id == order.GamePlayWayId);
            var betLimitList = GetLimitByUser(order.UserId);
            var betlimit = betLimitList.Find(it => it.GameType == gameplayway.GameType);   //找出单注限制
            if (order.Amount < betlimit.LeastLimit || order.Amount > betlimit.LargestLimit)
                return false;
            var groupLimitList = GetRateGroupLimitByUser(order.UserId);
            var groupBetLimit = groupLimitList.Find(it => it.GameType == gameplayway.GameType);
            if (order.Amount < groupBetLimit.LeastLimit || order.Amount > groupBetLimit.LargestLimit)
                return false;
            return true;
        }
        protected bool CheckGameLimit(BetOrder order)
        {
            //注单中的公司肯定在今日开奖公司中
            var company = TodayCompany.Find(it => it.CompanyId == order.CompanyId);
            if (company == null) return false;
            var companyType = company.CompanyType;
            //获取注单公司所在公司类型所有公司
            var companyTypeCompanys = TodayCompany.Where(it => it.CompanyType == company.CompanyType);
            Spec<BetOrder> orSpec = null;
            bool isFirst = true;
            foreach (var com in companyTypeCompanys)
            {
                if (isFirst)
                {
                    isFirst = false;
                    orSpec = it => it.CompanyId == com.CompanyId;
                }
                else
                    orSpec = orSpec.Or(it => it.CompanyId == com.CompanyId);
            }
            if (order.Num.IsNum())
            {
                orSpec = orSpec.And(it => it.Num == order.Num);
            }
            else if (order.Num.IsPL2())
            {
                var splNums = order.Num.Split('#');
                Spec<BetOrder> plSpec = it => it.Num == order.Num;
                plSpec = plSpec.Or(it => it.Num == splNums[1] + "#" + splNums[0]);
                orSpec = orSpec.And(plSpec);
            }
            else if (order.Num.IsPL3())
            {
                var plNums = order.Num.Split('#');
                string[] nums = new string[5];
                string numFormat = "{0}#{1}#{2}";
                nums[0] = string.Format(numFormat, nums[0], nums[2], nums[1]);
                nums[1] = string.Format(numFormat, nums[1], nums[0], nums[2]);
                nums[2] = string.Format(numFormat, nums[1], nums[2], nums[0]);
                nums[3] = string.Format(numFormat, nums[2], nums[0], nums[1]);
                nums[4] = string.Format(numFormat, nums[2], nums[1], nums[0]);
                Spec<BetOrder> plSpec = it => it.Num == order.Num;
                foreach (var plNum in nums)
                    plSpec = plSpec.Or(it => order.Num == plNum);
                orSpec = orSpec.And(plSpec);
            }
            //获取数据库中的总金额
            Func<BetOrder, bool> spec = it => it.GamePlayWayId == order.GamePlayWayId && orSpec(it);
            decimal dbAmount = 0, beAddAmount = 0;

            string key = BaseChecker.GetBetAmountDicKey(order);
            BetOrder target;
            if (BaseChecker.UserValidOrderDic.TryGetValue(key, out target))
                dbAmount = target.Amount;

            //foreach (var item in BaseChecker.UserValidOrderList)
            //    if (spec(item))
            //        dbAmount += item.Amount;
            foreach (var item in BaseChecker.BeInsertOrderList)
                if (spec(item))
                    beAddAmount += item.Amount;

            var totalAmount = dbAmount + beAddAmount;    //取得总金额
            totalAmount += order.Amount;
            var gameLimitList = GetGameLimitByUser(order.UserId);
            //找出游戏限制
            var gameLimit = gameLimitList.Find(it => it.CompanyType == companyType && it.GamePlayWayId == order.GamePlayWayId);
            if (gameLimit == null)
                throw new ApplicationException(string.Format("不存在GamePlayWayId为{0}的游戏限制!", order.GamePlayWayId));

            if (totalAmount > gameLimit.LimitValue)
                return false;

            var rateGroupGamelimits = GetRateGroupGameLimitByUser(order.UserId);
            //找出游戏限制
            var groupGameLimit = rateGroupGamelimits.Find(it => it.CompanyType == companyType && it.GamePlayWayId == order.GamePlayWayId);
            if (groupGameLimit == null)
                throw new ApplicationException(string.Format("不存在GamePlayWayId为{0}的分成组游戏限制!", order.GamePlayWayId));

            if (totalAmount > groupGameLimit.LimitValue)
                return false;

            return true;
        }

        public void RollOrderLimit(BetOrder order)
        {
            throw new NotImplementedException();
        }


    }
}
