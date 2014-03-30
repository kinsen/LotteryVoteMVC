using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Core
{
    internal abstract class UserStrategy
    {
        internal const string SHARERATE = "ShareRate";
        internal const string LEASTRATE = "LeastRate";
        internal const string LARGESTRATE = "LargestRate";
        internal const string SHARETYPE = "ShareType";
        internal const string SUPERSHARERATE = "SuperShareRate";
        internal const string MASTERSHARERATE = "MasterShareRate";
        internal const string SPECIES = "Species";
        internal const string GROUPIDS = "GroupIds";
        internal const string ROLE = "Role";
        internal const string AUTHS = "Auths";

        #region DA Properties
        private UserManager _userManager;
        private CommManager _commManager;
        private GameBetLimitDataAccess _daGameBetLimit;
        private BetLimitDataAccess _daBetLimit;

        internal GameBetLimitDataAccess DaGameBetLimit
        {
            get
            {
                if (_daGameBetLimit == null)
                    _daGameBetLimit = new GameBetLimitDataAccess();
                return _daGameBetLimit;
            }
            set
            {
                _daGameBetLimit = value;
            }
        }
        internal BetLimitDataAccess DaBetLimit
        {
            get
            {
                if (_daBetLimit == null)
                    _daBetLimit = new BetLimitDataAccess();
                return _daBetLimit;
            }
            set
            {
                _daBetLimit = value;
            }
        }
        public UserManager UserManager
        {
            get
            {
                if (_userManager == null)
                    _userManager = new UserManager();
                return _userManager;
            }
            set
            {
                _userManager = value;
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
        public ShareRateGroupManager ShareRateGroupManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<ShareRateGroupManager>();
            }
        }
        #endregion

        private IDictionary<string, object> _params;
        public IDictionary<string, object> ParamData
        {
            get
            {
                if (_params == null)
                    _params = new Dictionary<string, object>();
                return _params;
            }
            set
            {
                _params = value;
            }
        }

        public abstract void AddUser(User user);
        public void ClearParams()
        {
            ParamData.Clear();
        }
        public void AddParam(string paramName, object paramValue)
        {
            if (ParamData.ContainsKey(paramName))
                ParamData.Remove(paramName);
            ParamData.Add(paramName, paramValue);
        }

        /// <summary>
        /// 插入用户.
        /// </summary>
        /// <param name="user">The user.</param>
        protected void InsertUser(User user)
        {
            if (UserManager.IsExsit(user.UserName))
                throw new BusinessException(Resource.UserAlreadyExist);
            user.UserInfo.Password = EncryptHelper.Encrypt(user.UserInfo.Password);

            UserManager.DaUser.Insert(user);
            user.UserInfo.UserId = user.UserId;
            UserManager.DaUserInfo.Tandem(UserManager.DaUser);
            UserManager.DaUserInfo.Insert(user.UserInfo);
        }
        /// <summary>
        /// 解析参数.
        /// </summary>
        /// <typeparam name="T">参数类型</typeparam>
        /// <param name="paramName">参数名称.</param>
        /// <param name="mustExist">是否必须存在.</param>
        /// <returns></returns>
        protected T ParseParam<T>(string paramName, bool mustExist = true)
        {
            if (!ParamData.ContainsKey(paramName))
                throw new ArgumentNullException(paramName);
            object paramValue = ParamData[paramName];
            if (paramValue == null)
            {
                if (mustExist)
                    throw new ArgumentNullException(paramName);
                else
                    paramValue = default(T);
            }

            T returnVal = (T)paramValue;
            return returnVal;
        }
        /// <summary>
        ///检查父级别角色.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="role">The role.</param>
        protected void CheckParentRole(User parent, Role role)
        {
            if (parent.Role != role)
                throw new NoPermissionException("新增用户", string.Format("用户{0},Role:{1}新增Role为{2}的用户", parent.UserName, parent.Role, role));
        }
        /// <summary>
        /// 检查信用额
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="parent">The parent.</param>
        protected void CheckGivenCredit(User user, User parent)
        {
            if (user.UserInfo.GivenCredit > parent.UserInfo.AvailableGivenCredit)
                throw new BusinessException(Resource.GivenCredit);
        }
        /// <summary>
        /// 初始化用户佣金设置.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="species">The species.</param>
        protected void InitUserCommission(User user, IEnumerable<LotterySpecies> species)
        {
            CommManager.AddDefaultUserCommission(user, species);
        }
        /// <summary>
        /// 初始化用户的下注限制
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="betLimitList">限制列表，请注意必须先将UserId指向新用户.</param>
        protected void InitUserBetLimit(User user, IEnumerable<BetLimit> betLimitList)
        {
            DaBetLimit.Tandem(UserManager.DaUser);
            DaBetLimit.InsertLimits(betLimitList);
        }
        /// <summary>
        /// 初始化用户的游戏下注限制
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="betLimitList">限制列表，请注意必须先将UserId指向新用户.</param>
        protected void InitUserGameBetLimit(User user, IEnumerable<GameBetLimit> betLimitList)
        {
            DaGameBetLimit.Tandem(UserManager.DaUser);
            DaGameBetLimit.InsertLimits(betLimitList);
        }
        /// <summary>
        /// 更新父级用户可用信用.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="credit">可用信用额.</param>
        protected void UpdateParentGivenCredit(User user, decimal credit)
        {
            user.UserInfo.AvailableGivenCredit = credit;
            UserManager.DaUserInfo.Tandem(UserManager.DaUser);
            UserManager.DaUserInfo.Update(user.UserInfo);
        }
        protected void CheckShareRate(User parent, User user)
        {
            if (user.UserInfo.RateGroup == null)
                user.UserInfo.RateGroup = ShareRateGroupManager.GetGroup(user.UserInfo.RateGroupId);
            if (user.UserInfo.RateGroup.ShareRate > parent.UserInfo.RateGroup.ShareRate)
                throw new BusinessException(Resource.ShareRate);
        }
    }
}
