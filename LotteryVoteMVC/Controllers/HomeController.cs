using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Controllers
{
    public class HomeController : BaseController
    {
        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Index()
        {
            ViewBag.Message = "欢迎使用 ASP.NET MVC!";

            return View();
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult About()
        {
            return View();
        }
    }
}
