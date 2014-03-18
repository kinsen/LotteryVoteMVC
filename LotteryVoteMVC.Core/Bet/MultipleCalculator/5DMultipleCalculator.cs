using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Core.Bet
{
    internal class FiveDigitMultipleCalculator : DigitMultipleCalculator
    {
        private const string LOTTERRESULT = "5DLotteryResult";
        protected override IDictionary<int, IEnumerable<string>> LotteryResult
        {
            get
            {
                var lotteryResult = HttpContext.Current.Cache[LOTTERRESULT] as IDictionary<int, IEnumerable<string>>;
                if (lotteryResult == null)
                {
                    lotteryResult = new Dictionary<int, IEnumerable<string>>();
                    foreach (var result in TodayLotteryResult)
                    {
                        lotteryResult.Add(result.Company.CompanyId, result.Records.Where(it => it.Value.Length >= 4).Select(it => it.Value));
                    }
                    //将今日4D开奖结果放到缓存中，并在5分钟没操作后清空缓存
                    HttpContext.Current.Cache.Add(LOTTERRESULT, lotteryResult, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return lotteryResult;
            }
        }

        public override int Calculate(BetOrder order)
        {
            base.CheckNumLength(order.Num, 5);
            //var companyType = DaCompanyType.GetCompanyTypeByCompany(order.CompanyId);
            var gamePlayWay = LotterySystem.Current.FindGamePlayWay(order.GamePlayWayId); //DaGamePlayWay.GetGamePlayWay(order.GamePlayWayId);
            return TransToMethod(gamePlayWay)(order.Num, order.CompanyId);
        }

        protected override int CalcHead(string num, int companyId)
        {
            throw new NotImplementedException();
        }

        protected override int CalcLast(string num, int companyId)
        {
            throw new NotImplementedException();
        }

        protected override int CalcHeadAndLast(string num, int companyId)
        {
            throw new NotImplementedException();
        }

        protected override int CalcRoll(string num, int companyId)
        {
            return GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(num)).Count();
        }

        protected override int CalcRoll7(string num, int companyId)
        {
            throw new NotImplementedException();
        }

        protected override void ClearLotteryResult()
        {
            HttpContext.Current.Cache.Remove(LOTTERRESULT);
        }
    }
}
