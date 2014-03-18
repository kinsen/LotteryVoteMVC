using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core.Authorizes
{
    /// <summary>
    /// 基本代理验证,包括所有代理和分身
    /// </summary>
    public class AgentAuthorizeAttribute : UserAuthorizeAttribute
    {
        public AgentAuthorizeAttribute(UserState thresholdState)
            : base(thresholdState, new[] { Role.Company, Role.Super, Role.Master, Role.Agent, Role.Shadow })
        {
        }

        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
        }
    }
}
