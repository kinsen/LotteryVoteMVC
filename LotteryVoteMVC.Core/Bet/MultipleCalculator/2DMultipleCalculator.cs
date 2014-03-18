using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class TwoDigitMultipleCalculator : DigitMultipleCalculator
    {
        private const string LOTTERRESULT = "2DLotteryResult";

        /// <summary>
        /// 2D开奖结果  CompanyTypeId，Values
        /// </summary>
        protected override IDictionary<int, IEnumerable<string>> LotteryResult
        {
            get
            {
                var lotteryResult = HttpRuntime.Cache[LOTTERRESULT] as IDictionary<int, IEnumerable<string>>;
                if (lotteryResult == null)
                {
                    lotteryResult = new Dictionary<int, IEnumerable<string>>();
                    foreach (var result in TodayLotteryResult)
                    {
                        lotteryResult.Add(result.Company.CompanyId, result.Records.Where(it => it.Value.Length >= 2).Select(it => it.Value));
                    }
                    //将今日2D开奖结果放到缓存中，并在5分钟没操作后清空缓存
                    HttpRuntime.Cache.Add(LOTTERRESULT, lotteryResult, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return lotteryResult;
            }
        }

        public override int Calculate(BetOrder order)
        {
            base.CheckNumLength(order.Num, 2);
            //var companyType = DaCompanyType.GetCompanyTypeByCompany(order.CompanyId);
            var gamePlayWay = LotterySystem.Current.FindGamePlayWay(order.GamePlayWayId);// DaGamePlayWay.GetGamePlayWay(order.GamePlayWayId);
            return TransToMethod(gamePlayWay)(order.Num, order.CompanyId);
        }

        protected override int CalcHead(string num, int companyId)
        {
            var numLen = GetComTypeSupportNumLen(GetCompany(companyId).CompanyType).Find(it => it.NumLen.Length == 2); //DaNumLength.GetSupportNumLengthByLen(GetCompany(companyId).CompanyType.Id, 2);
            var count = GetCompanyLotteryResult(companyId).Take(numLen.Count).Where(it => it.EndsWith(num)).Count();
            return count;
        }
        protected override int CalcLast(string num, int companyId)
        {
            return GetCompanyLotteryResult(companyId).Last().EndsWith(num) ? 1 : 0;
        }
        protected override int CalcHeadAndLast(string num, int companyId)
        {
            return CalcHead(num, companyId) + CalcLast(num, companyId);
        }
        protected override int CalcRoll(string num, int companyId)
        {
            return GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(num)).Count();
        }
        protected override int CalcRoll7(string num, int companyId)
        {
            var result = GetCompanyLotteryResult(companyId);
            int count = result.Take(6).Where(it => it.EndsWith(num)).Count();
            
            if (result.Last().EndsWith(num))
                count++;

            return count;
        }

        protected override void ClearLotteryResult()
        {
            HttpRuntime.Cache.Remove(LOTTERRESULT);
        }
    }
}
