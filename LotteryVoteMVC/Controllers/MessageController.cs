using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;

namespace LotteryVoteMVC.Controllers
{
    public class MessageController : BaseController
    {
        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Guest, Role.Shadow, IsNormal = true)]
        public ActionResult Index()
        {
            var bulltins = BulletinManager.GetManager().GetBulletins(CurrentPage);
            ViewBag.User = CurrentUser;
            return View(bulltins);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Guest, Role.Shadow), HttpPost]
        public ActionResult Index(string Content)
        {
            var bulltins = BulletinManager.GetManager().GetBulletins(Content);
            ViewBag.User = CurrentUser;
            return View(bulltins);
        }

        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult Add(string Content)
        {
            if (!string.IsNullOrEmpty(Content))
                BulletinManager.GetManager().Add(Content);

            return RedirectToAction("");
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Remove(string Id)
        {
            if (!string.IsNullOrEmpty(Id))
                BulletinManager.GetManager().Remove(Id);
            return RedirectToAction("");
        }
    }
}
