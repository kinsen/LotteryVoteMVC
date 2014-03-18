using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    internal class ShadowStrategy : UserStrategy
    {
        private ShadowAuthManager _authManager;
        private int[] auths;

        internal ShadowAuthManager AuthManager
        {
            get
            {
                if (_authManager == null)
                    _authManager = new ShadowAuthManager();
                return _authManager;
            }
        }

        public override void AddUser(Models.User user)
        {
            auths = this.ParseParam<int[]>(AUTHS);
            var parent = UserManager.GetUser(user.ParentId);
            var parentAuths = AuthManager.DaAgentAuth.GetAuthByRole(parent.Role).Select(it => it.AuthorizeId).ToArray();
            foreach (int auth in auths)
                if (!parentAuths.Contains(auth))
                    throw new InvalidDataException("auth", string.Format("用户:{0}不拥有Id为{1}的模块授权", parent.UserId, auth));
            UserManager.DaUser.ExecuteWithTransaction(() =>
            {
                AuthManager.Tandem(UserManager.DaUser);
                base.InsertUser(user);
                AuthManager.AddShadowAuth(user, auths);
                AuthManager.ClearTandem();
            });
        }
    }
}
