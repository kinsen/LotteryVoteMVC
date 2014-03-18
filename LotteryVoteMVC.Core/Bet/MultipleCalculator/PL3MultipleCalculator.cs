using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class PL3MultipleCalculator : MultipleCalculator
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
            if (!order.Num.IsPL3())
                throw new ApplicationException(string.Format("num:{0} not a pl3 number", order.Num));
            return GetMultiple(order.Num, order.CompanyId);
        }

        private int GetMultiple(string num, int companyId)
        {
            var subNums = num.Split('#');

            var group = subNums.GroupBy(it => it).ToDictionary(it => it.Key, it => it.Count());

            List<int> multiples = new List<int>();
            foreach (var item in group)
            {
                var count = GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(item.Key)).Count() / item.Value;
                multiples.Add(count);
            }

            //int[] multiples = new int[subNums.Length];
            //for (int i = 0; i < subNums.Length; i++)
            //    multiples[i] = GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(subNums[i])).Count();
            return multiples.Min();
        }
        private bool IsAllContain(string num, int companyId)
        {
            int count = 0;
            var subNums = num.Split('#');
            foreach (var subNum in subNums)
            {
                int numCount = base.GetCompanyLotteryResult(companyId).Where(it => it.EndsWith(subNum)).Count();
                if (numCount > 0)
                    count++;
            }
            return count >= 3;
        }

        protected override void ClearLotteryResult()
        {
            HttpContext.Current.Cache.Remove(LOTTERRESULT);
        }
    }
}
