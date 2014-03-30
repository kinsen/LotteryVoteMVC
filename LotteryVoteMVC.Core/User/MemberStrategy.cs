using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core
{
    internal class MemberStrategy : UserStrategy
    {
        #region params
        private IEnumerable<LotterySpecies> species;
        private int[] groupIds;
        #endregion

        public override void AddUser(Models.User user)
        {
            if (user.UserInfo.RateGroupId > 1)
                user.UserInfo.RateGroupId = user.UserInfo.RateGroupId;
            species = this.ParseParam<IEnumerable<LotterySpecies>>(SPECIES);
            groupIds = this.ParseParam<int[]>(GROUPIDS);

            var parent = UserManager.GetUser(user.ParentId);
            user.Role = parent.Role + 1;
            user.UserInfo.AvailableGivenCredit = user.UserInfo.GivenCredit;
            CheckShareRate(parent, user);
            CheckGivenCredit(user, parent);
            var betLimitList = DaBetLimit.ListLimitByUser(user.ParentId);
            var gameBetLimitList = DaGameBetLimit.ListLimitByUser(user.ParentId);
            var groups = CommManager.GetCommGroups(groupIds);
            UserManager.DaUser.ExecuteWithTransaction(() =>
            {
                CommManager.Tandem(UserManager.DaUser);
                InsertUser(user);
                UpdateParentGivenCredit(parent, parent.UserInfo.AvailableGivenCredit - user.UserInfo.GivenCredit);

                betLimitList = betLimitList.ForEach(it => { it.UserId = user.UserId; return it; });
                gameBetLimitList = gameBetLimitList.ForEach(it => { it.UserId = user.UserId; return it; });

                InitUserCommission(user, species);
                InitUserBetLimit(user, betLimitList);
                InitUserGameBetLimit(user, gameBetLimitList);
                AddMemberPackage(user, groups);

                CommManager.ClearTandem();
            });
        }

        private void AddMemberPackage(User user, IEnumerable<CommissionGroup> groups)
        {
            foreach (var group in groups)
            {
                CommManager.AddMemberPackage(user, group);
            }
        }
    }
}
