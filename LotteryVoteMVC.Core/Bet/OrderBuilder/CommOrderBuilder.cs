using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    /// <summary>
    /// 普通订单生成器
    /// </summary>
    internal class CommOrderBuilder : OrderBuilderBase<BetItem>
    {
        public override IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie,
            IEnumerable<BetItem> betList, IDictionary<string, object> parameters)
        {
            bool isCon = false;
            object parameterValue;
            if (parameters.TryGetValue("ISCON", out parameterValue))
                isCon = Convert.ToBoolean(parameterValue);

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
    }
}
