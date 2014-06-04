using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Web;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Controllers
{
    public class MemberController : BaseController
    {
        public CommManager CommManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<CommManager>();
            }
        }
        public FreezeFundsManager FreezeManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<FreezeFundsManager>();
            }
        }
        public UserLimitManager UserLimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserLimitManager>();
            }
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult Info()
        {
            //获取用户余额
            var freeze = FreezeManager.GetFreezeFund(CurrentUser);
            ViewBag.BetCredit = CurrentUser.UserInfo.AvailableGivenCredit;
            ViewBag.Outstanding = freeze == null ? 0 : freeze.Amount;
            ViewBag.CompanyTypes = EnumHelper.GetDescription<CompanyType>();
            ViewBag.GameTypes = EnumHelper.GetDescription<GameType>();
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays;
            var commGroups = CommManager.GetCommissionGroupByUser(CurrentUser, LotterySpecies.VietnamLottery);
            var pack = CommManager.GetMemberPackage(CurrentUser, LotterySpecies.VietnamLottery);
            ViewBag.CommGroup = commGroups.Where(it => it.Key.GroupId == pack.GroupId).FirstOrDefault();
            ViewBag.GameLimits = UserLimitManager.GetGameLimits(CurrentUser);
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            string loginUrl = Extended.GetLoginUrl();
            if (!string.Equals(loginUrl, Request.AppRelativeCurrentExecutionFilePath, StringComparison.InvariantCultureIgnoreCase))
                return Redirect(loginUrl);
            return View();
        }
        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            if (LoginCenter.IsUserLogin())
                return RedirectToAction("Logout");
            string message;
            var loginResult = LoginCentre.MemberLogin(UserName, Password, out message);
            if (Request.IsAjaxRequest())
            {
                return Json(new JsonResultModel
                {
                    IsSuccess = loginResult,
                    Model = new { ShowChangePwd = ShowChangePassword(), YourPassword = Password },
                    Message = message
                });
            }
            if (loginResult)
                return Redirect("~/");
            else
                ModelState.AddModelError("", message);
            return View();
        }

        public ActionResult Agent()
        {
#if !DEBUG
            string loginUrl = Extended.GetLoginUrl();
            if (!string.Equals(loginUrl, Request.AppRelativeCurrentExecutionFilePath, StringComparison.InvariantCultureIgnoreCase))
                return Redirect(loginUrl);
#endif
            ViewBag.Lang = CultureHelper.GetCurrentCulture();
            return View();
        }
        [HttpPost]
        public ActionResult Agent(LoginModel model)
        {
            bool retunVal;
            string message = string.Empty;
            if ((retunVal = ModelState.IsValid))
            {
                var verifyCode = Session["VerifyCode"];
                if (verifyCode == null || !verifyCode.Equals(model.VerifyCode.ToLower()))
                {
                    retunVal = false;
                    ModelState.AddModelError("VerifyCode", Resources.Login.Login.VerifyCodeError);
                    message = ModelState.ToErrorString();
                }
                if (retunVal)
                    retunVal = LoginCentre.AgentLogin(model.UserName, model.Password, out message);
            }
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = retunVal,
                    Model = new { ShowChangePwd = ShowChangePassword(), YourPassword = model.Password },
                    Message = message
                });
            if (retunVal) return Redirect("~/");
            else return View(model);
        }
        public ActionResult Logout()
        {
            LoginCenter login = new LoginCenter();
            login.Logout();
            return Redirect("~/");
        }

        public VerifyCodeResult Captcha()
        {
            return new VerifyCodeResult();
        }

        /// <summary>
        ///是否显示修改密码
        /// </summary>
        /// <returns></returns>
        private bool ShowChangePassword()
        {
            if (CurrentUser == null) return false;
            return !CurrentUser.UserInfo.LastChangePwd.HasValue ||
                CurrentUser.UserInfo.LastChangePwd.Value.AddMonths(LotterySystem.Current.ChangePasswordCycle) < DateTime.Now;
        }
    }
}
