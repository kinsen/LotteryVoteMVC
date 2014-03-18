using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class PromptController : Controller
    {
        //
        // GET: /Prompt/

        public ActionResult Index()
        {
            ViewBag.Title = Resource.Success;
            ViewBag.Url = HttpUtility.UrlDecode(Request["Url"]);
            ViewBag.Message = HttpUtility.UrlDecode(Request["Msg"]);
            return View();
        }

        public ActionResult Error()
        {
            ViewBag.Title = Resource.Error;
            ViewBag.Url = HttpUtility.UrlDecode(Request["Url"]);
            ViewBag.Message = MvcHtmlString.Create(HttpUtility.UrlDecode(Request["Msg"]));
            return View("Index");
        }

        public ActionResult Maintenance()
        {
            return View();
        }
    }
}
