using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class A_BPL2OrderBuilder : OrderBuilderBase<BetItem>
    {
        protected string Ext1 { get; set; }

        public override IDictionary<BetSheet, IList<BetOrder>> BuildOrder(User user, LotterySpecies specie,
            IEnumerable<BetItem> betList, IDictionary<string, object> parameters)
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

                    if (betItem.CompanyList.Length % 2 != 0) continue;      //A&B PL2的公司必须是两个,所以必须被2整除

                    for (int i = 0; i < betItem.CompanyList.Length; i += 2)
                    {
                        var company = todayLotteryCompany.Find(it => it.CompanyId == betItem.CompanyList[i]);        //找到对应的公司
                        if (company.CompanyType != CompanyType.EighteenA) continue; //A&B PL2的第一个公司必须是18A类型的公司

                        var companyB = todayLotteryCompany.Find(it => it.CompanyId == betItem.CompanyList[i + 1]);
                        if (companyB == null || companyB.CompanyType != CompanyType.EighteenB) continue;            //A&B PL2的第二个公司必须是18B

                        Ext1 = betItem.CompanyList[i + 1].ToString();       //设置扩展字段为B公司

                        var companyType = company.CompanyType;
                        foreach (var wager in betItem.WargerList)       //便利下注订单
                        {
                            if (wager.Wager <= 0) continue;
                            CheckBetWager(wager.GamePlayTypeId, wager.IsFullPermutation, wager.Wager, betWagerList);
                            foreach (var num in numList)
                                AddOrderToDic(betSheetDic[sheet], companyType, company, memberComm, num, wager, user, fullArrangementNum, false);
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

        protected override BetOrder BuildBetOrder(string num, int numQuantity,
            Pair<CommissionGroup, IEnumerable<ConcreteCommission>> memberComm,
            GamePlayWay gamePlayWay, LotteryCompany company, decimal wager, User user, bool isCon = false)
        {
            var order = base.BuildBetOrder(num, numQuantity, memberComm, gamePlayWay, company, wager, user, isCon);
            order.Ext1 = this.Ext1;
            return order;
        }
    }
}
