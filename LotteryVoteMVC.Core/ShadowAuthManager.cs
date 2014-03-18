using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using System.Web.Mvc;
using System.Web;

namespace LotteryVoteMVC.Core
{
    public class ShadowAuthManager : ManagerBase
    {
        private const string M_SHADOWAUTHS = "ShadowAuths";
        private AgentAuthorizeActionDataAccess _daAgentAuth;
        private AgentAuthorizeInRoleDataAccess _daAuthInRole;
        private ShadowAuthorizeActionDataAccess _daShadowAuth;
        private LoginCenter _loginCer;
        public AgentAuthorizeActionDataAccess DaAgentAuth
        {
            get
            {
                if (_daAgentAuth == null)
                    _daAgentAuth = new AgentAuthorizeActionDataAccess();
                return _daAgentAuth;
            }
        }
        public AgentAuthorizeInRoleDataAccess DaAuthInRole
        {
            get
            {
                if (_daAuthInRole == null)
                    _daAuthInRole = new AgentAuthorizeInRoleDataAccess();
                return _daAuthInRole;
            }
        }
        public ShadowAuthorizeActionDataAccess DaShadowAuth
        {
            get
            {
                if (_daShadowAuth == null)
                    _daShadowAuth = new ShadowAuthorizeActionDataAccess();
                return _daShadowAuth;
            }
        }
        protected LoginCenter LoginCenter
        {
            get
            {
                if (_loginCer == null)
                    _loginCer = new LoginCenter();
                return _loginCer;
            }
        }

        #region Agent Auth
        public void AddAuth(AgentAuthorizeAction auth, IEnumerable<Role> roles)
        {
            DaAgentAuth.ExecuteWithTransaction(() =>
            {
                DaAuthInRole.Tandem(DaAgentAuth);
                DaAgentAuth.Insert(auth);
                foreach (var role in roles)
                    DaAuthInRole.Insert(auth, role);
            });
        }
        public void UpdateAuth(AgentAuthorizeAction auth, IEnumerable<Role> roles)
        {
            DaAgentAuth.ExecuteWithTransaction(() =>
            {
                DaAuthInRole.Tandem(DaAgentAuth);
                DaAgentAuth.Update(auth);
                DaAuthInRole.Delete(auth.AuthorizeId);
                foreach (var role in roles)
                    DaAuthInRole.Insert(auth, role);
            });
        }
        public void DeleteAuth(int authId)
        {
            DaAgentAuth.Delete(authId);
            DaAuthInRole.Delete(authId);
        }
        #endregion

        #region Shadow Auth
        public void AddShadowAuth(User user, int[] auths)
        {
            List<ShadowAuthorizeAction> shadowAuths = new List<ShadowAuthorizeAction>();
            foreach (int auth in auths)
                shadowAuths.Add(new ShadowAuthorizeAction
                {
                    UserId = user.UserId,
                    AuthorizeId = auth
                });
            DaShadowAuth.Insert(shadowAuths);
        }
        public void DeleteShadowAuth(User user)
        {
            DaShadowAuth.Delete(user);
        }
        #endregion

        #region Select
        public IEnumerable<Role> GetAuthRoles(int authId)
        {
            var authRoles = DaAuthInRole.GetAuthRoles(authId);
            return authRoles.Select(it => (Role)it.RoleId);
        }
        public AgentAuthorizeAction GetAuth(int authId)
        {
            return DaAgentAuth.GetAgetnAuth(authId);
        }
        public IEnumerable<AgentAuthorizeAction> GetShadowAuths(User shadow)
        {
            return DaAgentAuth.GetAuthByUser(shadow);
        }
        public IEnumerable<AgentAuthorizeAction> GetAllAuth()
        {
            return DaAgentAuth.GetAll();
        }
        /// <summary>
        /// 获取这个用户所能分配的权限.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IDictionary<string, MultiSelectList> GetAuthByUser(User user)
        {
            return DaAgentAuth.GetAuthByRole(user.Role).GroupBy(it => it.Controller)
                .ToDictionary(it => AuthResource.ResourceManager.GetString(it.Key) ?? it.Key, it => it.ToMultiSelectList(x => AuthResource.ResourceManager.GetString(x.Name) ?? x.Name, x => x.AuthorizeId.ToString(), x => x.DefaultState));
        }
        public bool IsExist(string name)
        {
            return DaAgentAuth.GetAgentAuth(name) != null;
        }
        #endregion

        /// <summary>
        /// 获取分身所拥有的权限
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public IEnumerable<AgentAuthorizeAction> GetShadowAuth()
        {
            var auths = HttpContext.Current.Session[M_SHADOWAUTHS] as IEnumerable<AgentAuthorizeAction>;
            if (auths == null)
            {
                ShadowAuthManager authManager = new ShadowAuthManager();
                auths = authManager.GetShadowAuths(LoginCenter.GetCurrentUser());
                HttpContext.Current.Session[M_SHADOWAUTHS] = auths;
            }
            return auths;
        }
        public bool HasLicense(string controller, string action)
        {
            var auth = GetShadowAuth();
            return auth.Contains(it => it.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase)
                && it.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
