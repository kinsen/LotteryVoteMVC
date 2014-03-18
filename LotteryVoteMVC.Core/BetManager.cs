using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Bet;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Limit;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Core
{
    public class BetManager : ManagerBase
    {
        #region Properties
        private static object _lockHelper = new object();
        private BetSheetDataAccess _daSheet;
        private BetOrderDataAccess _daOrder;
        private RollbackAmountDataAccess _daRollbackAmount;
        private BetOrderBuilder _orderBuilder;
        private OrderAncestorCommInfoDataAccess _daAncestorComm;
        private LimitChecker _checker;
        internal LimitChecker Checker
        {
            get
            {
                if (_checker == null)
                    _checker = new LimitChecker();
                return _checker;
            }
            set
            {
                _checker = value;
            }
        }
        //internal BetOrderBuilder OrderBuilder
        //{
        //    get
        //    {
        //        if (_orderBuilder == null)
        //            _orderBuilder = new BetOrderBuilder
        //            {
        //                Checker = this.Checker
        //            };
        //        return _orderBuilder;
        //    }
        //    set
        //    {
        //        _orderBuilder = value;
        //    }
        //}
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
        public RollbackAmountDataAccess DaRollbackAmount
        {
            get
            {
                if (_daRollbackAmount == null)
                    _daRollbackAmount = new RollbackAmountDataAccess();
                return _daRollbackAmount;
            }
        }
        public OrderAncestorCommInfoDataAccess DaAncestorComm
        {
            get
            {
                if (_daAncestorComm == null)
                    _daAncestorComm = new OrderAncestorCommInfoDataAccess();
                return _daAncestorComm;
            }
        }
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }
        public FreezeFundsManager FreeFundsManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<FreezeFundsManager>();
            }
        }
        #endregion

        /// <summary>
        /// 下注
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="betList">The bet list.</param>
        /// <returns></returns>
        public BetResult AddBet(User member, LotterySpecies specie, IEnumerable<BetItem> betList)
        {
            var result = OrderBuilder.Build(member, specie, betList); //OrderBuilder.BuildOrder(member, specie, betList);
            AddBetSheet(member, result.Sheet, result.Result.ActualTurnover);
            return result.Result;
        }
        /// <summary>
        /// 12生肖，快速下单.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="betList">The bet list.</param>
        /// <returns></returns>
        public BetResult AddBet(User member, LotterySpecies specie, IEnumerable<AutoBetItem> betList)
        {
            //var betSheetDic = OrderBuilder.BuildAutoBetOrder(member, specie, betList);
            var result = OrderBuilder.Build(member, specie, betList);
            AddBetSheet(member, result.Sheet, result.Result.ActualTurnover);
            return result.Result;
        }
        public BetResult AddBet(User member, LotterySpecies specie, FastBetItem fastBetItem, GameType gameType = GameType.TwoDigital)
        {
            //var betSheetDic = OrderBuilder.BuildFastBetOrder(member, specie, fastBetItem, gameType);
            var result = OrderBuilder.Build(member, specie, fastBetItem, gameType);
            AddBetSheet(member, result.Sheet, result.Result.ActualTurnover);
            return result.Result;
        }
        public BetResult AddUnionPL2Bet(User member, LotterySpecies specie, IEnumerable<BetItem> betList)
        {
            var result = OrderBuilder.BuildUnionPL2(member, specie, betList);
            AddBetSheet(member, result.Sheet, result.Result.ActualTurnover);
            return result.Result;
        }
        /// <summary>
        ///取消大注单
        /// </summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="user">The user.</param>
        public void CancleSheet(int sheetId, User user)
        {
            var sheet = DaSheet.GetBetSheet(sheetId);
            if (sheet.Status != BetStatus.Valid) return;
            if (user.Role != Role.Company && !(user.UserId == sheet.UserId && sheet.CanCancel()))
                return;
            //throw new NoPermissionException("Cancel Sheet", string.Format("User:{0}没有取消Sheet:{1}的权限!", user.UserId, sheet.SheetId));

            var orders = DaOrder.GetOrdersBySheet(sheetId).ToList();
            var openCompanys = TodayLotteryCompany.Instance.GetOpenCompany();   //存在已关盘注单
            if (orders.Contains(it => !openCompanys.Contains(x => x.CompanyId == it.CompanyId))) return;
            var turnover = orders.Where(it => it.Status == BetStatus.Valid).Sum(it => it.Turnover);
            foreach (var order in orders)
                Checker.RollLimit(order);
            DaSheet.ExecuteWithTransaction(() =>
            {
                DaOrder.Tandem(DaSheet);
                FreeFundsManager.Tandem(DaSheet);
                DaOrder.UpdateStateBySheet(sheetId, BetStatus.Invalid);
                DaSheet.UpdateBetSheetStatus(sheetId, BetStatus.Invalid);
                FreeFundsManager.UnFreezeUserFunds(sheet.UserId, turnover);
                FreeFundsManager.ClearTandem();
            });
        }
        /// <summary>
        /// 取消小注单
        /// </summary>
        /// <param name="orderId">The order id.</param>
        /// <param name="user">The user.</param>
        public void CancleOrder(int orderId, User user)
        {
            var order = DaOrder.GetBetOrder(orderId);

            if (order.Status != BetStatus.Valid) return;

            if (user.Role != Role.Company && !(user.UserId == order.UserId && order.CanCancel())) return;
            //throw new NoPermissionException("Cancel betOrder", string.Format("User {0} can't cancel betOrder {1}", user.UserId, orderId));
            if (!TodayLotteryCompany.Instance.GetOpenCompany().Contains(it => it.CompanyId == order.CompanyId)) return;
            Checker.RollLimit(order);
            DaOrder.ExecuteWithTransaction(() =>
            {
                FreeFundsManager.Tandem(DaOrder); //串联事物
                DaOrder.UpdateState(order.OrderId, BetStatus.Invalid);
                FreeFundsManager.UnFreezeUserFunds(order.UserId, order.Turnover);   //解冻资金
                FreeFundsManager.ClearTandem();
            });
            var validOrderCount = DaSheet.CountOrder(order.SheetId, BetStatus.Valid);
            if (validOrderCount == 0)
                DaSheet.UpdateBetSheetStatus(order.SheetId, BetStatus.Invalid);
        }
        /// <summary>
        /// 结算指定公司的注单
        /// </summary>
        /// <param name="companyId">The company id.</param>
        internal IEnumerable<BetOrder> SettleBetOrder(int companyId)
        {
            var orderList = DaOrder.GetOrders(BetStatus.Valid, companyId, DateTime.Today).ToList();
            SettleOrders(orderList);
            var sheets = DaSheet.GetCanSettleSheetByCompany(companyId, DateTime.Today).ToList();

            string timeToken = DateTime.Now.Ticks.ToString();
            var rbAmounts = GetRollbackAmounts(orderList, timeToken);

            DaOrder.ExecuteWithTransaction(() =>
            {
                //foreach (var order in orderList)
                DaOrder.Update(orderList);

                DaRollbackAmount.Tandem(DaOrder);
                DaRollbackAmount.InsertAmounts(rbAmounts);
                DaRollbackAmount.RollbackAmounts(timeToken);

                DaSheet.Tandem(DaOrder);
                foreach (var sheet in sheets)
                    sheet.Status = BetStatus.Settled;
                DaSheet.Update(sheets);
                //foreach (var sheet in sheets)
                //    DaSheet.UpdateBetSheetStatus(sheet.SheetId, BetStatus.Settled);

                FreeFundsManager.ClearTandem();
            });
            return orderList;
        }
        internal IEnumerable<BetOrder> ResettleBetOrder(int companyId)
        {
            var orderList = DaOrder.GetOrders(BetStatus.Settled, companyId, DateTime.Today).ToList();
            SettleOrders(orderList);
            DaOrder.ExecuteWithTransaction(() =>
            {
                //foreach (var order in orderList)
                DaOrder.Update(orderList);
            });
            return orderList;
        }

        private void SettleOrders(IEnumerable<BetOrder> orders)
        {
            foreach (var order in orders)
            {
                int winmultiple = WinMultipleCalculator.GetCalculator().Calculate(order);   //获取输赢倍数
                decimal winResult;
                if (winmultiple > 0)
                    winResult = (order.Amount * (decimal)(order.Odds * winmultiple)) - order.NetAmount; //若倍数大于0，则为赢，赢钱=下注额×赔率×倍数-净额
                else
                    winResult = order.NetAmount * -1;       //若倍数小于等于0，则为输，输钱=净额×-1
                order.DrawResult = winResult;
                order.Status = BetStatus.Settled;
            }
        }
        /// <summary>
        /// 添加注单
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="betSheetDic">The bet sheet dic.</param>
        /// <param name="totalTurnover">The total turnover.</param>
        private void AddBetSheet(User member, IDictionary<BetSheet, IList<BetOrder>> betSheetDic, decimal totalTurnover)
        {
            Checker.Monitor(() =>
            DaSheet.ExecuteWithTransaction(() =>
            {
                FreeFundsManager.Tandem(DaSheet);
                FreeFundsManager.FreezeUserFunds(member, totalTurnover);
                InsertOrderToDataBase(betSheetDic);
                FreeFundsManager.ClearTandem();
            }), () =>
            {
                foreach (var sheet in betSheetDic.Values)
                    foreach (var order in sheet)
                        Checker.RollLimit(order);
            });
        }
        /// <summary>
        /// 将注单写入数据库
        /// </summary>
        /// <param name="betSheetDic">The bet sheet dic.</param>
        private void InsertOrderToDataBase(IDictionary<BetSheet, IList<BetOrder>> betSheetDic)
        {
            string ipAddress = IPHelper.IPAddress;
            List<OrderAncestorCommInfo> commInfoList = new List<OrderAncestorCommInfo>();
            DaOrder.Tandem(DaSheet);
            DaOrder.ExecuteWithTransaction(() =>
            {
                foreach (var betSheet in betSheetDic)
                {
                    if (betSheet.Value.Count == 0) continue;
                    var sheet = betSheet.Key;
                    sheet.IPAddress = ipAddress;
                    DaSheet.Insert(sheet);
                    foreach (var order in betSheet.Value)
                    {
                        order.SheetId = sheet.SheetId;
                        DaOrder.Insert(order);
                        order.AncestorCommission.ForEach(it =>
                        {
                            it.OrderId = order.OrderId;
                            commInfoList.Add(it);
                        });
                    }
                }
                try
                {
                    DaAncestorComm.Tandem(DaSheet);
                    DaAncestorComm.Insert(commInfoList);
                }
                catch
                {
                    foreach (var ci in commInfoList)
                        LogConsole.Debug(string.Format("OrderId:{0},RoleId:P{1},Comm:{2},CommAmount:{3}", ci.OrderId, ci.RoleId, ci.Commission, ci.CommAmount));
                    throw;
                }
            });
        }
        private IEnumerable<RollbackAmount> GetRollbackAmounts(IEnumerable<BetOrder> orders, string timetoken)
        {
            IDictionary<int, RollbackAmount> rollbackAmountDic = new Dictionary<int, RollbackAmount>();
            foreach (var order in orders)
            {
                RollbackAmount rbAmount;
                if (!rollbackAmountDic.TryGetValue(order.UserId, out rbAmount))
                {
                    rbAmount = new RollbackAmount
                    {
                        UserId = order.UserId,
                        TimeToken = timetoken,
                        Amount = 0
                    };
                    rollbackAmountDic.Add(order.UserId, rbAmount);
                }
                rbAmount.Amount += order.Turnover;
            }
            return rollbackAmountDic.Values;
        }
    }
}
