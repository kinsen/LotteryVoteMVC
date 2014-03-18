using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Limit;
using System.Web;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core.Bet
{
    internal class BetOrderBuilder
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

        private BetResult _betResult;
        public BetResult BetResult
        {
            get
            {
                if (_betResult == null)
                    _betResult = new BetResult();
                return _betResult;
            }
        }

        /// <summary>
        /// 创建注单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="speice">The speice.</param>
        /// <param name="betList">The bet list.</param>
        /// <param name="isCon">是否快速下单.</param>
        /// <returns></returns>
        public IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie, IEnumerable<BetItem> betList, bool isCon = false)
        {
            try
            {
                this._betResult = new BetResult();
                var todayLotteryCompany = TodayLotteryCompany.Instance.GetTodayCompany();       //获取今日开奖公司
                var memberComm = CommManager.GetMemberCommissionInSession(user, specie);        //获取会员的佣金
                Dictionary<BetSheet, IList<BetOrder>> betSheetDic = new Dictionary<BetSheet, IList<BetOrder>>();    //注单字典
                List<WagerItem> betWagerList = new List<WagerItem>();        //下注金額列表
                StringBuilder betCompanySB = new StringBuilder();
                foreach (var betItem in betList)
                {
                    var numList = ParseBetNums(betItem.Num);
                    if (numList == null) continue;
                    IList<long> fullArrangementNum = null;      //全排列号码
                    BetSheet sheet = new BetSheet               //注单
                    {
                        Num = betItem.Num,
                        UserId = user.UserId,
                        Status = BetStatus.Valid
                    };
                    betCompanySB.Clear();
                    betWagerList.Clear();
                    betSheetDic.Add(sheet, new List<BetOrder>());
                    foreach (var companyId in betItem.CompanyList)      //先遍历公司列表，同个号码支持多个公司
                    {
                        var company = todayLotteryCompany.Find(it => it.CompanyId == companyId);        //找到对应的公司
                        var companyType = company.CompanyType;
                        foreach (var wager in betItem.WargerList)       //便利下注订单
                        {
                            if (wager.Wager <= 0) continue;
                            CheckBetWager(wager.GamePlayTypeId, wager.IsFullPermutation, wager.Wager, betWagerList);
                            foreach (var num in numList)
                                AddOrderToDic(betSheetDic[sheet], companyType, company, memberComm, num, wager, user, fullArrangementNum, isCon);
                        }
                        betCompanySB.AppendFormat("{0} ", company.Abbreviation);    //现在只记录公司名称即可
                    }
                    sheet.BetCompany = betCompanySB.ToString();
                    sheet.BetAmount = GetBetWagerData(betSheetDic[sheet], betWagerList);
                }
                return betSheetDic;
            }
            catch
            {
                //回滚注单
                foreach (var order in Checker.BeInsertOrderList)
                    Checker.RollLimit(order);
                throw;
            }
        }
        public IDictionary<BetSheet, IList<BetOrder>> BuildFastBetOrder(User user, LotterySpecies specie, FastBetItem fastBetItem, GameType gameType)
        {
            this._betResult = new BetResult();
            var todayLotteryCompany = TodayLotteryCompany.Instance.GetTodayCompany();       //获取今日开奖公司
            var memberComm = CommManager.GetMemberCommissionInSession(user, specie);        //获取会员的佣金
            Dictionary<BetSheet, IList<BetOrder>> betSheetDic = new Dictionary<BetSheet, IList<BetOrder>>();    //注单字典
            List<WagerItem> betWagerList = new List<WagerItem>();        //下注金額列表
            StringBuilder betCompanySB = new StringBuilder();
            BetSheet sheet = new BetSheet               //注单
            {
                Num = "FastBet" + EnumHelper.GetEnumDescript(gameType).Description,
                UserId = user.UserId,
                Status = BetStatus.Valid
            };
            betSheetDic.Add(sheet, new List<BetOrder>());
            foreach (var companyId in fastBetItem.Companys)
            {
                IList<long> fullArrangementNum = null;      //全排列号码
                var company = todayLotteryCompany.Find(it => it.CompanyId == companyId);        //找到对应的公司
                var companyType = company.CompanyType;
                foreach (var gpwId in fastBetItem.GamePlayWays)
                {
                    CheckBetWager(gpwId, fastBetItem.IsFullPermutation, fastBetItem.Wager, betWagerList);
                    var wager = new WagerItem { GamePlayTypeId = gpwId, Wager = fastBetItem.Wager, IsFullPermutation = fastBetItem.IsFullPermutation };
                    foreach (var num in fastBetItem.NumList)
                    {
                        AddOrderToDic(betSheetDic[sheet], companyType, company, memberComm, num, wager, user, fullArrangementNum, false);
                    }
                }
                betCompanySB.AppendFormat("{0} ", company.Abbreviation);    //现在只记录公司名称即可
                sheet.BetCompany = betCompanySB.ToString();
                sheet.BetAmount = GetBetWagerData(betSheetDic[sheet], betWagerList);
            }
            return betSheetDic;
        }
        /// <summary>
        /// 创建快速下单(十二生肖，双单等)注单
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="betList">The bet list.</param>
        /// <returns></returns>
        public IDictionary<BetSheet, IList<BetOrder>> BuildAutoBetOrder(User member, LotterySpecies specie, IEnumerable<AutoBetItem> betList)
        {
            this._betResult = new BetResult();
            BetResult returnResult = new Models.BetResult();
            var todayLotteryCompany = TodayLotteryCompany.Instance.GetTodayCompany();       //获取今日开奖公司
            var memberComm = CommManager.GetMemberCommissionInSession(member, specie);        //获取会员的佣金
            IDictionary<BetSheet, IList<BetOrder>> betSheetDic = new Dictionary<BetSheet, IList<BetOrder>>();
            IDictionary<BetSheet, IList<BetOrder>> resultDic = new Dictionary<BetSheet, IList<BetOrder>>();
            foreach (var betOrder in betList)
            {
                int[] nums;
                switch (betOrder.BetType)
                {
                    case AutoBetType.TwelveZodiac: nums = LotterySystem.Current.TwelveZodiac; break;
                    case AutoBetType.EvenEven: nums = LotterySystem.Current.EvenEven; break;
                    case AutoBetType.EvenOdd: nums = LotterySystem.Current.EvenOdd; break;
                    case AutoBetType.OddEven: nums = LotterySystem.Current.OddEven; break;
                    case AutoBetType.OddOdd: nums = LotterySystem.Current.OddOdd; break;
                    default: throw new InvalidDataException("不可到达,数据异常!");
                }
                var sheet = BuildAutoElectionCodeOrder(member, specie, betOrder.CompanyList, betOrder.WagerList, nums);
                betSheetDic.AddRange(sheet);
                returnResult.Append(this.BetResult);

                List<BetOrder> orderList = new List<BetOrder>();
                foreach (var item in betSheetDic)
                    orderList.AddRange(item.Value);
                StringBuilder companySb = new StringBuilder();
                foreach (var companyId in betOrder.CompanyList)
                {
                    var company = todayLotteryCompany.Find(it => it.CompanyId == companyId);
                    if (company == null)
                        throw new InvalidDataException("CompanyId:" + companyId);
                    companySb.AppendFormat("{0} ", company.Abbreviation);
                }
                BetSheet orderSheet = new BetSheet
                {
                    Num = betOrder.BetType.ToString(),
                    Turnover = orderList.Sum(it => it.Turnover),
                    NetAmount = orderList.Sum(it => it.NetAmount),
                    Commission = orderList.Sum(it => it.Commission),
                    UserId = member.UserId,
                    Status = BetStatus.Valid,
                    IPAddress = IPHelper.IPAddress,
                    BetCompany = companySb.ToString(),
                    BetAmount = JoinSheetBetAmount(sheet.Keys)
                };
                resultDic.Add(orderSheet, orderList);
                betSheetDic.Clear();
            }
            this._betResult = returnResult;
            return resultDic;
        }
        /// <summary>
        /// 创建自动选码注单.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <param name="companys">The companys.</param>
        /// <param name="wagers">The wagers.</param>
        /// <param name="nums">The nums.</param>
        /// <returns></returns>
        private IDictionary<BetSheet, IList<BetOrder>> BuildAutoElectionCodeOrder(User user, LotterySpecies specie, int[] companys, IList<KeyValuePair<PlayWay, decimal>> wagers, int[] nums)
        {
            PlayWay[] allowBetPlayWay = new[] { PlayWay.Head, PlayWay.Last, PlayWay.HeadAndLast, PlayWay.Roll };
            List<BetItem> betList = new List<BetItem>();
            var wagerItems = new List<WagerItem>();
            foreach (var wager in wagers)
            {
                if (!allowBetPlayWay.Contains(wager.Key)) continue;
                var gpw = LotterySystem.Current.FindGamePlayWay(GameType.TwoDigital, wager.Key);
                wagerItems.Add(new WagerItem
                {
                    GamePlayTypeId = gpw.Id,
                    IsFullPermutation = false,
                    Wager = wager.Value
                });
            }
            nums.ForEach(num =>
            {
                betList.Add(new BetItem
                {
                    Num = num.ToString("D2"),
                    CompanyList = companys,
                    WargerList = wagerItems
                });
            });

            return BuildOrder(user, specie, betList, LotterySystem.Current.AutoElectionCodeCommission > 0);
        }
        /// <summary>
        /// 合并各个大注单的下注金额说明
        /// </summary>
        /// <param name="sheets">The sheets.</param>
        /// <returns></returns>
        private string JoinSheetBetAmount(IEnumerable<BetSheet> sheets)
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
        private void AddOrderToDic(IList<BetOrder> orderList, CompanyType companyType, LotteryCompany company, Pair<CommissionGroup, IEnumerable<ConcreteCommission>> memberComm,
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
        private BetOrder BuildBetOrder(string num, int numQuantity, Pair<CommissionGroup, IEnumerable<ConcreteCommission>> memberComm, GamePlayWay gamePlayWay,
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
        private IList<OrderAncestorCommInfo> BuildAncestorCommission(decimal turnOver, User member, GameType gt, CompanyType ct, LotterySpecies specie, bool isCon)
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
        private int CountNumQuantity(string num, GamePlayWay gamePlayWay, CompanyType companyType)
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
        private IEnumerable<ConcreteCommission> GetParentCommList(User member, Role role, LotterySpecies specie)
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
        private void CheckBetWager(int gameplaywayId, bool isFullPermutation, decimal amount, List<WagerItem> wagerList)
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
        private string GetBetWagerData(IList<BetOrder> orderList, List<WagerItem> wagerList)
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
        private IList<string> ParseBetNums(string num)
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
