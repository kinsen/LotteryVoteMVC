using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core.Bet
{
    internal class AutoBetOrderBuilder : CommOrderBuilder
    {

        public IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie,
            IEnumerable<AutoBetItem> betList, IDictionary<string, object> parameters)
        {
            this._betResult = new BetResult();
            BetResult returnResult = new Models.BetResult();
            var todayLotteryCompany = TodayLotteryCompany.Instance.GetTodayCompany();       //获取今日开奖公司
            var memberComm = CommManager.GetMemberCommissionInSession(user, specie);        //获取会员的佣金
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
                    case AutoBetType.Small: nums = LotterySystem.Current.Small; break;
                    case AutoBetType.Big: nums = LotterySystem.Current.Big; break;
                    default: throw new InvalidDataException("不可到达,数据异常!");
                }
                var sheet = BuildAutoElectionCodeOrder(user, specie, betOrder.CompanyList, betOrder.WagerList, nums);
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
                    UserId = user.UserId,
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
        private IDictionary<BetSheet, IList<BetOrder>> BuildAutoElectionCodeOrder(User user, LotterySpecies specie,
            int[] companys, IList<KeyValuePair<PlayWay, decimal>> wagers, int[] nums)
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

            return base.BuildOrder(user, specie, betList,
                new Dictionary<string, object> { 
                    { "ISCON", LotterySystem.Current.AutoElectionCodeCommission > 0 } 
                });
        }
    }
}
