using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using System.Web;

namespace LotteryVoteMVC.Core
{
    public class LimitManager : ManagerBase
    {
        public BetUpperLimitDataAccess _daUpperLimit;
        private DefaultUpperLimitDataAccess _daDefaultLimit;
        private DropWaterManager _dwManager;
        public BetUpperLimitDataAccess DaUpperLimit
        {
            get
            {
                if (_daUpperLimit == null)
                    _daUpperLimit = new BetUpperLimitDataAccess();
                return _daUpperLimit;
            }
        }
        public DefaultUpperLimitDataAccess DaDefaultLimit
        {
            get
            {
                if (_daDefaultLimit == null)
                    _daDefaultLimit = new DefaultUpperLimitDataAccess();
                return _daDefaultLimit;
            }
        }
        public DropWaterManager DwManager
        {
            get
            {
                if (_dwManager == null)
                    _dwManager = new DropWaterManager();
                return _dwManager;
            }
        }

        #region BetUpperLimit
        public void AddLimit(BetUpperLimit limit)
        {
            DaUpperLimit.Insert(limit);
        }
        public void UpdateLimit(BetUpperLimit limit)
        {
            DaUpperLimit.Update(limit);
        }
        /// <summary>
        /// 获取今日指定公司的上限
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public IDictionary<string, BetUpperLimit> GetTodayUpperLimit(int companyId, int gameplayway)
        {
            var dt = DaUpperLimit.GetUpperLimit(companyId, gameplayway, DateTime.Today);
            return dt.AsEnumerable().Select(it => new BetUpperLimit
            {
                LimitId = it.Field<int>(BetUpperLimit.LIMITID),
                Num = it.Field<string>(BetUpperLimit.NUM),
                CompanyId = it.Field<int>(BetUpperLimit.COMPANYID),
                GamePlayWayId = it.Field<int>(BetUpperLimit.GAMEPLAYWAYID),
                DropValue = it.Field<double>(BetUpperLimit.DROPVALUE),
                NextLimit = it.Field<decimal>(BetUpperLimit.NEXTLIMIT),
                UpperLlimit = it.Field<decimal>(BetUpperLimit.UPPERLLIMIT),
                TotalBetAmount = it.Field<decimal>(BetUpperLimit.TOTALBETAMOUNT),
                StopBet = it.Field<bool>(BetUpperLimit.STOPBET),
                CreateTime = it.Field<DateTime>(BetUpperLimit.CREATETIME)
            }).ToDictionary(it => it.Num, it => it);
        }
        public PagedList<BetUpperLimit> SearchUpperLimit(DateTime date, string num, int companyId, int gameplayway, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<BetUpperLimit>(DaUpperLimit.ListUpperLimitByCondition(date, num, companyId, gameplayway, start, end), pageIndex, pageSize,
                DaUpperLimit.CountUpperLimitByCondition(date, num, companyId, gameplayway));
        }
        #endregion

        #region DefaultUpperLimit
        public void AddDefaultUpperLimit(CompanyType companyType, int gameplaywayId, decimal limitAmount)
        {
            var limit = DaDefaultLimit.GetDefaultUpperLimit(companyType, gameplaywayId);
            if (limit != null)
                throw new BusinessException(Resource.AlreadyExist);
            limit = new DefaultUpperLimit
            {
                CompanyType = companyType,
                GamePlayWayId = gameplaywayId,
                LimitAmount = limitAmount
            };
            DaDefaultLimit.Insert(limit);
        }
        public void RemoveDefaultUpperLimit(int limitId)
        {
            DaDefaultLimit.Delete(limitId);
        }
        public BetUpperLimit GetDefaultUpperLimit(string num, int companyId, int gameplaywayId)
        {
            CheckNum(num);
            var company = TodayLotteryCompany.Instance.GetTodayCompany().Find(it => it.CompanyId == companyId);
            if (company == null)
                throw new ApplicationException("今日不存在公司!CompanyId:" + companyId);

            var defaultUpplerLimit = GetDefaultUpperLimit(company, gameplaywayId);
            var limitAmount = defaultUpplerLimit != null ? defaultUpplerLimit.LimitAmount : LotterySystem.Current.DefaultUpperLimit;

            var minDropWaters =GetMininumDropWater(num, gameplaywayId, company);//最小金额跌水
            int minDropCount = 0;
            DropWater dropWater = null;
            if (minDropWaters != null)
            {
                foreach (var drop in minDropWaters)
                {
                    minDropCount++;
                    if (dropWater != null && dropWater.DropType == DropType.Manual) continue;
                    dropWater = drop;
                }
                if (minDropCount > 2)
                    throw new ApplicationException(string.Format("号码:{0}的跌水记录大于2!CompanyId:{1} , GamePlayWayId:{2}", num, companyId, gameplaywayId));
            }

            var limit = new BetUpperLimit
            {
                Num = num,
                CompanyId = companyId,
                GamePlayWayId = gameplaywayId,
                DropValue = 0,
                NextLimit = dropWater == null ? limitAmount : dropWater.Amount,
                UpperLlimit = limitAmount,
                TotalBetAmount = 0
            };
            return limit;
        }
        public IEnumerable<DefaultUpperLimit> GetDefaultUpperLimits()
        {
            return DaDefaultLimit.GetAll();
        }
        private void CheckNum(string num)
        {
            var isNum = num.IsNum() || num.IsPL2() || num.IsPL3();
            if (!isNum)
                throw new ArgumentException("非有效数字,Num:" + num);
        }
        private const string m_DEFAULTLIMITDIC = "DefaultUpperLimitDIC";
        private const string m_MININUMDPDIC = "MininumDropWaterDic";
        /// <summary>
        /// 根据公司类型和玩法获取默认上限限制
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <returns></returns>
        private DefaultUpperLimit GetDefaultUpperLimit(LotteryCompany company, int gameplaywayId)
        {
            IDictionary<string, DefaultUpperLimit> upperlimitDic = HttpContext.Current.Items[m_DEFAULTLIMITDIC] as IDictionary<string, DefaultUpperLimit>;
            if (upperlimitDic == null)
            {
                upperlimitDic = new Dictionary<string, DefaultUpperLimit>();
                HttpContext.Current.Items[m_DEFAULTLIMITDIC] = upperlimitDic;
            }
            string key = string.Format("{0}_{1}", (int)company.CompanyType, gameplaywayId);
            DefaultUpperLimit limit;
            if (!upperlimitDic.TryGetValue(key, out limit))
            {
                limit = DaDefaultLimit.GetDefaultUpperLimit(company.CompanyType, gameplaywayId);
                upperlimitDic.Add(key, limit);
            }
            return limit;
        }
        private IEnumerable<DropWater> GetMininumDropWater(string num, int gameplaywayId, LotteryCompany company)
        {
            var dropwaterDic = HttpContext.Current.Items[m_MININUMDPDIC] as IDictionary<string, IEnumerable<DropWater>>;
            if (dropwaterDic == null)
            {
                dropwaterDic = new Dictionary<string, IEnumerable<DropWater>>();
                HttpContext.Current.Items[m_MININUMDPDIC] = dropwaterDic;
            }
            string key = string.Format("{0}_{1}_{2}", company.CompanyId, gameplaywayId, num);
            IEnumerable<DropWater> minDropWaters;
            if (!dropwaterDic.TryGetValue(key, out minDropWaters))
            {
                var dropWaters = DwManager.GetTodayNumsDropWater(num, gameplaywayId, company);
                minDropWaters = dropWaters.GroupBy(it => it.Amount).OrderBy(it => it.Key).FirstOrDefault();//最小金额跌水
                dropwaterDic.Add(key, minDropWaters);
            }
            return minDropWaters;
        }
        #endregion
    }
}
