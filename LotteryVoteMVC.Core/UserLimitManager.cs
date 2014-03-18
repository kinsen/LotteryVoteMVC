using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 用户限制管理器
    /// </summary>
    public class UserLimitManager : ManagerBase
    {
        private BetLimitDataAccess _daBetLimit;
        private GameBetLimitDataAccess _daGameBetLimit;

        public BetLimitDataAccess DaBetLimit
        {
            get
            {
                if (_daBetLimit == null)
                    _daBetLimit = new BetLimitDataAccess();
                return _daBetLimit;
            }
        }
        public GameBetLimitDataAccess DaGameBetLimit
        {
            get
            {
                if (_daGameBetLimit == null)
                    _daGameBetLimit = new GameBetLimitDataAccess();
                return _daGameBetLimit;
            }
        }

        #region BetLimit
        /// <summary>
        /// 更新用户下注限制（先删除，再插入）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="limits">The limits.</param>
        public void UpdateBetLimit(User user, IEnumerable<BetLimit> limits)
        {
            if (user.Role > Role.Company)
            {
                var parentLimits = DaBetLimit.ListLimitByUser(user.ParentId);
                foreach (var plimit in parentLimits)
                {
                    var limit = limits.Find(it => it.GameId == plimit.GameId);
                    if (limit == null)
                        throw new InvalidDataException("limit", string.Format("不存在GameType:{0}的限制,UserId:{1}", plimit.GameType, user.UserId));
                    if (limit.LeastLimit < plimit.LeastLimit || limit.LargestLimit > plimit.LargestLimit)
                    {
                        var desc = EnumHelper.GetEnumDescript(plimit.GameType).Description;
                        string message = string.Format(Resources.Models.ModelResource.OverOutRange, desc, plimit.LeastLimit, plimit.LargestLimit);
                        throw new BusinessException(message);
                    }
                }
            }
            limits = limits.ForEach(it => { it.UserId = user.UserId; return it; });
            DaBetLimit.ExecuteWithTransaction(() =>
            {
                DaBetLimit.DeleteLimit(user);
                DaBetLimit.InsertLimits(limits);
            });

        }
        public IEnumerable<BetLimit> GetLimits(User user)
        {
            return GetLimits(user.UserId);
        }
        public IEnumerable<BetLimit> GetLimits(int userId)
        {
            return DaBetLimit.ListLimitByUser(userId);
        }
        #endregion

        #region GameBetLimit
        public void UpdateGameLimit(User user, CompanyType companyType, IEnumerable<GameBetLimit> limits)
        {
            if (user.Role > Role.Company)
            {
                var parentLimits = DaGameBetLimit.GetLimits(user.ParentId, companyType);
                foreach (var plimit in parentLimits)
                {
                    var limit = limits.Find(it => it.GamePlayWayId == plimit.GamePlayWayId);
                    if (limit == null)
                        throw new InvalidDataException("limit", string.Format("不存在GamePlayTypeId:{0}的限制,UserId:{1}", plimit.GamePlayWayId, user.UserId));
                    if (limit.LimitValue > plimit.LimitValue)
                    {
                        string message = string.Format(Resources.Models.ModelResource.OverOutRange, GPWManager.ToString(plimit.GamePlayWayId), 0, plimit.LimitValue);
                        throw new BusinessException(message);
                    }
                }
            }
            limits = limits.ForEach(it => { it.UserId = user.UserId; return it; });
            DaGameBetLimit.ExecuteWithTransaction(() =>
            {
                DaGameBetLimit.Delete(user, companyType);
                DaGameBetLimit.InsertLimits(limits);
            });
        }
        public IEnumerable<GameBetLimit> GetGameLimits(User user)
        {
            return GetGameLimits(user.UserId);
        }
        public IEnumerable<GameBetLimit> GetGameLimits(int userId)
        {
            return DaGameBetLimit.ListLimitByUser(userId);
        }
        #endregion
    }
}
