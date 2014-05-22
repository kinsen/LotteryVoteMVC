using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Configuration;

namespace LotteryVoteMVC.Core.Web
{
    public class ThemeControllerBase : Controller
    {
        public string Theme { get; private set; }
        protected override void Execute(System.Web.Routing.RequestContext requestContext)
        {
            var themeName = ConfigurationManager.AppSettings["ThemeName"];
            var defaultTheme = ConfigurationManager.AppSettings["DefaultTheme"];

            if (requestContext.HttpContext.Items[themeName] == null)
            {
                //first time load
                var cookie = requestContext.HttpContext.Request.Cookies.Get("theme");
                var theme = "Default";
                if (cookie != null)
                    theme = cookie.Value;
                requestContext.HttpContext.Items[themeName] = theme; //requestContext.HttpContext.Request.Cookies.Get("theme").Value;
            }
            else
            {
                //requestContext.HttpContext.Items[themeName] = defaultTheme;

                var previewTheme = requestContext.RouteData.Values.ContainsKey("theme") ? requestContext.RouteData.Values["theme"].ToString() : string.Empty;
                if (!string.IsNullOrEmpty(previewTheme))
                {
                    requestContext.HttpContext.Items[themeName] = previewTheme;
                }
            }

            Theme = requestContext.HttpContext.Items[themeName].ToString();
            base.Execute(requestContext);
        }
    }
}
