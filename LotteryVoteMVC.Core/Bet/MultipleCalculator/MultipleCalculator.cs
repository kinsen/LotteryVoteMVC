using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Web;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Application;
using System.Web.Caching;

namespace LotteryVoteMVC.Core.Bet
{
    internal abstract class MultipleCalculator
    {
        private const string TODAYRESULT = "TodayLotteryResult";
        private const string COMTYPESUPPORTDIC = "ComTypeSupportDic";
        private IList<LotteryCompany> _todayCompany;
        protected IList<LotteryCompany> TodayCompany
        {
            get
            {
                if (_todayCompany == null)
                    _todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
                return _todayCompany;
            }
        }
        public CompanyManager ComManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<CompanyManager>();
            }
        }

        /// <summary>
        /// 今日开奖结果.
        /// </summary>
        public IEnumerable<LotteryResult> TodayLotteryResult
        {
            get
            {
                var result = HttpRuntime.Cache[TODAYRESULT] as IEnumerable<LotteryResult>;
                if (result == null)
                {
                    result = ComManager.GetLotteryResultByDate(DateTime.Today, false).ToList();
                    //将今日开奖结果放到缓存中，并在5分钟没操作后清空缓存
                    HttpRuntime.Cache.Add(TODAYRESULT, result, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return result;
            }
        }
        /// <summary>
        /// 公司类型号码支持长度 
        /// </summary>
        public IDictionary<CompanyType, IList<CompanyTypeSupportNumLen>> ComTypeSupportDic
        {
            get
            {
                var dic = HttpRuntime.Cache[COMTYPESUPPORTDIC] as IDictionary<CompanyType, IList<CompanyTypeSupportNumLen>>;
                if (dic == null)
                {
                    dic = new Dictionary<CompanyType, IList<CompanyTypeSupportNumLen>>();
                    //将今日开奖结果放到缓存中，并在5分钟没操作后清空缓存
                    HttpRuntime.Cache.Add(COMTYPESUPPORTDIC, dic, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(5), CacheItemPriority.Default, null);
                }
                return dic;
            }
        }
        protected abstract IDictionary<int, IEnumerable<string>> LotteryResult { get; }
        public abstract int Calculate(BetOrder order);
        protected abstract void ClearLotteryResult();
        protected IEnumerable<string> GetCompanyLotteryResult(int companyId)
        {
            IEnumerable<string> result = LotteryResult.ContainsKey(companyId) ? LotteryResult[companyId] : null;
            if (result == null && TodayLotteryResult != null)
            {
                HttpRuntime.Cache.Remove(MultipleCalculator.TODAYRESULT);
                ClearLotteryResult();
                if (!LotteryResult.ContainsKey(companyId))
                    throw new BusinessException(Resource.NoLotteryNum);
                result = LotteryResult[companyId];
            }
            return result;
        }
        protected LotteryCompany GetCompany(int companyId)
        {
            var company = LotterySystem.Current.FindCompany(companyId);
            if (company == null)
                throw new InvalidDataException("找不到公司!CompanyId:" + companyId);
            return company;
        }
        protected IList<CompanyTypeSupportNumLen> GetComTypeSupportNumLen(CompanyType comType)
        {
            if (!ComTypeSupportDic.ContainsKey(comType))
                ComTypeSupportDic.Add(comType, ComManager.GetNumLenthByCompanyType(comType).ToList());
            return ComTypeSupportDic[comType];
        }
    }
}
