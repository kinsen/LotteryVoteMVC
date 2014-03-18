using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class OrderController : BaseController
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

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Index(int? Id)
        {
            PagedList<BetOrder> orders;
            if (this.IsSearch())
                orders = OrderManager.SearchMemberTodayOrderByCondition(CurrentUser, Id, this.BetStatus(), this.Num(), this.Company(), this.GamePlayWay(), this.Sort(), this.SortType(), CurrentPage);
            else
                orders = OrderManager.GetMemberBetOrder(CurrentUser, Id, CurrentPage);

            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany();
            return View(orders);
        }

        [UserAuthorize(UserState.Active, Role.Guest, Role.Company, Role.Shadow)]
        public ActionResult Cancel(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            BetManager.CancleOrder(Id.Value, MatrixUser);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.CancelOrder, string.Format("OrderId : {0}", Id.Value));
            return Redirect(Request.UrlReferrer.ToString());
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult All()
        {
            var orders = OrderManager.GetUserBetOrder(MatrixUser, this.Num(), this.Company(), this.GamePlayWay(), this.CurrentPage);
            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany();
            ViewBag.User = MatrixUser;
            return View("Manager", orders);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Guest, Role.Shadow)]
        public ActionResult Win(int? Id)
        {
            User targetUser = MatrixUser; ;
            if (Id.HasValue && !UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser))
                targetUser = MatrixUser;
            var orders = OrderManager.GetWinOrders(targetUser, this.Num(), this.Company(), this.GamePlayWay(), this.FromDate(), this.ToDate(), CurrentPage);
            return View(orders);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult WinReport()
        {
            var orders = OrderManager.GetWinOrders(MatrixUser, this.Num(), this.Company(), this.GamePlayWay(), this.FromDate(), this.ToDate(), this.Sort(), this.SortType("desc"), CurrentPage);
            return View(orders);
        }

        /// <summary>
        /// 各级代理查看Sheet的order
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns></returns>
        [AgentAuthorize(UserState.Active)]
        public ActionResult Sheet(int? Id)
        {
            if (!Id.HasValue) PageNotFound();

            var orders =
                this.IsSearch() ?
                OrderManager.SearchUserTodayOrderByCondition(MatrixUser, Id, this.BetStatus(), this.Num(), this.Company(), this.GamePlayWay(), this.Sort(), this.SortType(), this.CurrentPage)
                :
                OrderManager.GetUserBetOrder(MatrixUser, Id.Value, CurrentPage);
            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany();
            ViewBag.User = MatrixUser;
            return View("Manager", orders);
        }
    }
}
