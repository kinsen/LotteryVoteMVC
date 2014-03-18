using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using System.Web;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Core.Web
{
    /// <summary>
    /// 单一登录过滤器，使得单一用户同一时间段只能有一个人登录
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class SingleLoginFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!LoginCenter.IsUserLogin() || LoginCenter.CurrentUser.Role == Role.Company) return;

            var onLine = HttpContext.Current.Application[LotterySystem.M_ONLINEUSERCOUNT] as IDictionary<int, Pair<string, Role>>;
            string sessionId = HttpContext.Current.Session.SessionID;
            int userId = onLine.Where(it => it.Value.Key == sessionId).Select(it => it.Key).SingleOrDefault();
            if (!onLine.ContainsKey(userId))
            {
                LoginCenter lotinCentre = new LoginCenter();
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new JsonResultModel
                        {
                            IsSuccess = false,
                            Message = Resource.LoginByOtherOne
                        }
                    };
                }
                else
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    string actionUrl = urlHelper.Action("Error", "Prompt");
                    string loginUrl = urlHelper.Action(LoginCenter.CurrentUser.Role == Role.Guest ? "Login" : "Agent", "Member");
                    string redirectUrl = string.Format("{0}?Url={1}&Msg={2}", actionUrl, loginUrl, Resource.LoginByOtherOne);
                    filterContext.Result = new RedirectResult(redirectUrl);
                }
                lotinCentre.Logout();
            }
        }
    }
}
