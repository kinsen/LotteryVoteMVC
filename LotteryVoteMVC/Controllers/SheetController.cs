using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class SheetController : BaseController
    {
        public OrderManager OrderManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<OrderManager>();
            }
        }
        public BetManager BetManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<BetManager>();
            }
        }
        public LogManager LogManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LogManager>();
            }
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult Index(int? Id)
        {
            var targetUser = MatrixUser;
            if (Id.HasValue)
                if (!UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser))
                    PageNotFound();
            var sheets = this.IsSearch() ? OrderManager.SearchUserSheets(MatrixUser, this.UserName(), BetStatus.Valid, this.Num(), CurrentPage) :
                OrderManager.GetTodaySheets(targetUser, BetStatus.Valid, string.Empty, CurrentPage);
            ViewBag.User = MatrixUser;
            return View(sheets);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Bet()
        {
            var sheets = OrderManager.GetTodayUnSettleSheets(CurrentUser, CurrentPage);
            ViewBag.CurrentUser = CurrentUser;
            return View(sheets);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Guest, Role.Shadow)]
        public ActionResult Cancel(int? Id)
        {
            if (!Id.HasValue) PageNotFound();

            BetManager.CancleSheet(Id.Value, MatrixUser);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.CancelSheet, string.Format("SheetId : {0}", Id.Value));
            return Redirect(Request.UrlReferrer.ToString());
        }
    }
}
