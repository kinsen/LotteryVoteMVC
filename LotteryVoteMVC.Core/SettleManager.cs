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
        private MemberWinLostDataAccess _daMemberWL;
        public MemberWinLostDataAccess DaMemberWL
        {
            get
            {
                if (_daMemberWL == null)
                    _daMemberWL = new MemberWinLostDataAccess();
                return _daMemberWL;
            }
        }
        private ShareRateWLDataAccess _daShareRateWL;
        public ShareRateWLDataAccess DaShareRateWL
        {
            get
            {
                if (_daShareRateWL == null)
                    _daShareRateWL = new ShareRateWLDataAccess();
                return _daShareRateWL;
            }
        }
        private OutcomeDataAccess _daOutcome;
        public OutcomeDataAccess DaOutCome
        {
            get
            {
                if (_daOutcome == null)
                    _daOutcome = new OutcomeDataAccess();
                return _daOutcome;
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
        internal IList<BetOrder> AllOrders { get; private set; }
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
        private List<ShareRateWL> _shareRateWL;
        protected List<ShareRateWL> ShareRateWLs
        {
            get
            {
                if (_shareRateWL == null)
                    _shareRateWL = new List<ShareRateWL>();
                return _shareRateWL;
            }
        }
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
            //return DaSettle.CountSettleCountByCompany(companyId, DateTime.Today) == 0;
            return DaShareRateWL.CountSettleCountByCompany(companyId, DateTime.Today) == 0;
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
                    AllOrders = BetManager.SettleBetOrder(companyId).ToList();
                    BetUsers = UserManager.GetBetUsers();
                    var companys = BetUsers.Where(it => it.Role == Role.Company); //UserManager.DaUser.GetUserByRole(Models.Role.Company);
                    foreach (var cp in companys)
                        GetShareRateWinLost(cp);
                    //GetSettleResult(cp);
                    var winlost = BuildMemberWinLost(ShareRateWLs);
                    //DaSettle.Insert(SettleResultList);
                    DaShareRateWL.Insert(ShareRateWLs);
                    DaMemberWL.Insert(winlost);
                    List<MemberWLParentCommission> comms = new List<MemberWLParentCommission>();
                    foreach (var wl in winlost)
                        comms.AddRange(wl.ParentCommission);
                    DaMemberWL.InsertParentCommission(comms);

                    var outcomes = ShareRateWLs.Select(it => it.OutCome);
                    DaOutCome.Insert(outcomes);
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
            if (!M_IsSetting)
            {
                M_IsSetting = true;
                try
                {
                    SettleCompany = companyId;
                    //DaSettle.Delete(companyId, DateTime.Today);
                    DaMemberWL.Delete(companyId, DateTime.Today);
                    DaShareRateWL.Delete(companyId, DateTime.Today);
                    DaOutCome.Delete(companyId, DateTime.Today);
                    AllOrders = BetManager.ResettleBetOrder(companyId).ToList();
                    BetUsers = UserManager.GetBetUsers();
                    var companys = BetUsers.Where(it => it.Role == Role.Company);// UserManager.DaUser.GetUserByRole(Models.Role.Company);
                    foreach (var cp in companys)
                        GetShareRateWinLost(cp);
                    //GetSettleResult(cp);
                    var winlost = BuildMemberWinLost(ShareRateWLs);
                    //DaSettle.Insert(SettleResultList);
                    DaShareRateWL.Insert(ShareRateWLs);
                    DaMemberWL.Insert(winlost);
                    List<MemberWLParentCommission> comms = new List<MemberWLParentCommission>();
                    foreach (var wl in winlost)
                        comms.AddRange(wl.ParentCommission);
                    DaMemberWL.InsertParentCommission(comms);

                    var outcomes = ShareRateWLs.Select(it => it.OutCome);
                    DaOutCome.Insert(outcomes);

                }
                finally
                {
                    M_IsSetting = false;
                    Dispose();
                }
            }
        }

        #region Private
        /// <summary>
        /// 构建用户输赢
        /// </summary>
        /// <param name="results">The results.</param>
        /// <returns></returns>
        private List<MemberWinLost> BuildMemberWinLost(IList<ShareRateWL> results)
        {
            List<MemberWinLost> list = new List<MemberWinLost>();
            var guestWL = BuildGuestMemberWL(results);
            list.AddRange(guestWL);

            var agent = BuildAgentsMemberWL(results, guestWL);
            var master = BuildAgentsMemberWL(results, agent);
            var super = BuildAgentsMemberWL(results, master);
            list.AddRange(agent);
            list.AddRange(master);
            list.AddRange(super);
            return list;
        }
        /// <summary>
        /// 获取会员以外各级代理的会员报表.
        /// </summary>
        /// <param name="results">传入分成报表主要是为了获得结果中的用户信息，避免再一次获取用户.</param>
        /// <param name="childWL">The child WL.</param>
        /// <returns></returns>
        private List<MemberWinLost> BuildAgentsMemberWL(IList<ShareRateWL> results, IList<MemberWinLost> childWL)
        {
            Dictionary<int, MemberWinLost> wlDict = new Dictionary<int, MemberWinLost>();
            foreach (var child in childWL)
            {
                MemberWinLost wl;
                if (!wlDict.TryGetValue(child.User.ParentId, out wl))
                {
                    wl = new MemberWinLost();
                    wl.CompanyId = child.CompanyId;
                    wl.UserId = child.User.ParentId;
                    var shareWl = results.Find(it => it.UserId == child.User.ParentId);
                    wl.User = shareWl.User;
                    wl.ShareRate = shareWl.User.UserInfo.RateGroup.ShareRate;
                    wl.ShareRateWL = shareWl;
                    wlDict[child.User.ParentId] = wl;
                }
                wl.OrderCount += child.OrderCount;
                wl.BetTurnover += child.BetTurnover;
                wl.WinLost += child.WinLost;
                wl.CompanyWL += child.CompanyWL;
                wl.ShareRateWL.CompanyWL += child.CompanyWL;
                wl.TotalCommission += child.ParentCommission.Where(it => it.Role == wl.User.Role).Sum(it => it.Commission);
                wl.TotalWinLost = wl.TotalCommission + wl.WinLost;
                SumParentCommission(wl, child.ParentCommission.Where(it => it.Role < wl.User.Role));
            }
            return wlDict.Values.ToList();
        }
        private void SumParentCommission(MemberWinLost wl, IEnumerable<MemberWLParentCommission> addition)
        {
            if (wl.ParentCommission == null || wl.ParentCommission.Count() == 0)
            {
                var list = new List<MemberWLParentCommission>();
                foreach (var tmp in addition)
                {
                    var comm = new MemberWLParentCommission
                    {
                        UserId = wl.UserId,
                        CompanyId = tmp.CompanyId,
                        RoleId = tmp.RoleId,
                        Commission = tmp.Commission
                    };
                    list.Add(comm);
                }
                wl.ParentCommission = list;
                return;
            }
            foreach (var item in wl.ParentCommission)
            {
                var tmp = addition.Find(x => x.RoleId == item.RoleId);
                if (tmp != null)
                    item.Commission += tmp.Commission;
            }

        }
        private List<MemberWinLost> BuildGuestMemberWL(IList<ShareRateWL> results)
        {
            List<MemberWinLost> list = new List<MemberWinLost>();
            //获取所有用户级别的结算记录
            foreach (var result in results.Where(it => it.User.Role == Role.Guest))
            {
                MemberWinLost wl = new MemberWinLost();
                wl.CompanyId = result.CompanyId;
                wl.UserId = result.UserId;
                wl.User = result.User;
                wl.ShareRate = result.User.UserInfo.RateGroup.ShareRate;
                wl.OrderCount = result.OrderCount;
                wl.BetTurnover = result.SumTurnover;
                wl.TotalWinLost = result.SumWinLost;
                wl.WinLost = result.SumWinLost - result.Commission;
                wl.TotalCommission = result.Commission;
                decimal superComm = 0;
                List<MemberWLParentCommission> parentComms = new List<MemberWLParentCommission>();
                for (int i = (int)Role.Company; i < (int)Role.Guest; i++)
                {
                    var comm = new MemberWLParentCommission();
                    comm.UserId = result.UserId;
                    comm.CompanyId = result.CompanyId;
                    comm.RoleId = i;
                    comm.Commission = UserBetOrderDic[result.User.UserId].Select(it => GetOAC(it.OrderId, (Role)i).CommAmount).Sum();
                    parentComms.Add(comm);
                    if (i == (int)Role.Super)
                        superComm = comm.Commission;
                }
                wl.ParentCommission = parentComms;
                //公司输赢=（会员总输赢-会员佣金+super佣金）*（1-会员分成）
                wl.CompanyWL = (wl.WinLost + superComm) * (decimal)(1 - wl.ShareRate);
                result.CompanyWL = wl.CompanyWL;
                list.Add(wl);
            }
            return list;
        }

        private List<ShareRateWL> BuildShareRateWL(IList<SettleResult> results)
        {
            List<ShareRateWL> list = new List<ShareRateWL>();
            foreach (var result in results.Where(it => it.User.Role != Role.Guest))
            {
                ShareRateWL wl = new ShareRateWL();
                var parent = results.Find(it => it.UserId == result.User.ParentId);
                double parentShareRate = parent == null ? 1 : parent.User.UserInfo.RateGroup.ShareRate;
                decimal netShareRate = (decimal)(parentShareRate - result.User.UserInfo.RateGroup.ShareRate);

                wl.CompanyId = result.CompanyId;
                wl.UserId = result.UserId;
                wl.ShareRate = result.User.UserInfo.RateGroup.ShareRate;
                wl.OrderCount = result.OrderCount;
                wl.BetTurnover = result.BetTurnover;
                wl.WinLost = result.WinLost * netShareRate;
                wl.TotalComm = result.Commission * netShareRate;
                wl.TotalWinLost = result.TotalWinLost * netShareRate;
                list.Add(wl);
            }
            return list;
        }

        private IList<ShareRateWL> GetShareRateWinLost(User user)
        {
            List<ShareRateWL> shareRateWLList = new List<ShareRateWL>();     //用于盛放子用户的结余记录

            if (user.Role == Role.Guest && BetUsers.Contains(it => it.UserId == user.UserId)) //会员结算，必须包含在今日下注用户中，减少多余的无效计算
            {
                var result = CalcMemberShareRateWL(user);
                if (result != null)
                {
                    shareRateWLList.Add(result);
                    ShareRateWLs.Add(result);
                }
                return shareRateWLList;
            }

            var childUser = BetUsers.Where(it => it.ParentId == user.UserId);
            List<BetOrder> betOrderList = new List<BetOrder>();                 //用于存放本级用户可用到的注单
            foreach (var child in childUser)
            {
                var childWL = GetShareRateWinLost(child);
                shareRateWLList.AddRange(childWL);              //迭代直到所有Guest级别的用户都结余完毕
                betOrderList.AddRange(UserBetOrderDic[child.UserId]);           //将子用户的注单也添加到本级用户的注单容器中
            }
            UserBetOrderDic.Add(user.UserId, betOrderList);

            if (user.Role == Role.Company)        //到公司级别不进行上级结余计算（公司为最顶级了）
                return shareRateWLList;

            var userWL = CalcUserShareRateWL(user, shareRateWLList);
            ShareRateWLs.Add(userWL);
            return new List<ShareRateWL> { userWL };
        }

        private ShareRateWL CalcMemberShareRateWL(User user)
        {
            //获取会员的下注单
            var orderList = GetOrders(user).ToList();
            UserBetOrderDic[user.UserId] = orderList;

            if (orderList == null || orderList.Count == 0)
                return null;
            var oacs = OrderManager.GetTodayOac(user, SettleCompany, BetStatus.Settled).GroupBy(it => it.OrderId).ToDictionary(it => it.Key, it => it.Select(x => x));
            OrderOacDic.AddRange(oacs);

            ShareRateWL wl = new ShareRateWL();
            wl.User = user;
            wl.UserId = user.UserId;
            wl.ShareRate = user.UserInfo.RateGroup.ShareRate;
            OutCome outcome = new OutCome { UserId = user.UserId };
            outcome.WinOrders = new List<BetOrder>();
            wl.OutCome = outcome;
            bool isFirst = true;
            foreach (var order in orderList)
            {
                if (isFirst)
                {
                    isFirst = false;
                    wl.CompanyId = order.CompanyId;
                }
                order.User = user;
                wl.OrderCount += 1;
                wl.SumTurnover += order.Turnover;
                wl.SumWinLost += order.DrawResult;
                wl.Commission += order.Commission;
                outcome.NetAmount += order.NetAmount;
                if (order.DrawResult > 0)
                    outcome.WinOrders.Add(order);
            }


            //如果分成是0，则说明没有占成数，所以分成应该是0
            var shareRate = (decimal)(1 - user.UserInfo.RateGroup.ShareRate);

            wl.BetTurnover = wl.SumTurnover * shareRate;    //下注额=会员下注额*分成
            wl.MemberWinLost = wl.SumWinLost - wl.Commission;   //会员输赢=会员总输赢-佣金
            wl.WinLost = wl.MemberWinLost * shareRate;         //输赢=会员输赢*分成
            wl.TotalComm = wl.Commission * shareRate;       //总佣金=本层总佣金*分成
            wl.TotalWinLost = wl.WinLost + wl.TotalComm;    //总输赢=输赢+总佣金

            outcome.CompanyId = wl.CompanyId;
            outcome.OrderCount = wl.OrderCount;
            outcome.Turnover = wl.SumTurnover;
            outcome.JuniorNetAmount = outcome.NetAmount; //会员的本级净金额=下级的净金额
            outcome.JustWin = outcome.WinOrders.Count == 0 ? 0 : outcome.WinOrders.Sum(it => it.DrawResult + it.NetAmount);   //纯赢=输赢+净金额
            outcome.JuniorJustWin = outcome.JustWin;    //会员的下级纯赢=本级纯赢
            outcome.TotalWinLost = outcome.JuniorTotalWinLost = outcome.NetAmount - outcome.JustWin;

            return wl;
        }
        private ShareRateWL CalcUserShareRateWL(User user, List<ShareRateWL> childWL)
        {
            ShareRateWL wl = new ShareRateWL();
            wl.OutCome = new OutCome { UserId = user.UserId };
            var winOrders = new List<BetOrder>();
            wl.OutCome.WinOrders = winOrders;

            wl.UserId = user.UserId;
            wl.User = user;
            wl.ShareRate = user.UserInfo.RateGroup.ShareRate;
            bool isFirst = true;
            foreach (var item in childWL)
            {
                if (isFirst)
                {
                    isFirst = false;
                    wl.CompanyId = item.CompanyId;
                }
                wl.OrderCount += item.OrderCount;
                wl.SumTurnover += item.SumTurnover;
                wl.SumWinLost += item.SumWinLost;
                wl.BetTurnover += item.BetTurnover;
                wl.WinLost += item.WinLost;

                winOrders.AddRange(item.OutCome.WinOrders);
                wl.OutCome.JuniorJustWin += item.OutCome.JustWin;
                wl.OutCome.JuniorNetAmount += item.OutCome.NetAmount;
                wl.OutCome.JuniorTotalWinLost += item.OutCome.TotalWinLost;
            }

            //用户总佣金，累加子用户所有订单中该用户的佣金        各级总佣金=注单佣金×会员分成
            foreach (var order in UserBetOrderDic[user.UserId])
            {
                var comm = GetOAC(order.OrderId, user.Role).CommAmount;
                wl.Commission += comm;
                wl.TotalComm += comm * (decimal)(1 - order.User.UserInfo.RateGroup.ShareRate);

                //净金额=金额-本级佣金
                wl.OutCome.NetAmount += (order.Turnover - comm);
            }
            //var userTotalComm = UserBetOrderDic[user.UserId]
            //    .Select(it => GetOAC(it.OrderId, user.Role).CommAmount * (decimal)(it.User.UserInfo.RateGroup.ShareRate == 0 ? 1 : it.User.UserInfo.RateGroup.ShareRate))
            //    .Sum();

            //wl.TotalComm = userTotalComm;
            wl.TotalWinLost = wl.TotalComm + wl.WinLost;

            wl.OutCome.CompanyId = wl.CompanyId;
            wl.OutCome.OrderCount = wl.OrderCount;
            wl.OutCome.Turnover = wl.SumTurnover;
            wl.OutCome.JustWin = wl.OutCome.WinOrders.Sum(it => it.DrawResult + it.NetAmount);   //纯赢=输赢+净金额
            wl.OutCome.TotalWinLost = wl.OutCome.NetAmount - wl.OutCome.JustWin;

            return wl;
        }


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
            result.ShareRate = user.UserInfo.RateGroup.ShareRate;
            result.UpperShareRate = GetParentShareRate(user);
            result.UpperWinLost = result.WinLost * -1 * (decimal)result.UpperShareRate;   //代理输赢=会员输赢×代理成数
            result.CompanyWinLost = CountCompanyWinLost(1 - result.UpperShareRate, result.WinLost, result.TotalCommission);
            result.UpperTotalWinLost = result.TotalWinLost / -1 - result.CompanyWinLost;         //代理总输赢=会员总输赢-公司输赢
            result.UpperCommission = result.UpperTotalWinLost - result.UpperWinLost;        //代理佣金=代理总输赢-代理输赢
            result.UserId = user.UserId;
            result.User = user;
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
                result.User = user;
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
            //如果本级的分成是0，则说明上级可能是1或0，则将分成累积到下一层
            if (user.UserInfo.RateGroup.ShareRate == 0) return 0;
            User parent;
            if (!UserDic.TryGetValue(user.ParentId, out parent))
            {
                parent = UserManager.GetUser(user.ParentId);
                UserDic.Add(user.ParentId, parent);
            }
            var parentRate = parent.UserInfo.RateGroup.ShareRate;
            //如果父级的分成是0，则按1来算（0之上只能是1）
            if (parentRate == 0)
                parentRate = 1;
            return parentRate - user.UserInfo.RateGroup.ShareRate;
        }
        private IList<BetOrder> GetOrders(User user)
        {
            if (AllOrders == null)
                AllOrders = OrderManager.DaOrder.GetOrders(BetStatus.Settled, SettleCompany, DateTime.Today).ToList();
            return AllOrders.List(it => it.UserId == user.UserId);
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
        public IEnumerable<MemberWinLost> ListMemberWinLost(User user, DateTime fromDate, DateTime toDate)
        {
            var memberWL = DaMemberWL.ListMemberWinLost(user.UserId, fromDate, toDate);
            var agentComms = DaMemberWL.ListMemberWinLostAgentCommission(user.UserId, fromDate, toDate);
            var result = MergerWL(memberWL);
            result.ForEach(it => it.ParentCommission = agentComms.Where(x => x.UserId == it.UserId));
            return result;
        }
        public IEnumerable<ShareRateWL> ListShareRateWL(User user, DateTime fromDate, DateTime toDate)
        {
            var shareRateWL = DaShareRateWL.ListWinLost(user, fromDate, toDate);
            return MergerWL(shareRateWL);
        }
        public IEnumerable<OutCome> ListOutcome(User user, DateTime fromDate, DateTime toDate)
        {
            var outcomes = DaOutCome.ListOutCome(user, fromDate, toDate);
            return MergerOutCome(outcomes);
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
        private IList<MemberWinLost> MergerWL(IEnumerable<MemberWinLost> source)
        {
            List<MemberWinLost> list = new List<MemberWinLost>();
            foreach (var gp in source.GroupBy(it => it.UserId))
            {
                MemberWinLost wl = null;
                bool isFirst = true;
                foreach (var wlItem in gp)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        wl = wlItem;
                        list.Add(wl);
                        continue;
                    }
                    wl.OrderCount += wlItem.OrderCount;
                    wl.BetTurnover += wlItem.BetTurnover;
                    wl.TotalCommission += wlItem.TotalCommission;
                    wl.WinLost += wlItem.WinLost;
                    wl.TotalWinLost += wlItem.TotalWinLost;
                    wl.CompanyWL += wlItem.CompanyWL;
                }
            }
            return list;
        }
        private IList<ShareRateWL> MergerWL(IEnumerable<ShareRateWL> source)
        {
            List<ShareRateWL> list = new List<ShareRateWL>();
            foreach (var gp in source.GroupBy(it => it.UserId))
            {
                ShareRateWL wl = null;
                bool isFirst = true;
                foreach (var wlItem in gp)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        wl = wlItem;
                        list.Add(wl);
                        continue;
                    }
                    wl.OrderCount += wlItem.OrderCount;
                    wl.BetTurnover += wlItem.BetTurnover;
                    wl.WinLost += wlItem.WinLost;
                    wl.CompanyWL += wlItem.CompanyWL;
                    wl.TotalComm += wlItem.TotalComm;
                    wl.TotalWinLost += wlItem.TotalWinLost;
                }
            }
            return list;
        }
        private IList<OutCome> MergerOutCome(IEnumerable<OutCome> source)
        {
            List<OutCome> list = new List<OutCome>();
            foreach (var gp in source.GroupBy(it => it.UserId))
            {
                OutCome wl = null;
                bool isFirst = true;
                foreach (var wlItem in gp)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                        wl = wlItem;
                        list.Add(wl);
                        continue;
                    }
                    wl.OrderCount += wlItem.OrderCount;
                    wl.Turnover += wlItem.Turnover;
                    wl.NetAmount += wlItem.NetAmount;
                    wl.JuniorNetAmount += wlItem.JuniorNetAmount;
                    wl.TotalWinLost += wlItem.TotalWinLost;
                    wl.JuniorTotalWinLost += wlItem.JuniorTotalWinLost;
                    wl.JustWin += wlItem.JustWin;
                    wl.JuniorJustWin += wlItem.JuniorJustWin;
                }
            }
            return list;
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
