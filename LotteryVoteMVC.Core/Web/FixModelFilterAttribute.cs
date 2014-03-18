using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Configuration;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Core.Web
{
    /// <summary>
    /// 更新模式过滤器，用户网站更新时候显示维护中页面
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class FixModelFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"].ToString();
            var action = filterContext.RouteData.Values["action"].ToString();

            if (controller == "Prompt" && action == "Maintenance") return;

            var fixing = ConfigurationManager.AppSettings["Fixing"];
            if (!string.IsNullOrEmpty(fixing) &&
                fixing.Equals("true", StringComparison.InvariantCultureIgnoreCase))
            {
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult
                    {
                        Data = new JsonResultModel
                        {
                            IsSuccess = false,
                            Message = Resource.SystemMaintenance
                        }
                    };
                }
                else
                {
                    var urlHelper = new UrlHelper(filterContext.RequestContext);
                    string actionUrl = urlHelper.Action("Maintenance", "Prompt");
                    filterContext.Result = new RedirectResult(actionUrl);
                }
            }
        }
    }
}
