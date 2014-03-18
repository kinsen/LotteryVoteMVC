using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    /// <summary>
    /// 快速下注注单生成器
    /// </summary>
    internal class FastBetOrderBuilder : OrderBuilderBase<FastBetItem>
    {
        public override IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie,
            IEnumerable<FastBetItem> betList, IDictionary<string, object> parameters)
        {
            var fastBetItem = betList.FirstOrDefault();
            if (fastBetItem == null) return null;

            GameType gameType;
            if (!parameters.ContainsKey("GameType"))
                throw new ArgumentException("parameters 中找不到GameType项，GameType是快速下注必要参数!");
            gameType = EnumHelper.GetEnum<GameType>(parameters["GameType"].ToString());

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
    }
}
