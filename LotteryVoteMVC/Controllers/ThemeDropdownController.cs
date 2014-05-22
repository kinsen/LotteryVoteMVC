using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace LotteryVoteMVC.Controllers
{
    public class ThemeDropdownController : Controller
    {
        [HttpPost]
        public ActionResult Index(string theme)
        {
            var themeName = ConfigurationManager.AppSettings["themeName"];

            ControllerContext.RequestContext.HttpContext.Items[themeName] = theme;
            var themeCookie = new HttpCookie("theme", theme);
            HttpContext.Response.Cookies.Add(themeCookie);

            const string controller = "Home";
            const string action = "Index";
            if (Request.IsAjaxRequest())
                return Json(true);
            return Redirect(string.Format("~/{0}/{1}", controller, action));
        }
    }
}