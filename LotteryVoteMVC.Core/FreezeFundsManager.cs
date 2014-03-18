using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources.Models;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    public class FreezeFundsManager : ManagerBase
    {
        private FreezeFundsDataAccess _daFreezeFunds;
        public FreezeFundsDataAccess DaFreezeFunds
        {
            get
            {
                if (_daFreezeFunds == null)
                    _daFreezeFunds = new FreezeFundsDataAccess();
                return _daFreezeFunds;
            }
        }
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        #region Action
        /// <summary>
        /// 冻结用户资金.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="amount">The amount.</param>
        public void FreezeUserFunds(User user, decimal amount)
        {
            var userInfo = UserManager.DaUserInfo.GetUserInfo(user.UserId);
            if (userInfo.AvailableGivenCredit < amount)
                throw new BusinessException(string.Format(ModelResource.MustLessThan, Resource.TotalBetAmount, userInfo.AvailableGivenCredit));
            userInfo.AvailableGivenCredit -= amount;
            DaFreezeFunds.ExecuteWithTransaction(() =>
            {
                UserManager.Tandem(DaFreezeFunds);
                UserManager.DaUserInfo.Update(userInfo);
                AddFreezeFunds(user, amount);
                UserManager.ClearTandem();
            });
        }
        public void UnFreezeUserFunds(int userId, decimal amount)
        {
            var userInfo = UserManager.DaUserInfo.GetUserInfo(userId);
            userInfo.AvailableGivenCredit += amount;
            DaFreezeFunds.ExecuteWithTransaction(() =>
            {
                UserManager.Tandem(DaFreezeFunds);
                UserManager.DaUserInfo.Update(userInfo);
                UnFreezeFunds(userId, amount);
                UserManager.ClearTandem();
            });
        }
        /// <summary>
        /// 添加用户冻结资金，如果存在则累加，否则新增
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="amount">The amount.</param>
        private void AddFreezeFunds(User user, decimal amount)
        {
            var freezeFund = DaFreezeFunds.GetFreezeFundsByUser(user.UserId, BetStatus.Valid);
            if (freezeFund != null)
            {
                freezeFund.Amount += amount;
                DaFreezeFunds.Update(freezeFund);
            }
            else
            {
                freezeFund = new FreezeFunds
                {
                    UserId = user.UserId,
                    Amount = amount,
                    Status = BetStatus.Valid
                };
                DaFreezeFunds.Insert(freezeFund);
            }
        }
        /// <summary>
        ///解冻资金.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="amount">The amount.</param>
        private void UnFreezeFunds(int userId, decimal amount)
        {
            var freezeFund = DaFreezeFunds.GetFreezeFundsByUser(userId, BetStatus.Valid);
            if (freezeFund == null)
                throw new InvalidDataException("user have not valid freeze funds,userId:" + userId);
            if (freezeFund.Amount - amount < 0)
                throw new InvalidDataException(string.Format("解冻资金异常，冻结资金:{0},UserId:{1},还剩冻结资金:{2}", amount, userId, freezeFund.Amount));
            freezeFund.Amount -= amount;
            DaFreezeFunds.Update(freezeFund);
        }
        #endregion

        #region Select
        public FreezeFunds GetFreezeFund(User user)
        {
            return DaFreezeFunds.GetFreezeFundsByUser(user.UserId, BetStatus.Valid);
        }
        #endregion
    }
}
