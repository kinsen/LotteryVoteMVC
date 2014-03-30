using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Core
{
    public class UserManager : ManagerBase
    {
        public UserDataAccess _daUser;
        public UserInfoDataAccess _daUserInfo;
        private CommManager _commManager;
        private ShadowAuthManager _authManger;
        public UserDataAccess DaUser
        {
            get
            {
                if (_daUser == null)
                    _daUser = new UserDataAccess();
                return _daUser;
            }
        }
        public UserInfoDataAccess DaUserInfo
        {
            get
            {
                if (_daUserInfo == null)
                    _daUserInfo = new UserInfoDataAccess();
                return _daUserInfo;
            }
        }
        public CommManager CommManager
        {
            get
            {
                if (_commManager == null)
                    _commManager = new CommManager();
                return _commManager;
            }
        }
        public ShadowAuthManager AuthManager
        {
            get
            {
                if (_authManger == null)
                    _authManger = new ShadowAuthManager();
                return _authManger;
            }
        }
        public ShareRateGroupManager ShareRateGroupManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<ShareRateGroupManager>();
            }
        }

        #region Select
        public bool IsExsit(string userName)
        {
            return DaUser.GetUserByUserName(userName) != null;
        }
        public bool IsParent(int parentId, int userId, out User user)
        {
            var familys = DaUser.GetParentUser(userId);
            if (familys.Contains(it => it.UserId == parentId))
            {
                user = familys.Find(it => it.UserId == userId);
                return true;
            }
            user = null;
            return false;
        }
        public User GetUser(int userId)
        {
            return DaUser.GetUserById(userId);
        }

        public IEnumerable<User> GetMembers()
        {
            return DaUser.GetUserByRole(Role.Guest);
        }
        public IEnumerable<User> GetChilds(User user)
        {
            return DaUser.GetChild(user, user.Role + 1);
        }
        public IEnumerable<User> GetShadows(User user)
        {
            return DaUser.GetChild(user, Role.Shadow);
        }
        /// <summary>
        /// 获取用户所在家庭（所有父级，并包括自身）
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        public IEnumerable<User> GetFamily(int userId)
        {
            return DaUser.GetParentUser(userId);
        }

        /// <summary>
        /// 获取今日下注用户
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetBetUsers()
        {
            return DaUser.GetBetUsers();
        }

        public IEnumerable<User> GetUserByCondition(string username, string name, UserState state, string sortField, User user)
        {
            return DaUser.GetUserByCondition(name, username, state, user.Role + 1, user, sortField);
        }
        public IEnumerable<User> GetShadowByCondition(string username, string name, UserState state, string sortField, User user)
        {
            return DaUser.GetUserByCondition(name, username, state, Role.Shadow, user, sortField);
        }

        #endregion

        #region Insert
        public void AddSuper(User user, IEnumerable<LotterySpecies> species)
        {
            var userStrategy = UserStrategyFactory.GetFactory().GetUserStrategy(Role.Super, this);
            userStrategy.AddParam(UserStrategy.ROLE, Role.Super);
            userStrategy.AddParam(UserStrategy.SPECIES, species);
            userStrategy.AddUser(user);
        }
        public void AddMaster(User user, IEnumerable<LotterySpecies> species)
        {
            var userStrategy = UserStrategyFactory.GetFactory().GetUserStrategy(Role.Master, this);
            userStrategy.AddParam(UserStrategy.ROLE, Role.Master);
            userStrategy.AddParam(UserStrategy.SPECIES, species);
            userStrategy.AddUser(user);
        }
        public void AddAgent(User user, IEnumerable<LotterySpecies> species)
        {
            var userStrategy = UserStrategyFactory.GetFactory().GetUserStrategy(Role.Agent, this);
            userStrategy.AddParam(UserStrategy.ROLE, Role.Agent);
            userStrategy.AddParam(UserStrategy.SPECIES, species);
            userStrategy.AddUser(user);
        }
        public void AddMember(User user, IEnumerable<LotterySpecies> species, int commGroup)
        {
            var userStrategy = UserStrategyFactory.GetFactory().GetUserStrategy(Role.Guest, this);
            userStrategy.AddParam(UserStrategy.SPECIES, species);
            userStrategy.AddParam(UserStrategy.GROUPIDS, new[] { commGroup });
            userStrategy.AddUser(user);
        }
        public void AddShadow(User user, int[] auths)
        {
            var userStrategy = UserStrategyFactory.GetFactory().GetUserStrategy(Role.Shadow, this);
            userStrategy.AddParam(UserStrategy.AUTHS, auths);
            userStrategy.AddUser(user);
        }
        #endregion

        #region Update
        public void UpdatePwd(User user, string newPassword)
        {
            var encryptPwd = EncryptHelper.Encrypt(newPassword);
            if (encryptPwd == user.UserInfo.Password)
                throw new BusinessException(Resource.NewPwdCantEqualsOlds);
            user.UserInfo.Password = encryptPwd;
            user.UserInfo.LastChangePwd = DateTime.Now;
            DaUserInfo.Update(user.UserInfo);
        }
        public User UpdateUser(User user, IDictionary<LotterySpecies, int> memberpacks, out bool changeShareRate)
        {
            var originalUser = GetUser(user.UserId);
            var parent = GetUser(originalUser.ParentId);
            //user.UserInfo.RateGroupId /= 100; //先将百分比转换成小数
            //1.检查信用额
            CheckGivenCredit(ref originalUser, user, parent);
            //2.检查分成
            changeShareRate = CheckShareRate(ref originalUser, user, parent);
            originalUser.UserInfo.Name = user.UserInfo.Name;
            originalUser.UserInfo.State = user.UserInfo.State;
            originalUser.UserInfo.Email = user.UserInfo.Email;
            DaUserInfo.ExecuteWithTransaction(() =>
            {
                DaUserInfo.Update(originalUser.UserInfo);
                DaUserInfo.Update(parent.UserInfo);
            });
            //3.检查分组
            if (user.Role == Role.Guest && (memberpacks != null && memberpacks.Count > 0))
            {
                var commGroups = CommManager.GetCommGroups(memberpacks.Values.ToArray());
                foreach (var group in commGroups)
                    CommManager.UpdateMemberPackage(user, group);
            }
            return originalUser;
        }
        public void UpdateShadow(User shadow, int[] auths)
        {
            DaUserInfo.ExecuteWithTransaction(() =>
            {
                AuthManager.Tandem(DaUserInfo);
                DaUserInfo.Update(shadow.UserInfo); //分身用户只可能更改信息
                AuthManager.DeleteShadowAuth(shadow);
                AuthManager.AddShadowAuth(shadow, auths);
                AuthManager.ClearTandem();
            });
        }
        public void ChangeState(User user, UserState state)
        {
            DaUserInfo.UpdateState(user, state);
        }
        /// <summary>
        /// 修改家族状态（以传入用户为根节点）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="state">The state.</param>
        /// <param name="parent">The parent.</param>
        public void ChangeFamilyState(User user, UserState state)
        {
            DaUserInfo.UpdateFamilyState(user, state);
        }
        private void CheckGivenCredit(ref User originalUser, User updateUser, User parent)
        {
            var originalCredit = originalUser.UserInfo.GivenCredit;
            var updateCredit = updateUser.UserInfo.GivenCredit;
            //已使用掉的信用额
            var usedCredit = originalCredit - originalUser.UserInfo.AvailableGivenCredit;

            //1.如果修改的信用额大于原先的信用额
            if (updateCredit > originalCredit)
            {
                var diff = updateCredit - originalCredit;
                if (parent.UserInfo.AvailableGivenCredit < diff)
                {
                    string error = string.Format(ModelResource.MustLessThan, Resource.GivenCredit, originalCredit + parent.UserInfo.AvailableGivenCredit);
                    throw new BusinessException(error);
                }
                parent.UserInfo.AvailableGivenCredit -= diff;
                originalUser.UserInfo.GivenCredit = updateCredit;
                originalUser.UserInfo.AvailableGivenCredit = updateCredit - usedCredit;
            }
            else if (updateCredit < originalCredit)     //修改的信用额小于原先的信用额
            {
                //要修改的信用额小于用户已使用的信用额，则必须全部回收子用户的信用额，但须考虑到会员可能已经下注，资金已冻结，未开奖，
                //故保留会员的信用额为冻结资金，可用信用额为0
                if (updateCredit < usedCredit)
                {
                    //获取家族树，并倒序，这样在所有用户信用额回滚时确保数据不会凌乱
                    var family = DaUser.GetFamily(originalUser).OrderByDescending(it => it.UserId).ToList();
                    foreach (var child in family)
                    {
                        var rollBackCredit = child.UserInfo.AvailableGivenCredit;   //获取子用户可回滚的信用额
                        decimal childUsedCredit = child.UserInfo.GivenCredit - child.UserInfo.AvailableGivenCredit; //获取子用户已使用的信用额
                        child.UserInfo.AvailableGivenCredit = 0;        //子用户可用信用额为0
                        child.UserInfo.GivenCredit = childUsedCredit;   //子用户信用额为已使用的信用额
                        var childParent = family.Find(it => it.UserId == child.ParentId);
                        if (childParent != null)
                        {
                            childParent.UserInfo.AvailableGivenCredit += rollBackCredit;    //父级用户可用信用额累加
                            DaUserInfo.Update(child.UserInfo);
                        }
                        if (child.UserId == originalUser.UserId)
                        {
                            originalUser = child;
                            parent.UserInfo.AvailableGivenCredit += rollBackCredit;
                        }
                    }
                }
                else
                {
                    var diff = originalCredit - updateCredit;
                    parent.UserInfo.AvailableGivenCredit += diff;

                    originalUser.UserInfo.GivenCredit = updateCredit;
                    originalUser.UserInfo.AvailableGivenCredit = updateCredit - usedCredit;
                }
            }
        }
        /// <summary>
        /// 检查更新用户的分成（originalUser获得新的分成数）
        /// </summary>
        /// <param name="originalUser">The original user.</param>
        /// <param name="updateUser">The update user.</param>
        private bool CheckShareRate(ref User originalUser, User updateUser, User parent)
        {
            if (updateUser.UserInfo.RateGroup == null)
                updateUser.UserInfo.RateGroup = ShareRateGroupManager.GetGroup(updateUser.UserInfo.RateGroupId);
            //如果大于父级的分成，则不变
            if (updateUser.UserInfo.RateGroup.ShareRate > parent.UserInfo.RateGroup.ShareRate)
                return false;
            if (updateUser.UserInfo.RateGroup.ShareRate < originalUser.UserInfo.RateGroup.ShareRate)
            {
                //如果回收的分成比原来分成少。则子用户分成全部回收
                //DaUserInfo.UpdateFamilyShareRate(originalUser, 0);
                //如果回收的部分原来的分成少，则子用户分成全部都一致
                DaUserInfo.UpdateFamilyShareRate(originalUser, updateUser.UserInfo.RateGroupId);
            }
            bool change = originalUser.UserInfo.RateGroup.ShareRate != updateUser.UserInfo.RateGroup.ShareRate;
            originalUser.UserInfo.RateGroupId = updateUser.UserInfo.RateGroupId;
            return change;
        }
        #endregion
    }
}
