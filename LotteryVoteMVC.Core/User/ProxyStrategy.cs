using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    internal class ProxyStrategy : UserStrategy
    {
        private IEnumerable<LotterySpecies> species;
        private Role role;
        public override void AddUser(Models.User user)
        {
            if (user.UserInfo.RateGroupId > 1)
                user.UserInfo.RateGroupId = user.UserInfo.RateGroupId;
            InitParams();
            var parent = UserManager.GetUser(user.ParentId);
            user.Role = parent.Role + 1;
            user.UserInfo.AvailableGivenCredit = user.UserInfo.GivenCredit;
            CheckShareRate(parent, user);
            CheckGivenCredit(user, parent);
            var betLimitList = DaBetLimit.ListLimitByUser(user.ParentId);
            var gameBetLimitList = DaGameBetLimit.ListLimitByUser(user.ParentId);
            UserManager.DaUser.ExecuteWithTransaction(() =>
            {
                CommManager.Tandem(UserManager.DaUser);
                InsertUser(user);
                UpdateParentGivenCredit(parent, parent.UserInfo.AvailableGivenCredit - user.UserInfo.GivenCredit);
                InitUserCommission(user, species);
                betLimitList = betLimitList.ForEach(it => { it.UserId = user.UserId; return it; });
                gameBetLimitList = gameBetLimitList.ForEach(it => { it.UserId = user.UserId; return it; });
                InitUserBetLimit(user, betLimitList);
                InitUserGameBetLimit(user, gameBetLimitList);
            });
            CommManager.ClearTandem();
        }

        private void InitParams()
        {
            species = base.ParseParam<IEnumerable<LotterySpecies>>(UserStrategy.SPECIES);
            role = base.ParseParam<Role>(UserStrategy.ROLE);
        }
    }
}
