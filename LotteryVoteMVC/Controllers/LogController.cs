using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SystemFile = System.IO.File;

namespace LotteryVoteMVC.Controllers
{
    public class LogController : BaseController
    {
        public LogManager LogManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LogManager>();
            }
        }
        public LimitManager LimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LimitManager>();
            }
        }

        [UserAuthorize(UserState.Suspended, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Guest, Role.Shadow, IsNormal = true)]
        public ActionResult Login(int? Id)
        {
            var targetUser = CurrentUser;
            if (Id.HasValue && (!UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser))) PageNotFound();
            var logs = LogManager.GetLoginLog(targetUser, CurrentPage);
            ViewBag.User = targetUser;
            return View(logs);
        }

        [UserAuthorize(UserState.Suspended, IsNormal = true)]
        public ActionResult LoginFailed()
        {
            var logs = LogManager.GetFailedLoginLog(CurrentUser, CurrentPage);
            return View(logs);
        }

        [UserAuthorize(UserState.Suspended, Role.Company, Role.Super, Role.Master, Role.Agent, Role.Shadow, IsNormal = true)]
        public ActionResult Action(int? Id)
        {
            var targetUser = CurrentUser;
            if (Id.HasValue && (!UserManager.IsParent(MatrixUser.UserId, Id.Value, out targetUser)))
                PageNotFound();
            var logs = this.IsSearch() ? LogManager.GetActionLog(targetUser, this.UserName(), this.FromDate(), this.ToDate(), CurrentPage) : LogManager.GetActionLog(targetUser, CurrentPage);
            return View(logs);
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult System()
        {
            var log = LogManager.GetSystemError(CurrentUser, CurrentPage);
            return View(log);
        }
        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult DetailSystem(int? Id)
        {
            if (!Id.HasValue) return RedirectToAction("System");
            var log = LogManager.GetError(CurrentUser, Id.Value);
            return View(log);
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Log()
        {
            string folderPath = Server.MapPath("~/Log");
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            FileInfo[] files = dir.GetFiles();
            return View(files);
        }
        [UserAuthorize(UserState.Active, Role.Manager), HttpPost]
        public ActionResult Log(string FileName)
        {
            if (SystemFile.Exists(FileName))
            {
                System.IO.FileStream fs = new System.IO.FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] buffer = new byte[fs.Length];
                fs.Position = 0;
                fs.Read(buffer, 0, buffer.Length);

                string text = Encoding.Default.GetString(buffer, 0, buffer.Length);

                ViewBag.LogContent = Regex.Replace(Regex.Replace(text, "<", "&lt;"), ">", "&gt;");
            }
            return Log();
        }

        [UserAuthorize(UserState.Suspended, Role.Company, Role.Guest, Role.Shadow)]
        public ActionResult DropWater()
        {
            var limits = this.IsSearch() ? LimitManager.SearchUpperLimit(this.Date(), this.Num(), this.Company(), this.GamePlayWay(), CurrentPage) : GetTodayDropLimit();
            ViewBag.User = MatrixUser;
            return View(limits);
        }
        private PagedList<BetUpperLimit> GetTodayDropLimit()
        {
            var pageSize = LotterySystem.Current.PageSize;
            var limits = UpperLimitManager.GetManager().GetAllLimit().Where(it => it.DropValue > 0).OrderBy(it => it.CompanyId).ThenBy(it => it.GamePlayWayId).ThenBy(it => it.Num);
            return new PagedList<BetUpperLimit>(limits.Skip((CurrentPage - 1) * pageSize).Take(pageSize), CurrentPage, pageSize, limits.Count());
        }
    }
}
