using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 分成组限制管理器
    /// </summary>
    public class GroupLimitManager : ManagerBase
    {
        private RateGroupGameBetLimitDataAccess _daRateGroupGameBetLimit;
        private RateGroupBetLimitDataAccess _daRateGroupBetLimit;

        public RateGroupGameBetLimitDataAccess DaRateGroupGameBetLimit
        {
            get
            {
                if (_daRateGroupGameBetLimit == null)
                    _daRateGroupGameBetLimit = new RateGroupGameBetLimitDataAccess();
                return _daRateGroupGameBetLimit;
            }
        }
        public RateGroupBetLimitDataAccess DaRateGroupBetLimit
        {
            get
            {
                if (_daRateGroupBetLimit == null)
                    _daRateGroupBetLimit = new RateGroupBetLimitDataAccess();
                return _daRateGroupBetLimit;
            }
        }

        #region BetLimit
        /// <summary>
        /// 更新用户下注限制（先删除，再插入）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="limits">The limits.</param>
        public void UpdateBetLimit(int groupId, IEnumerable<RateGroupBetLimit> limits)
        {
            limits = limits.ForEach(it => { it.GroupId = groupId; return it; });
            DaRateGroupBetLimit.ExecuteWithTransaction(() =>
            {
                DaRateGroupBetLimit.DeleteLimit(groupId);
                DaRateGroupBetLimit.InsertLimits(limits);
            });

        }
        public IEnumerable<RateGroupBetLimit> GetLimits(int groupId)
        {
            return DaRateGroupBetLimit.ListLimitByGroup(groupId);
        }
        #endregion

        #region GameBetLimit
        public void UpdateGameLimit(int groupId, CompanyType companyType, IEnumerable<RateGroupGameBetLimit> limits)
        {
            //TODO:考虑是否要加上限制检查
            limits = limits.ForEach(it => { it.GroupId = groupId; return it; });
            DaRateGroupGameBetLimit.ExecuteWithTransaction(() =>
            {
                DaRateGroupGameBetLimit.Delete(groupId, companyType);
                DaRateGroupGameBetLimit.InsertLimits(limits);
            });
        }
        public IEnumerable<RateGroupGameBetLimit> GetGameLimits(int groupId)
        {
            return DaRateGroupGameBetLimit.ListLimitByGroup(groupId);
        }
        #endregion
    }
}
