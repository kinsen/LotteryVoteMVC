using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Utility;
using System.Diagnostics;
using System.Web;

namespace LotteryVoteMVC.Core.Web
{
    /// <summary>
    /// 请求消耗记录器
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestCostTimeFilterAttribute : ActionFilterAttribute
    {
        private const string M_BEGIN = "REQUESTBETINTIME";
        private const string M_END = "REQUESTENDTIME";
        private const string M_COST = "REQUESTCOST";
        private Stopwatch sw;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.HttpContext.Items[M_BEGIN] = DateTime.Now;
            sw = new Stopwatch();
            sw.Start();
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            filterContext.HttpContext.Items[M_END] = DateTime.Now;
            filterContext.HttpContext.Items[M_COST] = sw.ElapsedMilliseconds;
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            if (sw == null) return;

            var view = sw.ElapsedMilliseconds;
            DateTime now = DateTime.Now;
            sw.Stop();
            DateTime? beginTime = filterContext.HttpContext.Items[M_BEGIN] as DateTime?;
            DateTime? endTime = filterContext.HttpContext.Items[M_END] as DateTime?;
            var execute = filterContext.HttpContext.Items[M_COST];
            LogConsole.Info(string.Format("{4}:请求时间{2},处理完成时间:{3},逻辑消耗:{0}毫秒,呈现消耗:{1}毫秒", execute, view, beginTime.HasValue ? beginTime.Value.ToLongTimeString() : string.Empty,
                endTime.HasValue ? endTime.Value.ToLongTimeString() : string.Empty, HttpContext.Current.User.Identity.Name));
        }
    }
}
