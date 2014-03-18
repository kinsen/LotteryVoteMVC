using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using System.Web.Caching;

namespace LotteryVoteMVC.Core.Bet
{
    internal class PL2MultipleCalculator : MultipleCalculator
    {
        private const string LOTTERRESULT = "2DLotteryResult";

        /// <summary>
        /// 2D开奖结果  CompanyTypeId，Values
        /// </summary>
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
                        lotteryResult.Add(result.Company.CompanyId, result.Records.Where(it => it.Value.Length >= 2).Select(it => it.Value));
                    }
                    //将今日2D开奖结果放到缓存中，并在5分钟没操作后清空缓存
                    HttpContext.Current.Cache.Add(LOTTERRESULT, lotteryResult, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return lotteryResult;
            }
        }

        public override int Calculate(BetOrder order)
        {
            if (!order.Num.IsPL2())
                throw new ApplicationException(string.Format("num:{0} not a pl2 number", order.Num));
            return GetMultiple(order.Num, order.CompanyId);
        }

        private int GetMultiple(string num, int companyId)
        {
            var subNums = num.Split('#');
            //如果两个是相同的数字，则取该数数量的一半作次数
            if (subNums[0] == subNums[1])
                return GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(subNums[0])).Count() / 2;

            int[] multiples = new int[subNums.Length];
            for (int i = 0; i < subNums.Length; i++)
                multiples[i] = GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(subNums[i])).Count();
            return multiples.Min();
        }
        private bool IsAllContain(string num, int companyId)
        {
            int count = 0;
            var subNums = num.Split('#');
            foreach (var subNum in subNums)
            {
                int numCount = GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(subNum)).Count();
                if (numCount > 0)
                    count++;
            }
            return count >= 2;
        }

        protected override void ClearLotteryResult()
        {
            HttpContext.Current.Cache.Remove(LOTTERRESULT);
        }
    }
}
