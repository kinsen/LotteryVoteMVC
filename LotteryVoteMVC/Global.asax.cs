using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Web;

namespace LotteryVoteMVC
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new NoCacheFilterAttribute());
            filters.Add(new SingleLoginFilterAttribute());
            filters.Add(new FixModelFilterAttribute());
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapRoute("AccountShadow",
                "Account/Search/{Id}/{shadow}",
                new { controller = "Account", action = "Search", Id = UrlParameter.Optional, shadow = false });

            routes.MapRoute("Bet2D",
                "Bet/2D/{type}",
                new { controller = "Bet", action = "Index", type = UrlParameter.Optional });
            routes.MapRoute("Bet345D",
                "Bet/345D/{type}",
                new { controller = "Bet", action = "MultiD", type = UrlParameter.Optional });
            routes.MapRoute("BetRollParlay",
                "Bet/RollParlay/{type}",
                new { controller = "Bet", action = "RollParlay", type = UrlParameter.Optional });

            routes.MapRoute("BetZodiac",
                "Bet/Zodiac/{type}",
                new { controller = "Bet", action = "Zodiac", type = UrlParameter.Optional });

            routes.MapRoute("GetCommission",
                "Commission/GetCommission/{companyId}/{gameType}",
                new { controller = "Commission", action = "GetCommission", companyId = UrlParameter.Optional, gameType = UrlParameter.Optional });
            #region Limit
            routes.MapRoute("GameLimit",
                "Limit/GameLimit/{id}/{companytype}",
                new { controller = "Limit", action = "GameLimit", id = UrlParameter.Optional, companytype = UrlParameter.Optional });
            routes.MapRoute("RateGroupGameLimit",
                "ShareRateGroup/GameLimit/{id}/{companytype}",
                new { controller = "ShareRateGroup", action = "GameLimit", id = UrlParameter.Optional, companytype = UrlParameter.Optional });
            routes.MapRoute("EachLevelReport",
                "Report/EachLevel/{RoleId}/{UserId}",
                new { controller = "Report", action = "EachLevel", UserId = UrlParameter.Optional, RoleId = UrlParameter.Optional });
            routes.MapRoute("UpperMonitorDetail",
                "Limit/UpperMonitorDetail/{CompanyId}/{GamePlayWayId}",
                new { controller = "Limit", action = "UpperMonitorDetail", CompanyId = UrlParameter.Optional, GamePlayWayId = UrlParameter.Optional });
            routes.MapRoute("UpdateUpperLimit",
                "Limit/UpdateUpperLimit/{LimitId}/{AcceptBet}",
                new { controller = "Limit", action = "UpdateUpperLimit", LimitId = UrlParameter.Optional, AcceptBet = UrlParameter.Optional });
            routes.MapRoute("StopAcceptBet",
                "Limit/StopAcceptBet/{CompanyId}/{GamePlayWayId}",
                new { controller = "Limit", action = "StopAcceptBet" });
            #endregion
            #region Manager
            routes.MapRoute("ShadowAuthorize",
               "ShadowAuthorize/GetActions/{controllerName}",
               new { controller = "ShadowAuthorize", action = "GetActions", controllerName = UrlParameter.Optional });
            routes.MapRoute("ShadowAuthorizeId",
               "ShadowAuthorize/{action}/{Id}",
               new { controller = "ShadowAuthorize", action = "Index", Id = UrlParameter.Optional });
            #endregion

            routes.MapRoute("Maintenance", "Maintenance", new { controller = "Prompt", action = "Maintenance" });
            routes.MapRoute(
                "Default", // 路由名称
                "{controller}/{action}/{id}", // 带有参数的 URL
                new { controller = "Message", action = "Index", id = UrlParameter.Optional } // 参数默认值
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            //RouteDebug.RouteDebugger.RewriteRoutesForTesting(RouteTable.Routes);

            IDictionary<int, Pair<string, Role>> onLineList = new Dictionary<int, Pair<string, Role>>();
            Application.Add(LotterySystem.M_ONLINEUSERCOUNT, onLineList);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorThemeViewEngine());
        }

        protected void Session_End(object sender, EventArgs e)
        {
            Application.Lock();
            var onLineList = Application[LotterySystem.M_ONLINEUSERCOUNT] as IDictionary<int, Pair<string, Role>>;
            var userId = onLineList.Where(it => it.Value.Key == Session.SessionID).SingleOrDefault();
            onLineList.Remove(userId);
            Application.UnLock();
        }
        protected void Application_End(object sender, EventArgs e)
        {
            Application.RemoveAll();
            UpperLimitManager.GetManager().UpdateLimit();
        }
    }
}