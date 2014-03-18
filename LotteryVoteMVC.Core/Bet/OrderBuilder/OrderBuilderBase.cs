using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Core.Limit;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using System.Web;

namespace LotteryVoteMVC.Core.Bet
{
    internal abstract class OrderBuilderBase<T>
    {
        private CommManager _commManager;
        public CommManager CommManager
        {
            get
            {
                if (_commManager == null)
                    _commManager = new CommManager();
                return _commManager;
            }
        }

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

        protected BetResult _betResult;
        public BetResult BetResult
        {
            get
            {
                if (_betResult == null)
                    _betResult = new BetResult();
                return _betResult;
            }
        }

        public abstract IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie, IEnumerable<T> betList, IDictionary<string, object> parameters);

        /// <summary>
        /// 合并各个大注单的下注金额说明
        /// </summary>
        /// <param name="sheets">The sheets.</param>
        /// <returns></returns>
        protected string JoinSheetBetAmount(IEnumerable<BetSheet> sheets)
        {
            Dictionary<int, WagerItem> wagerDic = new Dictionary<int, WagerItem>();
            foreach (var sheet in sheets)
            {
                var wagerInfos = sheet.BetAmount.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in wagerInfos)
                {
                    var wagerInfo = item.Split(new[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                    int gpwId = int.Parse(wagerInfo[0]);
                    decimal wager = decimal.Parse(wagerInfo[1]);
                    bool d = wagerInfo[2] == "1";
                    if (!wagerDic.ContainsKey(gpwId))
                        wagerDic.Add(gpwId, new WagerItem { Wager = wager, IsFullPermutation = false });
                    var wagerItem = wagerDic[gpwId];
                    //wagerItem.Wager += wager;
                    if (d && !wagerItem.IsFullPermutation)
                        wagerItem.IsFullPermutation = true;
                }
            }
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var wager in wagerDic)
            {
                if (isFirst) isFirst = false;
                else sb.Append("@");
                sb.AppendFormat("{0}/{1}/{2}", wager.Key, wager.Value.Wager, wager.Value.IsFullPermutation ? 1 : 0);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建一个注单并附加到一个Sheet上
        /// </summary>
        /// <param name="orderList">The order list.</param>
        /// <param name="companyType">Type of the company.</param>
        /// <param name="company">The company.</param>
        /// <param name="memberComm">The member comm.</param>
        /// <param name="num">The num.</param>
        /// <param name="wager">The wager.</param>
        /// <param name="user">The user.</param>
        /// <param name="fullArrangementNum">The full arrangement num.</param>
        /// <param name="isCon">if set to <c>true</c> [is con].</param>
        protected void AddOrderToDic(IList<BetOrder> orderList, CompanyType companyType, LotteryCompany company, Pair<CommissionGroup, IEnumerable<ConcreteCommission>> memberComm,
            string num, WagerItem wager, User user, IList<long> fullArrangementNum, bool isCon = false)
        {
            List<GamePlayWay> gamePlayWayList = new List<GamePlayWay>();
            var gamePlayWay = LotterySystem.Current.FindGamePlayWay(wager.GamePlayTypeId);     //找到下注的游戏玩法
            var playWay = gamePlayWay.PlayWay;
            if (playWay == PlayWay.HeadAndLast)     //头尾则分解为头，尾两注
            {
                Func<PlayWay, GamePlayWay> findPlayWayFun = (pw) =>
                {
                    return LotterySystem.Current.FindGamePlayWay(gamePlayWay.GameType, pw);
                };
                var headPlayWay = findPlayWayFun(PlayWay.Head);
                var lastPlayWay = findPlayWayFun(PlayWay.Last);
                gamePlayWayList.Add(headPlayWay);
                gamePlayWayList.Add(lastPlayWay);
            }
            else if (playWay == PlayWay.Roll7)
            { 
                if(companyType!=CompanyType.Hanoi)
                    gamePlayWayList.Add(gamePlayWay);
            }
            else
                gamePlayWayList.Add(gamePlayWay);
            Action<string, int, GamePlayWay> buildOrderToListFunc = (n, q, gpw) =>
            {
                var order = BuildBetOrder(n, q, memberComm, gpw, company, wager.Wager, user, isCon);
                this.BetResult.ExceptTurnover += order.Turnover;
                if (company.IsInBetTime(gpw) && Checker.Check(order, user))
                {
                    orderList.Add(order);
                    this.BetResult.ActualTurnover += order.Turnover;
                    Checker.BeInsertOrderList.Add(order);

                    if (Checker.IsDrop)
                        this.BetResult.DropOrderList.Add(order);
                }
                else
                {
                    this.BetResult.NotAcceptOrderList.Add(order);
                }
            };
            gamePlayWayList.ForEach(x =>
            {
                var counter = NumQuantityCounterFactory.GetFactory.GetCounter(x.GameType, companyType, LotterySystem.Current.GetNumLenSupport(companyType));
                var betNum = counter.GetRealyBetNum(num);
                int numQuantity = CountNumQuantity(betNum, x, companyType); //计算该玩法的号码长度
                if (wager.IsFullPermutation)    //是否全排列下注
                {
                    if (fullArrangementNum == null) //获取号码全排列
                        fullArrangementNum = MathHelper.GetFullArrangement(betNum);
                    fullArrangementNum.ForEach(it => buildOrderToListFunc(it.ToString("D" + betNum.Length), numQuantity, x));
                }
                else
                    buildOrderToListFunc(betNum, numQuantity, x);
            });
        }
        /// <summary>
        /// 创建下注订单.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="numQuantity">The num quantity.</param>
        /// <param name="commModel">The comm model.</param>
        /// <param name="gamePlayWay">The game play way.</param>
        /// <param name="company">The company.</param>
        /// <param name="wager">The wager.</param>
        /// <returns></returns>
        protected virtual BetOrder BuildBetOrder(string num, int numQuantity, Pair<CommissionGroup, IEnumerable<ConcreteCommission>> memberComm, GamePlayWay gamePlayWay,
            LotteryCompany company, decimal wager, User user, bool isCon = false)
        {
            var commValue = memberComm.Value.Find(it => it.CompanyType == company.CompanyType && it.GameType == gamePlayWay.GameType);
            double percentComm = isCon ? commValue.Commission - LotterySystem.Current.AutoElectionCodeCommission : commValue.Commission;
            decimal comm = (decimal)MathHelper.PercentageToDecimal(percentComm, 4);
            decimal turnOver = wager * numQuantity;
            decimal commission = comm * turnOver;
            BetOrder order = new BetOrder
            {
                Num = num,
                GamePlayWayId = gamePlayWay.Id,
                CompanyId = company.CompanyId,
                Amount = wager,
                Odds = commValue.Odds,
                Commission = commission,
                Turnover = turnOver,
                Net = 100 - percentComm,
                NetAmount = turnOver - commission,
                Status = BetStatus.Valid,
                UserId = user.UserId,
                AncestorCommission = BuildAncestorCommission(turnOver, user, gamePlayWay.GameType, company.CompanyType, memberComm.Key.Specie, isCon)
            };
            if (isCon)
            {
                order.DropWater += LotterySystem.Current.AutoElectionCodeCommission;
            }
            return order;
        }
        /// <summary>
        /// 创建注单的各父级佣金
        /// </summary>
        /// <param name="turnOver">The turn over.</param>
        /// <param name="member">The member.</param>
        /// <param name="gt">The gt.</param>
        /// <param name="ct">The ct.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="isCon">if set to <c>true</c> [is con].</param>
        /// <returns></returns>
        protected IList<OrderAncestorCommInfo> BuildAncestorCommission(decimal turnOver, User member, GameType gt, CompanyType ct, LotterySpecies specie, bool isCon)
        {
            List<OrderAncestorCommInfo> ancestorCommList = new List<OrderAncestorCommInfo>();
            foreach (int roleId in Enum.GetValues(typeof(Role)))
            {
                Role role = (Role)roleId;
                if (role >= Role.Guest) continue;
                var concreteComm = GetParentCommList(member, role, specie).Find(it => it.GameType == gt && it.CompanyType == ct);

                double comm = concreteComm.Commission;
                //如果十二生肖跌水并且是公司，则要补上会员扣去的那点佣金
                if (isCon)
                {
                    if (role == Role.Company)
                        comm += LotterySystem.Current.AutoElectionCodeCommission;
                    else
                        comm -= LotterySystem.Current.AutoElectionCodeCommission;
                }
                ancestorCommList.Add(new OrderAncestorCommInfo
                {
                    RoleId = roleId,
                    Commission = comm,
                    CommAmount = turnOver * (decimal)(comm / 100)
                });
            }
            return ancestorCommList;
        }
        /// <summary>
        /// 计算号码数量（不同玩法中号码所占数量，如2D HN公司4个，18A1个，3D又不同）.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="gamePlayWay">The game play way.</param>
        /// <param name="companyType">Type of the company.</param>
        /// <param name="isFullPermutation">if set to <c>true</c> [is full permutation].</param>
        /// <returns></returns>
        protected int CountNumQuantity(string num, GamePlayWay gamePlayWay, CompanyType companyType)
        {
            var counter = NumQuantityCounterFactory.GetFactory.GetCounter(gamePlayWay.GameType, companyType, LotterySystem.Current.GetNumLenSupport(companyType));
            return counter.CountNumQuantity(num, gamePlayWay.PlayWay);
        }
        private const string M_PARENTCOMMDIC = "ParentCommDic";
        /// <summary>
        /// 获取指定父级的佣金
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="role">The role.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        protected IEnumerable<ConcreteCommission> GetParentCommList(User member, Role role, LotterySpecies specie)
        {
            var familyCommDic = HttpContext.Current.Session[M_PARENTCOMMDIC] as IDictionary<Role, IEnumerable<ConcreteCommission>>;
            if (familyCommDic == null || familyCommDic.Count == 0)
            {
                familyCommDic = CommManager.GetParentsCommission(member, specie);
                HttpContext.Current.Session.Add(M_PARENTCOMMDIC, familyCommDic);
            }
            return familyCommDic[role];
        }
        /// <summary>
        /// 检查下注额（用于累加，最后表示本次注单的下注信息）
        /// </summary>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="isFullPermutation">if set to <c>true</c> [is full permutation].</param>
        /// <param name="wagerList">The wager list.</param>
        protected void CheckBetWager(int gameplaywayId, bool isFullPermutation, decimal amount, List<WagerItem> wagerList)
        {
            //var gpw = LotterySystem.Current.FindGamePlayWay(gameplaywayId);
            //PlayWay playway = gpw.PlayWay;
            ////將頭尾才分為頭，尾兩種
            //if (playway == PlayWay.HeadAndLast)
            //{
            //    var newGPW = LotterySystem.Current.FindGamePlayWay(gpw.GameType, PlayWay.Head);
            //    CheckBetWager(newGPW.Id, isFullPermutation, wagerList);
            //    newGPW = LotterySystem.Current.FindGamePlayWay(gpw.GameType, PlayWay.Last);
            //    CheckBetWager(newGPW.Id, isFullPermutation, wagerList);
            //}
            //else
            //{
            var wagerItem = wagerList.Find(it => it.GamePlayTypeId == gameplaywayId);
            if (wagerItem == null)
            {
                wagerItem = new WagerItem
                {
                    GamePlayTypeId = gameplaywayId,
                    IsFullPermutation = false,
                    Wager = amount
                };
                wagerList.Add(wagerItem);
            }
            if (isFullPermutation)
                wagerItem.IsFullPermutation = true;
            //}
        }
        /// <summary>
        /// 创建注单Sheet的下注金额说明
        /// </summary>
        /// <param name="orderList">The order list.</param>
        /// <param name="wagerList">The wager list.</param>
        /// <returns></returns>
        protected string GetBetWagerData(IList<BetOrder> orderList, List<WagerItem> wagerList)
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var wager in wagerList)
            {
                if (isFirst) isFirst = false;
                else sb.Append("@");
                //wager.Wager = orderList.Where(it => it.GamePlayWayId == wager.GamePlayTypeId).Sum(it => it.Amount);
                //格式說明 遊戲玩法Id/金額/是否化字
                sb.AppendFormat("{0}/{1}/{2}", wager.GamePlayTypeId, wager.Wager, wager.IsFullPermutation ? 1 : 0);
            }
            return sb.ToString();
        }
        /// <summary>
        /// 分解下注号码，若是普通号码/PL2/PL3不操作,否则分解号码（例如连号等）
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        protected IList<string> ParseBetNums(string num)
        {
            IList<string> numList = null;
            if (num.IsNum() || num.IsPL2() || num.IsPL3())
                numList = new List<string> { num };
            else if (num.IsRangeNum())
                numList = num.ParseRangeNums();
            else if (num.Is2DBatterNum())
                numList = num.Parse2DBatterNums();
            else if (num.Is3DBatterNum())
                numList = num.Parse3DBatterNums();
            return numList;
        }
    }
}
