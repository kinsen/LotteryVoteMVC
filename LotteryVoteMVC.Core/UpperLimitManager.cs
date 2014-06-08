using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using LotteryVoteMVC.Models;
using System.Web;
using System.Web.Caching;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;
using LotteryVoteMVC.Data;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 上限限制管理器
    /// </summary>
    public class UpperLimitManager
    {
        private const string M_CACHEKEY = "CACHE_UPPERLIMIT_{0}_{1}";
        private static object _lockHelper = new object();
        private static UpperLimitManager m_manager;
        private UpperLimitManager()
        {
            InitContextDic();
            limitSynTimer.AutoReset = true;
            limitSynTimer.Enabled = true;
            limitSynTimer.Elapsed += new ElapsedEventHandler(limitSynTimer_Elapsed);
            limitSynTimer.Start();
        }

        private LimitManager _limitManager;
        private DropWaterManager _dwManager;
        private BetOrderDataAccess _daOrder;
        public LimitManager LimitManager
        {
            get
            {
                if (_limitManager == null)
                    _limitManager = new LimitManager();
                return _limitManager;
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
        public BetOrderDataAccess DaOrder
        {
            get
            {
                if (_daOrder == null)
                    _daOrder = new BetOrderDataAccess();
                return _daOrder;
            }
        }

        void limitSynTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckUpdate();
        }
        public static UpperLimitManager GetManager()
        {
            if (m_manager == null)
            {
                lock (_lockHelper)
                {
                    if (m_manager == null)
                        m_manager = new UpperLimitManager();
                }
            }
            return m_manager;
        }
        Timer limitSynTimer = new Timer(300000);        //五分钟检查更新

        /// <summary>
        /// 表示当前上限列表
        /// Key:CacheKey
        /// Value:内容是否被更改
        /// </summary>
        private IDictionary<string, bool> ContextDic = new Dictionary<string, bool>();
        private void InitContextDic()
        {
            var companys = TodayLotteryCompany.Instance.GetTodayCompany();
            foreach (var company in companys)
            {
                foreach (var gpw in LotterySystem.Current.GamePlayWays)
                    ContextDic.Add(string.Format(M_CACHEKEY, company.CompanyId, gpw.Id), false);
            }
        }
        private string GetCacheKey(int companyId, int gameplaywayId)
        {
            string key = string.Format(M_CACHEKEY, companyId, gameplaywayId);
            if (!ContextDic.ContainsKey(key))
                ContextDic.Add(key, false);
            return key;
        }

        private IDictionary<string, BetUpperLimit> GetLimitContext(string key)
        {
            var keyInfo = key.Split('_');
            int companyId = int.Parse(keyInfo[2]);
            int gameplayway = int.Parse(keyInfo[3]);
            return GetLimitContext(companyId, gameplayway);
        }
        /// <summary>
        ///仅从Cache中获取，若Cache不存在，则为空
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplayway">The gameplayway.</param>
        /// <returns></returns>
        private IDictionary<string, BetUpperLimit> GetLimitContextInCache(int companyId, int gameplayway)
        {
            string key = GetCacheKey(companyId, gameplayway);
            var context = HttpRuntime.Cache[key] as IDictionary<string, BetUpperLimit>;
            return context;
        }
        public IDictionary<string, BetUpperLimit> GetLimitContext(int companyId, int gameplayway)
        {
            string key = GetCacheKey(companyId, gameplayway);
            var context = HttpRuntime.Cache[key] as IDictionary<string, BetUpperLimit>;
            if (context == null)
            {
                context = LimitManager.GetTodayUpperLimit(companyId, gameplayway);
                //将限制放入缓存中，并在无操作10分钟之后移除
                HttpRuntime.Cache.Add(key, context, null, Cache.NoAbsoluteExpiration, TimeSpan.FromMinutes(10), CacheItemPriority.Normal,
                    (k, v, reason) =>
                    {
                        //移除缓存时更新到DB中
                        var limitContext = v as IDictionary<string, BetUpperLimit>;
                        if (limitContext == null) return;
                        Update(limitContext);
                    });

                if (!ContextDic.ContainsKey(key))
                    ContextDic.Add(key, false);
            }
            return context;
        }
        public BetUpperLimit GetLimit(int limitId)
        {
            BetUpperLimit upperLimit = null;
            foreach (var item in ContextDic)
            {
                var limitContext = GetLimitContext(item.Key);
                if (limitContext == null) continue;
                foreach (var limit in limitContext)
                {
                    if (limit.Value.LimitId == limitId)
                    {
                        upperLimit = limit.Value;
                        break;
                    }
                    if (upperLimit != null) break;
                }
            }
            return upperLimit;
        }
        public BetUpperLimit GetLimit(BetOrder order)
        {
            return GetLimit(order.Num, order.CompanyId, order.GamePlayWayId);
        }
        public BetUpperLimit GetLimit(string num, int companyId, int gameplaywayId)
        {
            var limitContext = GetLimitContext(companyId, gameplaywayId);
            BetUpperLimit limit;
            if (!limitContext.TryGetValue(num, out limit))
            {
                lock (limitContext)//数据库新增限制时，必须先对限制所属哈希表加锁，确保不会加入相同键值的重复数据
                {
                    if (!limitContext.TryGetValue(num, out limit))
                    {
                        limit = LimitManager.GetDefaultUpperLimit(num, companyId, gameplaywayId);
                        LimitManager.AddLimit(limit);
                        limitContext.Add(limit.Num, limit);
                    }
                }
            }
            return (BetUpperLimit)limit.Clone();
        }
        public IList<IDictionary<string, BetUpperLimit>> GetAllLimitContext()
        {
            List<IDictionary<string, BetUpperLimit>> contextList = new List<IDictionary<string, BetUpperLimit>>();
            List<string> removeItem = null;
            foreach (var item in ContextDic)
            {
                var context = GetLimitContext(item.Key);
                if (context == null || context.Count == 0)
                {
                    if (removeItem == null)
                        removeItem = new List<string>();
                    removeItem.Add(item.Key);
                }
                else
                    contextList.Add(context);
            }
            if (removeItem != null)
                foreach (var item in removeItem)
                    ContextDic.Remove(item);
            return contextList;
        }
        public IEnumerable<BetUpperLimit> GetAllLimit()
        {
            foreach (var context in GetAllLimitContext())
            {
                foreach (var limit in context.Values)
                    yield return limit;
            }
        }
        public void UpdateLimit(BetUpperLimit limit, decimal amount)
        {
            if (amount < limit.UpperLlimit)
            {
                if (amount < limit.TotalBetAmount)
                    throw new BusinessException(string.Format(ModelResource.MustGreatThan, Resource.Amount, limit.TotalBetAmount));
            }
            limit.UpperLlimit = amount;
            UpdateLimit(limit);
        }
        public void UpdateAcceptBet(int limitId, bool acceptBet)
        {
            var limit = GetLimit(limitId);
            if (limit == null) throw new InvalidDataException("UpperLimit", "找不到UpperLimit,LimitID:" + limitId);
            if (limit.StopBet != acceptBet)
            {
                limit.StopBet = acceptBet;
                UpdateLimit(limit);
            }
        }
        public void UpdateLimit(BetUpperLimit limit)
        {
            var limitContext = GetLimitContext(limit.CompanyId, limit.GamePlayWayId);
            BetUpperLimit upperlimit;
            if (!limitContext.TryGetValue(limit.Num, out upperlimit))
            {
                limit.IsChange = true;
                limitContext.Add(limit.Num, limit);
            }
            else
            {
                lock (upperlimit)        //对象写锁
                {
                    upperlimit.IsChange = true;     //表示对象已经被修改
                    upperlimit.DropValue = limit.DropValue;
                    upperlimit.NextLimit = limit.NextLimit;
                    upperlimit.UpperLlimit = limit.UpperLlimit;
                    upperlimit.TotalBetAmount = limit.TotalBetAmount;
                    upperlimit.StopBet = limit.StopBet;
                }
            }
            //更新上限上下文
            var key = GetCacheKey(limit.CompanyId, limit.GamePlayWayId);
            if (!ContextDic[key])
                ContextDic[key] = true;

            var _limit = limitContext[limit.Num];
            if (_limit != null)
                LogConsole.Debug(string.Format("LimitContext Num:{0} CompanyId:{1} GPW:{2} TotalAmount:{3} IsChange:{4}", _limit.Num, _limit.CompanyId, _limit.GamePlayWayId, _limit.TotalBetAmount, _limit.IsChange));
            else
                LogConsole.Debug(string.Format("LimitContext IsNull Num:{0} CompanyId:{1} GPW:{2} TotalAmount:{3}", limit.Num, limit.CompanyId, limit.GamePlayWayId, limit.TotalBetAmount));
        }
        public void UpdateLimit()
        {
            CheckUpdate();
        }
        public void RefreshLimitNextLimit(string num, int gameplaywayId)
        {
            var companys = TodayLotteryCompany.Instance.GetTodayCompany();
            foreach (var company in companys)
            {
                var limitContext = GetLimitContext(company.CompanyId, gameplaywayId);
                if (limitContext == null) continue;
                BetUpperLimit limit;
                if (!limitContext.TryGetValue(num, out limit)) continue;
                RefreshLimitNextLimit(limit);
            }
        }
        public void RefreshLimitNextLimit(BetUpperLimit limit)
        {
            LotteryCompany company = TodayLotteryCompany.Instance.GetTodayCompany().Find(it => it.CompanyId == limit.CompanyId);
            var dropWater = DwManager.GetTodayNumsDropWater(limit.Num, limit.GamePlayWayId, company);
            var drop = dropWater.Where(it => it.Amount >= limit.TotalBetAmount).OrderBy(it => it.Amount).FirstOrDefault();
            if (drop != null)
            {
                limit.NextLimit = drop.Amount;
                UpdateLimit(limit);
            }
        }
        /// <summary>
        /// 获取各个公司的限制信息
        /// </summary>
        /// <returns></returns>
        public IEnumerable<CompanyAmountInfo> GetCompanyLimitData()
        {
            var todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
            var companyGP = GetAllLimit().GroupBy(it => it.CompanyId).OrderBy(it => it.Key);
            List<CompanyAmountInfo> amountList = new List<CompanyAmountInfo>();
            foreach (var gp in companyGP)
            {
                var company = todayCompany.Find(it => it.CompanyId == gp.Key);

                var gpwGP = gp.GroupBy(it => it.GamePlayWayId);
                Dictionary<GamePlayWay, decimal> gpwDic = new Dictionary<GamePlayWay, decimal>();
                foreach (var gpw in gpwGP.OrderBy(it => it.Key))
                {
                    var gameplayway = LotterySystem.Current.FindGamePlayWay(gpw.Key);
                    var gpwBetAmount = gpw.Sum(it => it.TotalBetAmount);

                    gpwDic.Add(gameplayway, gpwBetAmount);
                }

                var companyBetAmount = gpwDic.Sum(it => it.Value);
                amountList.Add(new CompanyAmountInfo
                {
                    Company = company,
                    CompanyBetAmount = companyBetAmount,
                    GamePlayWayBetAmount = gpwDic
                });
            }
            return amountList;
        }


        private void CheckUpdate()
        {
            var todayCompany = TodayLotteryCompany.Instance.GetTodayCompany();
            List<string> removeItem = null;
            foreach (var item in ContextDic.Keys.ToList())
            {
                var keyInfo = item.Split('_');
                int companyId = int.Parse(keyInfo[2]);
                int gameplayway = int.Parse(keyInfo[3]);
                if (!todayCompany.Contains(it => it.CompanyId == companyId))
                {
                    if (removeItem == null)
                        removeItem = new List<string>();
                    removeItem.Add(item);
                    continue;
                }
                if (ContextDic[item]) //只有被修改的限制上下文才进行更新
                {
                    var limitContext = GetLimitContextInCache(companyId, gameplayway);
                    if (limitContext != null)
                    {
                        LogConsole.Debug(string.Format("LimitContext Company:{0} GPW:{1} Size:{2}", companyId, gameplayway, limitContext.Count));
                        Update(limitContext);
                    }
                    ContextDic[item] = false;
                }
            }

            //删除要删除的项目
            if (removeItem != null)
                foreach (var item in removeItem)
                    ContextDic.Remove(item);
        }
        private void Update(IDictionary<string, BetUpperLimit> limits)
        {
            foreach (string key in limits.Keys)
            {
                if (limits[key].IsChange)
                {
                    lock (limits[key])      //锁定实体
                    {
                        var limit = limits[key];
                        var amount = DaOrder.SumTotalBetAmount(limit.CompanyId, limit.GamePlayWayId, limit.Num);
                        LogConsole.Debug(string.Format("Update UpperLimit:Num:{0} Company:{1} GPW:{2} TotalAmount:{3} Amount:{4}", limit.Num, limit.CompanyId, limit.GamePlayWayId, limit.TotalBetAmount, amount));
                        if (limit.TotalBetAmount != amount)
                            limit.TotalBetAmount = amount;
                        LimitManager.UpdateLimit(limit);
                        //limits[key].TotalBetAmount = amount;
                        //将已修改状态改变
                        limits[key].IsChange = false;
                    }
                }
            }
        }
    }
}
