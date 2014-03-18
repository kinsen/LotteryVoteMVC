using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LotteryVoteMVC.Core.Web
{
    /// <summary>
    /// 清除Cache，防止客户端后退
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class NoCacheFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var response = filterContext.HttpContext.Response;
            response.Expires = 0;
            response.Buffer = true;
            response.ExpiresAbsolute = DateTime.Now.AddSeconds(-1);
            response.CacheControl = "no-cache";
            response.AddHeader("pragma", "no-cache");
            response.AddHeader("pragma", "no-store");
            response.AddHeader("Cache-Control", "no-store, no-cache, must-revalidate");
            response.Cache.SetCacheability(System.Web.HttpCacheability.NoCache);
            response.Cache.SetNoStore();
            base.OnActionExecuted(filterContext);
        }
    }
}
