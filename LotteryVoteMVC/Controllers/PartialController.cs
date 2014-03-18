using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class PartialController : BaseController
    {
        LoginCenter _loginCenter;
        public LoginCenter LoginCenter
        {
            get
            {
                if (_loginCenter == null)
                    _loginCenter = new LoginCenter();
                return _loginCenter;
            }
        }

        public MvcHtmlString OnLine()
        {
            string html = (CurrentUser != null && CurrentUser.Role == Role.Company) ?
                string.Format("<span>{0} {1}/{2}</span>", Resource.OnLine, LotterySystem.Current.OnLineUserCount, LotterySystem.Current.MemberCount)
                : string.Empty;
            return MvcHtmlString.Create(html);
        }

        public ActionResult Index()
        {
            return View();
        }

        public PartialViewResult GetMenu()
        {
            if (CurrentUser.Role == Role.Shadow) return ShadowNavMenu();
            return NavMenu();
        }
        public PartialViewResult NavMenu()
        {
            return this.PartialView("NavMenu", CurrentUser);
        }
        public PartialViewResult ShadowNavMenu()
        {
            ViewBag.MatrixUser = MatrixUser;
            return PartialView("ShadowNavMenu", CurrentUser);
        }

        public PartialViewResult TodayCompany()
        {
            return PartialView(TodayLotteryCompany.Instance.GetTodayCompany());
        }

        public PartialViewResult GamePlayWay()
        {
            return this.PartialView(LotterySystem.Current.GamePlayWays);
        }
    }
}
