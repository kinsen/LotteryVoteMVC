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
    public class ReportController : BaseController
    {
        public OrderManager OrderManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<OrderManager>();
            }
        }
        public SettleManager SettleManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<SettleManager>();
            }
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult Index(int? Id)
        {
            User targetUser;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                targetUser = MatrixUser;
            var fromDate = GetReportDate(this.FromDate());
            var toDate = GetReportDate(this.ToDate());
            var settleResult = SettleManager.GetSettleResult(targetUser, fromDate, toDate);
            ViewBag.User = targetUser;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            return View(settleResult);
        }
        private DateTime GetReportDate(DateTime date)
        {
            if (this.MatrixUser.Role == Role.Company) return date;
            if (date < DateTime.Today.AddDays(DateTime.Today.Day * -1 + 1).AddMonths(-1))
                return DateTime.Today;
            return date;
        }

        /// <summary>
        /// 获取会员的输赢报表
        /// </summary>
        /// <param name="Id">The id.</param>
        /// <returns></returns>
        [AgentAuthorize(UserState.Active)]
        public ActionResult MemberWinLost(int? Id)
        {
            User targetUser;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                targetUser = MatrixUser;
            var fromDate = GetReportDate(this.FromDate());
            var toDate = GetReportDate(this.ToDate());
            var wl = SettleManager.ListMemberWinLost(targetUser, fromDate, toDate);
            ViewBag.CurrentUser = MatrixUser;
            ViewBag.User = targetUser;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            return View(wl);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult ShareRateWL(int? Id)
        {
            User targetUser;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                targetUser = MatrixUser;
            var fromDate = GetReportDate(this.FromDate());
            var toDate = GetReportDate(this.ToDate());
            var wl = SettleManager.ListShareRateWL(targetUser, fromDate, toDate);
            ViewBag.User = targetUser;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            return View(wl);
        }


        [AgentAuthorize(UserState.Active)]
        public ActionResult WinLost(int? Id)
        {
            User targetUser = MatrixUser;
            if (Id.HasValue && (!UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser))) PageNotFound();
            var orders = OrderManager.GetUserSettleOrder(targetUser, this.Num(), this.Company(), this.GamePlayWay(), this.FromDate(), this.ToDate(), this.WinLost(), CurrentPage);
            return View(orders);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult FullStatements(int? Id)
        {
            User targetUser;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                targetUser = MatrixUser;
            var fromDate = this.FromDate();
            var toDate = this.ToDate();

            var userFullStatement = SettleManager.GetUserStatement(targetUser, fromDate, toDate);

            var settleResult = SettleManager.GetFullStatement(targetUser, fromDate, toDate);
            ViewBag.User = targetUser;
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            ViewBag.FullStatement = userFullStatement;
            return View(settleResult);
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult GetChildStates(int? Id)
        {
            User targetUser;
            if (!(Id.HasValue && UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                targetUser = MatrixUser;
            bool hasChild = targetUser.Role < Role.Agent;
            var fromDate = this.FromDate();
            var toDate = this.ToDate();
            var settleResult = SettleManager.GetFullStatement(targetUser, fromDate, toDate)
            .Select(it => new
            {
                UserId = it.UserId,
                UserName = it.UserName,
                OrderCount = it.OrderCount,
                BetTurnover = it.BetTurnover.ToString("N"),
                WinLost = it.WinLost.ToString("N"),
                ParentCommission = it.ParentCommission.ToString("N"),
                TotalCommission = it.TotalCommission.ToString("N"),
                HasChild = hasChild
            });
            return Json(settleResult, JsonRequestBehavior.AllowGet);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Statement()
        {
            BetOrder lastWeekTotal;
            var statement = OrderManager.GetStatement(CurrentUser, out lastWeekTotal);
            ViewBag.LastWeekTotal = lastWeekTotal;
            return View(statement);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Shadow)]
        public ActionResult Member()
        {
            var fromDate = this.FromDate();
            var toDate = this.ToDate();
            var source = SettleManager.GetMemberSettleResult(this.MatrixUser, fromDate, toDate, this.Sort(), this.SortType(), this.CurrentPage);
            ViewBag.From = fromDate;
            ViewBag.To = toDate;
            return View(source);
        }
        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult MemberWL()
        {
            var orders = OrderManager.GetMemberWinLost(CurrentUser, this.Num(), this.Company(), this.GamePlayWay(), this.WinLost(), this.Date(), this.Date(), CurrentPage);
            return View(orders);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Shadow, Role.Guest)]
        public ActionResult Cancel()
        {
            var orders = OrderManager.GetCancelOrders(MatrixUser, this.Date(), this.Num(), this.Company(), this.GamePlayWay(), CurrentPage);
            return View(orders);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult NumAmountRanking()
        {
            ViewBag.GameType = GameType.TwoDigital;
            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany();
            var result = OrderManager.GetTodayMaxAmountNum(MatrixUser, CurrentPage, GameType.TwoDigital);
            return View(result);
        }
        [AgentAuthorize(UserState.Active)]
        public ActionResult ThreeDNumAmountRanking()
        {
            ViewBag.GameType = GameType.ThreeDigital;
            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany();
            var result = OrderManager.GetTodayMaxAmountNum(MatrixUser, CurrentPage, GameType.ThreeDigital);
            return View("NumAmountRanking", result);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult NumAmountRankingDetails(int CompanyId, int GPWId, string Num)
        {
            var result = OrderManager.GetNumAmountRanking(CompanyId, GPWId, Num, CurrentPage);
            return View("NumAmountRankingDetails", result);
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult EachLevel(int? RoleId, int? UserId)
        {
            User targetUser = MatrixUser;
            Role targetRole = MatrixUser.Role + 1;
            if (UserId.HasValue)
                if (!UserManager.IsParent(MatrixUser.UserId, UserId.Value, out targetUser)) PageNotFound();
            if (RoleId.HasValue)
                targetRole = (Role)RoleId;
            var orders = OrderManager.GetTodayTotalChildReport(targetUser, targetRole);
            ViewBag.User = targetUser;
            return View(orders);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult CompanyAmountRanking()
        {
            if (this.IsSearch())
            {
                var ranks = OrderManager.GetCompanyRanking(this.Company(), this.FromDate(), this.ToDate());
                return View(ranks);
            }
            return View();
        }
    }
}
