using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;
using System.Web;
using System.Web.Caching;

namespace LotteryVoteMVC.Core
{
    public class SettleManager : ManagerBase, IDisposable
    {
        private const string M_ALLUSER = "AllValidUser";
        private const string M_ORDEROAC = "OrderOac";
        private static bool M_IsSetting = false;
        private SettleResultDataAccess _daSettle;
        public SettleResultDataAccess DaSettle
        {
            get
            {
                if (_daSettle == null)
                    _daSettle = new SettleResultDataAccess();
                return _daSettle;
            }
        }
        public BetManager BetManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<BetManager>();
            }
        }
        public OrderManager OrderManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<OrderManager>();
            }
        }
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        /// <summary>
        /// 本次结算公司的所有注单
        /// </summary>
        internal IEnumerable<BetOrder> AllOrders { get; private set; }
        /// <summary>
        /// 今日下注用户
        /// </summary>
        internal IEnumerable<User> BetUsers { get; private set; }
        private IDictionary<int, IList<BetOrder>> _userBetOrderDic;
        internal IDictionary<int, IList<BetOrder>> UserBetOrderDic
        {
            get
            {
                if (_userBetOrderDic == null)
                    _userBetOrderDic = new Dictionary<int, IList<BetOrder>>();
                return _userBetOrderDic;
            }
        }
        private IDictionary<int, IList<SettleResult>> _memberSettleResult;
        protected IDictionary<int, IList<SettleResult>> MemberSettleResultDic
        {
            get
            {
                if (_memberSettleResult == null)
                    _memberSettleResult = new Dictionary<int, IList<SettleResult>>();
                return _memberSettleResult;
            }
        }
        protected IDictionary<int, User> UserDic
        {
            get
            {
                var userDic = HttpRuntime.Cache[M_ALLUSER] as IDictionary<int, User>;
                if (userDic == null)
                {
                    userDic = UserManager.DaUser.GetAllValidUser().ToDictionary(it => it.UserId, it => it);
                    //将所有用户的信息写入缓存中5分钟
                    HttpRuntime.Cache.Add(M_ALLUSER, userDic, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return userDic;
            }
        }
        /// <summary>
        /// 所有注单的附加信息字典
        /// </summary>
        protected IDictionary<int, IEnumerable<OrderAncestorCommInfo>> OrderOacDic
        {
            get
            {
                var dic = HttpRuntime.Cache[M_ORDEROAC] as IDictionary<int, IEnumerable<OrderAncestorCommInfo>>;
                if (dic == null)
                {
                    dic = new Dictionary<int, IEnumerable<OrderAncestorCommInfo>>();
                    HttpRuntime.Cache.Add(M_ORDEROAC, dic, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Normal, null);
                }
                return dic;
            }
        }
        protected int SettleCompany { get; private set; }
        private List<SettleResult> _settleResult;
        protected List<SettleResult> SettleResultList
        {
            get
            {
                if (_settleResult == null)
                    _settleResult = new List<SettleResult>();
                return _settleResult;
            }
        }

        #region Action
        /// <summary>
        /// 询问该公司是否可以结算
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns>
        ///   <c>true</c> if this instance can settle the specified company id; otherwise, <c>false</c>.
        /// </returns>
        public bool CanSettle(int companyId)
        {
            return DaSettle.CountSettleCountByCompany(companyId, DateTime.Today) == 0;
        }
        /// <summary>
        /// 结算指定公司注单
        /// </summary>
        /// <param name="companyId">The company id.</param>
        public void Settle(int companyId)
        {
            if (!M_IsSetting)
            {
                M_IsSetting = true;
                try
                {
                    SettleCompany = companyId;
                    AllOrders = BetManager.SettleBetOrder(companyId);
                    BetUsers = UserManager.GetBetUsers();
                    var companys = BetUsers.Where(it => it.Role == Role.Company); //UserManager.DaUser.GetUserByRole(Models.Role.Company);
                    foreach (var cp in companys)
                        GetSettleResult(cp);
                    DaSettle.Insert(SettleResultList);
                }
                finally
                {
                    M_IsSetting = false;
                    Dispose();
                }
            }
        }
        /// <summary>
        /// 重新结算
        /// </summary>
        /// <param name="companyId">The company id.</param>
        public void Resettle(int companyId)
        {
            //清除缓存中的数据
            foreach (var key in new string[] { "TodayLotteryResult", "2DLotteryResult", "3DLotteryResult", "4DLotteryResult", "5DLotteryResult" })
                HttpRuntime.Cache.Remove(key);
            LogConsole.Debug("Setting");
            if (!M_IsSetting)
            {
                M_IsSetting = true;
                try
                {
                    SettleCompany = companyId;
                    DaSettle.Delete(companyId, DateTime.Today);
                    AllOrders = BetManager.ResettleBetOrder(companyId);
                    BetUsers = UserManager.GetBetUsers();
                    var companys = BetUsers.Where(it => it.Role == Role.Company);// UserManager.DaUser.GetUserByRole(Models.Role.Company);
                    foreach (var cp in companys)
                        GetSettleResult(cp);
                    LogConsole.Debug("Insert SettleResult.");
                    DaSettle.Insert(SettleResultList);
                }
                finally
                {
                    M_IsSetting = false;
                    Dispose();
                }
            }
        }

        #region Private
        private IList<SettleResult> GetSettleResult(User user)
        {
            List<SettleResult> memberResultList = new List<SettleResult>();     //用于盛放子用户的结余记录
            if (user.Role == Role.Guest && BetUsers.Contains(it => it.UserId == user.UserId))                                  //会员结余计算
            {
                var result = BuildMemberSettleResult(user);
                if (result != null)
                {
                    SettleResultList.Add(result);                               //添加到总结余记录表容器中
                    memberResultList.Add(result);
                }
                return memberResultList;
            }

            var childUser = BetUsers.Where(it => it.ParentId == user.UserId); //UserManager.GetChilds(user);
            List<BetOrder> betOrderList = new List<BetOrder>();                 //用于存放本级用户可用到的注单
            foreach (var child in childUser)
            {
                memberResultList.AddRange(GetSettleResult(child));              //迭代直到所有Guest级别的用户都结余完毕
                betOrderList.AddRange(UserBetOrderDic[child.UserId]);           //将子用户的注单也添加到本级用户的注单容器中

                if (MemberSettleResultDic.ContainsKey(child.UserId))
                    AddMemberSettleResultToDic(user, MemberSettleResultDic[child.UserId]);
            }
            UserBetOrderDic.Add(user.UserId, betOrderList);

            if (user.Role == Role.Company)        //到公司级别不进行上级结余计算（公司为最顶级了）
                return memberResultList;
            var userResultList = BuildUserSettleResult(user, memberResultList);
            SettleResultList.AddRange(userResultList);
            return userResultList;
        }
        /// <summary>
        /// 创建会员的结余结果.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private SettleResult BuildMemberSettleResult(User user)
        {
            var orderList = GetOrders(user).ToList();
            UserBetOrderDic[user.UserId] = orderList;

            if (orderList == null || orderList.Count == 0)
                return null;
            var oacs = OrderManager.GetTodayOac(user, SettleCompany, BetStatus.Settled).GroupBy(it => it.OrderId).ToDictionary(it => it.Key, it => it.Select(x => x));
            OrderOacDic.AddRange(oacs);

            List<SettleResult> resultList = new List<SettleResult>();
            SettleResult result = new SettleResult();
            foreach (var order in orderList)
            {
                result.OrderCount += 1;
                result.BetTurnover += order.Turnover;
                result.Commission += order.Commission;
                result.TotalWinLost += order.DrawResult;
                result.TotalCommission += GetOAC(order.OrderId, Role.Agent).CommAmount;
            }
            result.WinLost = result.TotalWinLost - result.Commission;

            result.UpperShareRate = GetParentShareRate(user);
            result.UpperWinLost = result.WinLost * -1 * (decimal)result.UpperShareRate;   //代理输赢=会员输赢×代理成数
            result.CompanyWinLost = CountCompanyWinLost(1 - result.UpperShareRate, result.WinLost, result.TotalCommission);
            result.UpperTotalWinLost = result.TotalWinLost / -1 - result.CompanyWinLost;         //代理总输赢=会员总输赢-公司输赢
            result.UpperCommission = result.UpperTotalWinLost - result.UpperWinLost;        //代理佣金=代理总输赢-代理输赢
            result.UserId = user.UserId;
            result.CompanyId = SettleCompany;
            AddMemberSettleResultToDic(user, result);
            return result;
        }

        /// <summary>
        /// 创建各级用户的结余结果.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="childSettleResultList">The child settle result list.</param>
        /// <returns></returns>
        private IList<SettleResult> BuildUserSettleResult(User user, IList<SettleResult> childSettleResultList)
        {
            var userWinLost = UserBetOrderDic[user.UserId].Sum(it => it.DrawResult);
            //用户总佣金，累加子用户所有订单中该用户的佣金
            var userTotalComm = UserBetOrderDic[user.UserId].Select(it => GetOAC(it.OrderId, user.Role - 1)).Sum(it => it.CommAmount);     //总佣金=父亲用户佣金
            List<SettleResult> userResultList = new List<SettleResult>();
            var gs = childSettleResultList.GroupBy(it => it.ShareRate + it.UpperShareRate);     //用分成来进行分组
            foreach (var results in gs)
            {
                SettleResult result = new SettleResult();
                List<SettleResult> memberSettleResultList = new List<SettleResult>();
                foreach (var item in results)
                {
                    if (item == null) continue;
                    memberSettleResultList.AddRange(MemberSettleResultDic[item.UserId]);
                    //result.OrderCount += item.OrderCount;                                       //累加子用户的订单数量
                    result.BetTurnover += item.BetTurnover;                                     //累加子用户的下注额
                    result.WinLost += (item.WinLost + item.UpperWinLost);                       //累加子用户的输赢          
                    result.Commission += (item.Commission + item.UpperCommission);              //累加子用户的佣金
                    result.TotalWinLost += (item.TotalWinLost + item.UpperTotalWinLost);        //累加子用户的总输赢
                }
                var memberWinLost = memberSettleResultList.Distinct().Sum(it => it.WinLost);
                result.OrderCount = UserBetOrderDic[user.UserId].Count;
                result.TotalCommission = userTotalComm;
                result.ShareRate = results.Key;                                                 //本级分成数(相对级别，包含本用户及其子用户的分成)
                result.UpperShareRate = GetParentShareRate(user);        //上级分成
                result.CompanyWinLost = CountCompanyWinLost(1 - result.UpperShareRate - result.ShareRate, memberWinLost, result.TotalCommission);
                result.UpperWinLost = memberWinLost * -1 * (decimal)result.UpperShareRate;       //上级输赢=子用户输赢（总）×上级成数
                result.UpperTotalWinLost = result.TotalWinLost / -1 - result.CompanyWinLost;    //上级总输赢=（总输赢/-1）-公司输赢
                result.UpperCommission = result.UpperTotalWinLost - result.UpperWinLost;        //上级佣金=上级总输赢-上级输赢
                result.UserId = user.UserId;
                result.CompanyId = SettleCompany;
                userResultList.Add(result);
            }

            return userResultList;
        }
        /// <summary>
        /// 获取父级所占的分成数
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private double GetParentShareRate(User user)
        {
            if (user.ParentId == 0) return 0;   //公司父级为0
            User parent;
            if (!UserDic.TryGetValue(user.ParentId, out parent))
            {
                parent = UserManager.GetUser(user.ParentId);
                UserDic.Add(user.ParentId, parent);
            }
            return parent.UserInfo.RateGroup.ShareRate - user.UserInfo.RateGroup.ShareRate;
        }
        private IEnumerable<BetOrder> GetOrders(User user)
        {
            if (AllOrders == null)
                AllOrders = OrderManager.DaOrder.GetOrders(BetStatus.Settled, SettleCompany, DateTime.Today);
            return AllOrders.Where(it => it.UserId == user.UserId);
        }

        /// <summary>
        /// 获取指定注单，角色的注单分成信息
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="role">The role.</param>
        /// <returns></returns>
        private OrderAncestorCommInfo GetOAC(int orderId, Role role)
        {
            IEnumerable<OrderAncestorCommInfo> oacs;
            if (!OrderOacDic.TryGetValue(orderId, out oacs))
            {
                oacs = OrderManager.GetOac(orderId).ToList();
                OrderOacDic.Add(orderId, oacs);
            }
            return oacs.Find(it => it.Role == role);
        }
        /// <summary>
        /// 计算公司输赢.
        /// </summary>
        /// <param name="companyShareRate">The company share rate.</param>
        /// <param name="userWinLost">The user win lost.</param>
        /// <param name="totalCommission">The total commission.</param>
        /// <returns></returns>
        private decimal CountCompanyWinLost(double companyShareRate, decimal userWinLost, decimal totalCommission)
        {
            return ((userWinLost + totalCommission) * (decimal)companyShareRate) * -1;      //公司输赢=（会员输赢+总佣金）×公司成数×-1；
        }

        private void AddMemberSettleResultToDic(User user, SettleResult result)
        {
            if (!MemberSettleResultDic.ContainsKey(user.UserId))
                MemberSettleResultDic.Add(user.UserId, new List<SettleResult>());
            MemberSettleResultDic[user.UserId].Add(result);
        }
        private void AddMemberSettleResultToDic(User user, IList<SettleResult> resultList)
        {
            if (!MemberSettleResultDic.ContainsKey(user.UserId))
                MemberSettleResultDic.Add(user.UserId, new List<SettleResult>());
            var settleResultList = MemberSettleResultDic[user.UserId] as List<SettleResult>;
            settleResultList.AddRange(resultList);
        }
        #endregion
        #endregion

        #region Search
        /// <summary>
        /// 获取指定用户，日期的结算记录
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<SettleResult> GetSettleResult(User user, DateTime fromDate, DateTime toDate)
        {
            var settleResult = DaSettle.ListSettleResult(user, fromDate, toDate);
            return MergerResult(settleResult);
        }
        public PagedList<SettleResult> GetMemberSettleResult(User user, DateTime fromDate, DateTime toDate, string orderField, string orderType, int pageIndex)
        {
            string[] orderFileds = new[] { "TotalWinLost", "WinLost", "Commission", "TotalCommission", "BetTurnover", "OrderCount" };
            string[] orderTypes = new[] { "desc", "asc" };
            if (!orderFileds.Contains(orderField)) orderField = orderFileds[0];
            if (!orderTypes.Contains(orderType)) orderType = orderTypes[0];
            var memberCount = DaSettle.CountBetMember(user, fromDate, toDate);
            var source = DaSettle.ListMemberSettleResult(user, fromDate, toDate, orderField, orderType, base.GetStart(pageIndex), base.GetEnd(pageIndex));
            return new PagedList<SettleResult>(source, pageIndex, pageSize, memberCount);
        }
        public IEnumerable<FullStatement> GetFullStatement(User user, DateTime fromDate, DateTime toDate)
        {
            return DaSettle.GetFullStatement(user, fromDate, toDate);
        }
        public FullStatement GetUserStatement(User user, DateTime fromDate, DateTime toDate)
        {
            return DaSettle.GetUserFullStatement(user, fromDate, toDate);
        }
        public IEnumerable<SettleResult> GetSelfSettleResult(User user, DateTime fromDate, DateTime toDate)
        {
            var settleResult = DaSettle.GetSettleResult(user, fromDate, toDate);
            return MergerResult(settleResult);
        }

        /// <summary>
        /// 合并结果，将结果中同一个用户的记录汇总到一条
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        private IList<SettleResult> MergerResult(IEnumerable<SettleResult> source)
        {
            List<SettleResult> settleList = new List<SettleResult>();
            foreach (var gp in source.GroupBy(it => it.UserId))
            {
                SettleResult settle = null; ;
                bool isFirst = true; ;
                foreach (var settleItem in gp)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        settle = settleItem;
                        settleList.Add(settle);
                        continue;
                    }
                    settle.OrderCount += settleItem.OrderCount;
                    settle.BetTurnover += settleItem.BetTurnover;
                    settle.TotalCommission += settleItem.TotalCommission;
                    settle.Commission += settleItem.Commission;
                    settle.WinLost += settleItem.WinLost;
                    settle.TotalWinLost += settleItem.TotalWinLost;
                    settle.UpperCommission += settleItem.UpperCommission;
                    settle.UpperWinLost += settleItem.UpperWinLost;
                    settle.UpperTotalWinLost += settleItem.UpperTotalWinLost;
                    settle.CompanyWinLost += settleItem.CompanyWinLost;
                }
            }
            return settleList;
        }
        #endregion

        public void Dispose()
        {
            this.AllOrders = null;
            this._userBetOrderDic = null;
            this._memberSettleResult = null;
            this._settleResult = null;
            GC.Collect();
        }
    }
}
