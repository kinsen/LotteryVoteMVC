using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    public class OrderManager : ManagerBase
    {
        private BetSheetDataAccess _daSheet;
        private BetOrderDataAccess _daOrder;
        private OrderAncestorCommInfoDataAccess _daOac;
        public BetSheetDataAccess DaSheet
        {
            get
            {
                if (_daSheet == null)
                    _daSheet = new BetSheetDataAccess();
                return _daSheet;
            }
        }
        public BetOrderDataAccess DaOrder
        {
            get
            {
                if (_daOrder == null)
                    _daOrder = new BetOrderDataAccess();
                return _daOrder;
            }
        }
        public OrderAncestorCommInfoDataAccess DaOAC
        {
            get
            {
                if (_daOac == null)
                    _daOac = new OrderAncestorCommInfoDataAccess();
                return _daOac;
            }
        }

        public void Backup(DateTime date)
        {
            DaSheet.ExecuteWithTransaction(() =>
            {
                DaOrder.Tandem(DaSheet);
                DaSheet.BackupSheet(date);
                DaOrder.BackupOrder(date);
                DaSheet.RemoveOld(date);
                DaOrder.RemoveOld(date);
            });
        }

        #region Orders
        /// <summary>
        ///获取指定用户今日有效的注单之和，并用CompanyId_GamePlayWayId_Num格式作为Key生成哈希字典
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IDictionary<string, BetOrder> GetTodayValidOrderTotal(User user)
        {
            return DaOrder.GetOrdersTotals(user, BetStatus.Valid, DateTime.Today)
                .ToDictionary(it => string.Format("{0}_{1}_{2}", it.CompanyId, it.GamePlayWayId, it.Num), it => it);
        }
        /// <summary>
        /// 获取指定用户(包括下属用户)今日的未结算注单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        /// <returns></returns>
        public PagedList<BetSheet> GetTodayUnSettleSheets(User user, int pageIndex)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            var status = new[] { BetStatus.Invalid, BetStatus.Valid };
            return new PagedList<BetSheet>(DaSheet.ListBetSheet(user, status, DateTime.Today, start, end), pageIndex, pageSize,
                DaSheet.CountBetSheet(user, DateTime.Today, status));
        }
        /// <summary>
        ///获取会员(必须是会员)的注单(如果存在sheet，则获取指定sheet的注单，否则获取所有注单)
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="sheetId">The sheet id.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetMemberBetOrder(User member, int? sheetId, int pageIndex)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("Role", string.Format("用户:{0}不是Guest用户", member));
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            IEnumerable<BetOrder> orders;
            int totalCount;
            if (sheetId.HasValue)
            {
                var sheet = DaSheet.GetBetSheet(sheetId.Value);
                if (sheet == null || sheet.UserId != member.UserId)
                    throw new NoPermissionException("会员查看Sheet注单", string.Format("SheetId:{0},User:{1}", sheetId.Value, member));
                orders = DaOrder.GetOrdersBySheet(sheetId.Value, start, end);
                totalCount = DaOrder.CountOrder(sheetId.Value);
            }
            else
            {
                orders = DaOrder.GetOrders(member.UserId, DateTime.Today, start, end);
                totalCount = DaOrder.CountOrder(member.UserId, DateTime.Today);
            }
            return new PagedList<BetOrder>(orders, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 搜索会员今日的注单
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="status">The status.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortType">Type of the sort.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> SearchMemberTodayOrderByCondition(User member, int? sheetId, BetStatus status, string num, int companyId, int gameplaywayId, string sortField, string sortType, int pageIndex)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("Role", string.Format("用户:{0}不是Guest用户", member));
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            if (sheetId.HasValue)
            {
                var sheet = DaSheet.GetBetSheet(sheetId.Value);
                if (sheet == null || sheet.UserId != member.UserId)
                    throw new NoPermissionException("会员查看Sheet注单", string.Format("SheetId:{0},User:{1}", sheetId.Value, member));
            }
            return new PagedList<BetOrder>(DaOrder.ListOrderByCondition(sheetId, member.UserId, status, DateTime.Today, num, companyId, gameplaywayId, sortField, sortType, start, end),
                pageIndex, pageSize, DaOrder.CountOrderByCondition(sheetId, member.UserId, status, DateTime.Today, num, companyId, gameplaywayId));
        }
        /// <summary>
        /// 获取Sheet的Order(可获取下属用户Sheet)
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetUserBetOrder(User user, int sheetId, int pageIndex)
        {
            var sheet = DaSheet.GetBetSheet(sheetId);
            User betUser;

            if (sheet == null || !ManagerHelper.Instance.GetManager<UserManager>().IsParent(user.UserId, sheet.UserId, out betUser))
                throw new NoPermissionException("Get Bet Order", string.Format("用户:{0}没有查看Sheet：{1}的权限", user.UserId, sheetId));
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            return new PagedList<BetOrder>(DaOrder.GetOrdersBySheet(sheetId, start, end), pageIndex, pageSize, DaOrder.CountOrder(sheetId));
        }
        public PagedList<BetOrder> GetUserBetOrder(User user, string num, int companyId, int gameplaywayId, int pageIndex)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            return new PagedList<BetOrder>(DaOrder.ListDescendantOrderByCondition(user.UserId, BetStatus.Valid, num, companyId, gameplaywayId, DateTime.Today, DateTime.Today, start, end),
                pageIndex, pageSize, DaOrder.CountDescendantOrderCondition(user.UserId, BetStatus.Valid, num, companyId, gameplaywayId, DateTime.Today, DateTime.Today));
        }
        /// <summary>
        /// 获取用户（包括下属用户）已结算的注单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetUserSettleOrder(User user, string num, int companyId, int gameplaywayId, DateTime fromDate, DateTime toDate, WinLost winlost, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<BetOrder>(DaOrder.ListDescendantOrderByCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, start, end, winlost),
                pageIndex, pageSize, DaOrder.CountDescendantOrderCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, winlost));
        }
        public PagedList<BetOrder> SearchUserTodayOrderByCondition(User user, int? sheetId, BetStatus status, string num, int companyId, int gameplaywayId, string sortField, string sortType, int pageIndex)
        {
            IEnumerable<BetOrder> orders;
            int totalCount;
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            if (sheetId.HasValue)
            {
                User betUser;
                var sheet = DaSheet.GetBetSheet(sheetId.Value);
                if (sheet == null || !ManagerHelper.Instance.GetManager<UserManager>().IsParent(user.UserId, sheet.UserId, out betUser))
                    throw new NoPermissionException("Get Bet Order", string.Format("用户:{0}没有查看Sheet：{1}的权限", user.UserId, sheetId));
                orders =
                DaOrder.ListOrderByCondition(sheetId, sheet.UserId, status, DateTime.Today, num, companyId, gameplaywayId, sortField, sortType, start, end);
                totalCount =
                DaOrder.CountOrderByCondition(sheetId, sheet.UserId, status, DateTime.Today, num, companyId, gameplaywayId);
            }
            else
            {
                orders = DaOrder.ListDescendantOrderByCondition(user.UserId, status, num, companyId, gameplaywayId, DateTime.Today, DateTime.Today, start, end);
                totalCount = DaOrder.CountDescendantOrderCondition(user.UserId, status, num, companyId, gameplaywayId, DateTime.Today, DateTime.Today);
            }
            return new PagedList<BetOrder>(orders, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 根据条件获取会员的输赢注单
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="winlost">The winlost.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetMemberWinLost(User member, string num, int companyId, int gameplaywayId, WinLost winlost,
            DateTime fromDate, DateTime toDate, int pageIndex)
        {
            if (member.Role != Role.Guest) throw new InvalidDataException("Role", string.Format("用户:{0}不是Guest用户", member));
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            return new PagedList<BetOrder>(DaOrder.ListOrderByCondition(member, BetStatus.Settled, num, companyId, gameplaywayId, winlost, fromDate, toDate, start, end),
                pageIndex, pageSize, DaOrder.CountOrderByCondition(member, BetStatus.Settled, num, companyId, gameplaywayId, winlost, fromDate, toDate));
        }
        /// <summary>
        /// 获取会员结单
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetStatement(User member, out BetOrder lastWeekTotal)
        {
            if (member.Role != Role.Guest) throw new NoPermissionException("获取会员结单", member.ToString());

            int dayCount = 14;
            int diffDay = 0;
            if (DateTime.Today.DayOfWeek == DayOfWeek.Sunday) diffDay = 7;
            else diffDay = (int)DateTime.Today.DayOfWeek;

            int count = diffDay + 7 > LotterySystem.Current.StatementOrderCount ? LotterySystem.Current.StatementOrderCount : diffDay + 7;
            var endDate = DateTime.Today.AddDays(-1 * count);
            var statememt = DaOrder.GetStatementOrder(member, dayCount);

            #region 获取上周总结

            var lastMonday = DateTime.Today.AddDays(-1 * (diffDay + 6));
            var lastSunday = lastMonday.AddDays(6);

            var lastWeekTotals = statememt.Where(it => it.CreateTime >= lastMonday && it.CreateTime <= lastSunday);
            lastWeekTotal = new BetOrder { CreateTime = lastSunday };
            foreach (var order in lastWeekTotals)
            {
                if (lastWeekTotal == null)
                {
                    lastWeekTotal = order;
                    lastWeekTotal.CreateTime = lastSunday;
                }
                else
                {
                    lastWeekTotal.Turnover += order.Turnover;
                    lastWeekTotal.Commission += order.Commission;
                    lastWeekTotal.NetWin += order.NetWin;
                    lastWeekTotal.DrawResult += order.DrawResult;
                    lastWeekTotal.CancelAmount += order.CancelAmount;
                }
            }
            #endregion

            var result = statememt.Where(it => it.CreateTime >= lastMonday);
            if (result.Count() < dayCount)
            {
                List<BetOrder> orders = new List<BetOrder>(result);
                List<DateTime> dates = new List<DateTime>();
                for (int i = 0; i < dayCount; i++)
                    dates.Add(DateTime.Today.AddDays(-1 * i));
                var notExistDate = dates.Except(result.Select(it => it.CreateTime));
                orders.AddRange(notExistDate.Select(it => new BetOrder { CreateTime = it }));
                result = orders;
            }
            int takeCount = (int)DateTime.Today.DayOfWeek + 7;
            if (takeCount > 10 || takeCount <= 7)
                takeCount = 10;
            return result.OrderBy(it => it.CreateTime).Skip(dayCount - takeCount).Take(count);
        }
        /// <summary>
        ///获取用户(可获取下属用户)的取消注单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="date">The date.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetCancelOrders(User user, DateTime date, string num, int companyId, int gameplaywayId, int pageIndex)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            IEnumerable<BetOrder> orders;
            int totalCount;
            if (user.Role == Role.Guest)
            {
                orders = DaOrder.ListOrderByCondition(user, BetStatus.Invalid, string.Empty, companyId, gameplaywayId, WinLost.All, date, date, start, end);
                totalCount = DaOrder.CountOrderByCondition(user, BetStatus.Invalid, string.Empty, companyId, gameplaywayId, WinLost.All, date, date);
            }
            else
            {
                orders = DaOrder.ListDescendantOrderByCondition(user.UserId, BetStatus.Invalid, num, companyId, gameplaywayId, date, date, start, end);
                totalCount = DaOrder.CountDescendantOrderCondition(user.UserId, BetStatus.Invalid, num, companyId, gameplaywayId, date, date);
            }
            return new PagedList<BetOrder>(orders, pageIndex, pageSize, totalCount);
        }
        /// <summary>
        /// 获取用户赢的注单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<BetOrder> GetWinOrders(User user, string num, int companyId, int gameplaywayId, DateTime fromDate, DateTime toDate, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<BetOrder>(DaOrder.ListDescendantOrderByCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, start, end, WinLost.Win),
                pageIndex, pageSize, DaOrder.CountDescendantOrderCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, WinLost.Win));
        }
        public PagedList<BetOrder> GetWinOrders(User user, string num, int companyId, int gameplaywayId, DateTime fromDate, DateTime toDate, string sortField, string sortType, int pageIndex)
        {
            switch (sortField)
            {
                case BetOrder.ORDERID: break;
                case BetOrder.AMOUNT: break;
                case BetOrder.TURNOVER: break;
                case BetOrder.NETAMOUNT: break;
                case BetOrder.CREATETIME: break;
                default: sortField = BetOrder.DRAWRESULT; break;
            }
            switch (sortType.ToUpper())
            {
                case "ASC": break;
                case "DESC": break;
                default: sortType = "DESC"; break;
            }
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            var result = DaOrder.ListDescendantOrderByCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, start, end, WinLost.Win, sortField, sortType);
            var count = DaOrder.CountDescendantOrderCondition(user.UserId, BetStatus.Settled, num, companyId, gameplaywayId, fromDate, toDate, WinLost.Win);
            return new PagedList<BetOrder>(result, pageIndex, pageSize, count);
        }
        /// <summary>
        /// 获取公司统计
        /// </summary>
        /// <param name="companyId">The company.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetCompanyRanking(int companyId, DateTime fromDate, DateTime toDate)
        {
            return DaOrder.ListCompanyRanking(companyId, fromDate, toDate);
        }
        /// <summary>
        /// 今日号码统计
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="gameTypes">The game types.</param>
        /// <returns></returns>
        public PagedList<BetNumInfo> GetTodayMaxAmountNum(User user, int pageIndex, GameType gameType)
        {
            int totalCount;
            var nums = GetMaxAmountNum(user, gameType, pageIndex, out totalCount);
            return new PagedList<BetNumInfo>(nums, pageIndex, pageSize, totalCount);
        }
        private IEnumerable<BetNumInfo> GetMaxAmountNum(User user, GameType gt, int pageIndex, out int totalCount)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;

            //只取头尾包组
            var gpws = LotterySystem.Current.GamePlayWays
                .Where(it => it.GameType == gt && (it.PlayWay == PlayWay.Head || it.PlayWay == PlayWay.Last || it.PlayWay == PlayWay.Roll))
                .Select(it => it.Id).ToArray();

            totalCount = DaOrder.CountMaxAmountOrderByNum(user.UserId, gpws, BetStatus.Invalid, DateTime.Today);
            var data = DaOrder.ListMaxAmountOrderByNum(user.UserId, gpws, BetStatus.Invalid, DateTime.Today, start, end);
            List<BetNumInfo> numInfoList = new List<BetNumInfo>();
            //根据号码分组
            foreach (var numData in data.AsEnumerable().GroupBy(it => it.Field<string>(BetOrder.NUM)))
            {
                BetNumInfo numInfo = new BetNumInfo();
                numInfo.Num = numData.Key;
                numInfo.Contents = new Dictionary<int, List<BetNumContent>>();
                //根据公司分组
                foreach (var comData in numData.GroupBy(it => it.Field<int>(BetOrder.COMPANYID)))
                {
                    List<BetNumContent> contentList = new List<BetNumContent>();
                    //根据玩法分组
                    foreach (var gpwData in comData.GroupBy(it => it.Field<int>(BetOrder.GAMEPLAYWAYID)))
                    {
                        var content = new BetNumContent
                        {
                            GamePlayWayId = gpwData.Key,
                            Amount = gpwData.Sum(it => it.Field<decimal>(BetOrder.AMOUNT))
                        };
                        contentList.Add(content);
                        numInfo.Amount += content.Amount;
                    }
                    numInfo.Contents.Add(comData.Key, contentList);
                }
                numInfoList.Add(numInfo);
            }
            Func<BetNumInfo, BetNumInfo, int> compare = (a, b) => { if (a.Amount > b.Amount)return -1; if (a.Amount < b.Amount)return 1; else return 0; };
            numInfoList.Sort(new Comparison<BetNumInfo>(compare));
            return numInfoList;
        }
        public IEnumerable<BetOrder> GetTodayTotalChildReport(User user, Role role)
        {
            return DaOrder.ListChildBetAmounts(user.UserId, role, BetStatus.Valid, DateTime.Today);
        }
        #endregion

        #region Sheets
        public PagedList<NumAmountRanking> GetNumAmountRanking(User user, int companyId, int gpwId, string num, int pageIndex)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            return
                new PagedList<NumAmountRanking>(DaOrder.GetNumAmountRankings(user.UserId, companyId, gpwId, num, start, end), pageIndex, pageSize,
                DaOrder.CountNumAmountRanking(user.UserId,companyId, gpwId, num));

        }
        public PagedList<BetSheet> GetTodaySheets(User user, BetStatus status, string num, int pageIndex)
        {
            int start = (pageIndex - 1) * pageSize + 1;
            int end = pageIndex * pageSize;
            return
                string.IsNullOrEmpty(num) ?
                new PagedList<BetSheet>(DaSheet.GetSheets(user, status, DateTime.Today, start, end), pageIndex, pageSize,
                DaSheet.CountSheets(user, status, DateTime.Today))
                :
                new PagedList<BetSheet>(DaSheet.GetSheets(user, status, DateTime.Today, num, start, end), pageIndex, pageSize, DaSheet.CountSheets(user, status, DateTime.Today, num));
        }
        public PagedList<BetSheet> SearchUserSheets(User user, string userName, BetStatus status, string num, int pageIndex)
        {
            var userManager = ManagerHelper.Instance.GetManager<UserManager>();
            var targetUser = userManager.DaUser.GetUserByUserName(userName);
            if (targetUser == null) return PagedList<BetSheet>.Empty;
            if (!userManager.IsParent(user.UserId, targetUser.UserId, out targetUser)) return PagedList<BetSheet>.Empty;
            return GetTodaySheets(targetUser, status, num, pageIndex);
        }
        #endregion

        #region OAC
        public IEnumerable<OrderAncestorCommInfo> GetOac(int orderId)
        {
            return DaOAC.GetAncestorComms(orderId);
        }

        /// <summary>
        /// 获取今日指定用户，公司的注单分成信息
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="state">The state.</param>
        /// <returns></returns>
        public IEnumerable<OrderAncestorCommInfo> GetTodayOac(User user, int companyId, BetStatus state)
        {
            return DaOAC.GetAncestorComms(user, companyId, state, DateTime.Today);
        }
        #endregion
    }
}
