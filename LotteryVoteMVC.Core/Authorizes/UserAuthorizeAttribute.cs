using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using System.Web;
using LotteryVoteMVC.Core.Exceptions;
using System.Configuration;

namespace LotteryVoteMVC.Core.Authorizes
{
    public class UserAuthorizeAttribute : AuthorizeAttribute
    {
        private bool _isNormal = false;
        private Role[] AuthorizedRoles;
        private LoginCenter _loginCer;
        /// <summary>
        /// 底限状态
        /// </summary>
        private UserState ThresholdState;
        /// <summary>
        /// 是否普通级别，若是，则无需上级授权分身用户也能直接访问
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is normal; otherwise, <c>false</c>.
        /// </value>
        public bool IsNormal
        {
            get { return _isNormal; }
            set { _isNormal = value; }
        }
        public UserAuthorizeAttribute(UserState thresholdState = UserState.Suspended, params Role[] authorizedRoles)
        {
            ThresholdState = thresholdState;
            this.AuthorizedRoles = authorizedRoles;
        }

        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }
        public ShadowAuthManager ShadowAuthManager
        {
            get { return ManagerHelper.Instance.GetManager<ShadowAuthManager>(); }
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

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!LoginCenter.IsLogin)
            {
                filterContext.Result = new RedirectResult(Extended.GetLoginUrl());
                return;
            }
            bool isAllowed = LoginCenter.IsLogin;
            if (isAllowed && !IsNormal)
            {
                var user = LoginCenter.GetCurrentUser();
                //必须存在用户，并且用户的状态满足底限状态
                isAllowed = AuthorizedRoles.Contains(user.Role) && user.UserInfo.State <= ThresholdState;
                if (user.Role == Role.Shadow)
                {
                    //如果是影子用户，则必须要有该模块的访问权限，并且实体用户有该模块的访问权限
                    isAllowed = this.HasVistAuthor(filterContext) && AuthorizedRoles.Contains(UserManager.GetUser(user.ParentId).Role);
                }
            }
            if (!isAllowed)
            {
                throw new NoPermissionException("越级访问");
            }
        }


        private bool HasVistAuthor(AuthorizationContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"].ToString() + "Controller";   //默认都以Controller结尾
            var action = filterContext.RouteData.Values["action"].ToString();
            var actionSign = GetActionSign(filterContext.ActionDescriptor);
            var auths = ShadowAuthManager.GetShadowAuth();

            Spec<AgentAuthorizeAction> condition = it => it.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase);
            condition = condition.And(it => it.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
            condition = condition.And(it => it.MethodSign.Equals(actionSign, StringComparison.InvariantCultureIgnoreCase));

            var auth = auths.Find(it => condition(it));
            return auth != null;
        }

        /// <summary>
        /// 获取Action的方法签名
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        private string GetActionSign(ActionDescriptor descriptor)
        {
            return string.Format("{0}({1})", GetActionHttpAttr(descriptor), GetActionParamsString(descriptor.GetParameters()));
        }

        /// <summary>
        ///获取Action的Http标记
        /// </summary>
        /// <param name="descriptor">The descriptor.</param>
        /// <returns></returns>
        private string GetActionHttpAttr(ActionDescriptor descriptor)
        {
            var attrs = descriptor.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), false);
            object attrType = typeof(HttpGetAttribute);//默认使用HttpGet
            if (attrs.Length > 0)
                attrType = attrs[0];

            string fullTypeName = attrType.ToString();
            string typeName = fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1);
            var actionHttpAttr = typeName.Replace("Attribute", string.Empty);
            return actionHttpAttr;
        }

        /// <summary>
        /// 获取Action的参数签名
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns></returns>
        private string GetActionParamsString(ParameterDescriptor[] args)
        {
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var arg in args)
            {
                if (isFirst) isFirst = false;
                else sb.Append(',');
                var argType = arg.ParameterType.ToString();
                sb.AppendFormat("{0}:{1}", argType.Substring(argType.LastIndexOf('.') + 1), arg.ParameterName);
            }
            //无参数Action则用Empty表示
            if (sb.Length == 0)
                sb.Append("Empty");
            return sb.ToString();
        }

    }
}
