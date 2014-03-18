using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Controllers
{
    public class LimitController : BaseController
    {
        public UserLimitManager UserLimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserLimitManager>();
            }
        }
        public LimitManager LimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LimitManager>();
            }
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Super, Role.Master, Role.Agent)]
        public ActionResult Index(int Id = 0)
        {
            if (Id == 0) PageNotFound();
            User user = null;
            if (!UserManager.IsParent(MatrixUser.UserId, Id, out user))
                PageNotFound();
            ViewBag.CompanyTypes = EnumHelper.GetDescription<CompanyType>();
            ViewBag.GameTypes = EnumHelper.GetDescription<GameType>();
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays;
            ViewBag.GameLimits = UserLimitManager.GetGameLimits(user);
            ViewBag.Role = user.Role;
            var betLimits = UserLimitManager.GetLimits(user);
            return View(betLimits);
        }

        #region 上限监控
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult UpperMonitor()
        {
            var limitData = UpperLimitManager.GetManager().GetCompanyLimitData();
            return View(limitData);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult UpperMonitorDetail(int? CompanyId, int? GamePlayWayId)
        {
            if (!CompanyId.HasValue) PageNotFound();
            if (!GamePlayWayId.HasValue) PageNotFound();
            int pageSize = LotterySystem.Current.PageSize;
            var skip = (CurrentPage - 1) * pageSize;
            var limits = UpperLimitManager.GetManager().GetLimitContext(CompanyId.Value, GamePlayWayId.Value).Values
                .OrderByDescending(it => it.TotalBetAmount);
            var result = new PagedList<BetUpperLimit>(limits.Skip(skip).Take(pageSize), CurrentPage, pageSize, limits.Count());
            return View(result);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult StopUpperLimits()
        {
            Spec<BetUpperLimit> condition = it => it.StopBet;
            if (this.IsSearch())
            {
                condition = condition.And(it => it.GamePlayWayId == this.GamePlayWay() && it.CompanyId == this.Company());
                if (!string.IsNullOrEmpty(this.Num()))
                    condition = condition.And(it => it.Num == this.Num());
            }
            var limits = UpperLimitManager.GetManager().GetAllLimit().Where(it => condition(it));
            return View(limits);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult AddStop(string Num, int Company, int GamePlayWay)
        {
            if (!Extended.NumIsCorrectGameTypeFormat(Num, GamePlayWay))
                return ErrorAction(Resource.PleaseSelectedCorrectGameType);
            StopAcceptBet(Num, Company, GamePlayWay);
            return RedirectToAction("StopUpperLimits");
        }
        /// <summary>
        /// 停止接受下注
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        private void StopAcceptBet(string num, int companyId, int gameplaywayId)
        {
            if (num.IsRangeNum() || num.IsBatterNum() || num.IsNumArray() || num.IsStartBatterNum())
            {
                foreach (var n in Extended.GetRangeNum(num))
                    StopAcceptBet(n, companyId, gameplaywayId);
                return;
            }
            var limit = UpperLimitManager.GetManager().GetLimit(num, companyId, gameplaywayId);
            UpperLimitManager.GetManager().UpdateAcceptBet(limit.LimitId, true);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult UpdateUpperLimit(int? limitId, bool? acceptBet)
        {
            if (!(limitId.HasValue && acceptBet.HasValue)) PageNotFound();
            UpperLimitManager.GetManager().UpdateAcceptBet(limitId.Value, acceptBet.Value);
            return Redirect(Request.UrlReferrer.ToString());
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult EditUpperLimit(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var limit = UpperLimitManager.GetManager().GetLimit(Id.Value);
            if (limit == null) PageNotFound();
            return View(limit);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult EditUpperLimit(int? Id, decimal UpperLlimit)
        {
            if (!Id.HasValue) PageNotFound();
            var limit = UpperLimitManager.GetManager().GetLimit(Id.Value);
            if (limit == null) PageNotFound();
            UpperLimitManager.GetManager().UpdateLimit(limit, UpperLlimit);
            if (Request.IsAjaxRequest())
            {
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success,
                    Model = limit
                });
            }
            else
                return SuccessAction();
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult StopAcceptBet(int CompanyId, int GamePlayWayId)
        {
            bool isStopAccept = Extended.IsStopAcceptBet(CompanyId, GamePlayWayId);
            if (isStopAccept)
                StopAcceptBetSettings.Instance.StartAcceptBet(CompanyId, GamePlayWayId);
            else
                StopAcceptBetSettings.Instance.StopAcceptBet(CompanyId, GamePlayWayId);
            return RedirectToAction("UpperMonitor");
        }
        #endregion

        [HttpPost]
        [AgentAuthorize(UserState.Active)]
        public ActionResult BetLimit(int Id, IEnumerable<BetLimit> model)
        {
            if (Id == 0) PageNotFound();
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                User user = UserManager.GetUser(Id);
                if (!EditCompanySelf(user) && (user == null || user.ParentId != MatrixUser.UserId))     //只有父级用户才能修改下级的佣金
                    throw new NoPermissionException("Edit BetLimit");
                UserLimitManager.UpdateBetLimit(user, model);
                ActionLogger.Log(CurrentUser, user, LogResources.UpdateUserBetLimit, LogResources.GetUpdateUserBetLimit(user.UserName));
                result.Message = Resource.Success;
            }
            return Json(result);
        }

        [HttpPost]
        [AgentAuthorize(UserState.Active)]
        public ActionResult GameLimit(int Id, CompanyType companyType, IEnumerable<GameBetLimit> model)
        {
            if (Id == 0) PageNotFound();
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                User user = UserManager.GetUser(Id);
                if (!EditCompanySelf(user) && (user == null || user.ParentId != MatrixUser.UserId))     //只有父级用户才能修改下级的佣金
                    throw new NoPermissionException("更改用户游戏限制");
                UserLimitManager.UpdateGameLimit(user, companyType, model);
                ActionLogger.Log(CurrentUser, user, LogResources.UpdateUserGameLimit, LogResources.GetUpdateUserGameLimit(user.UserName));
                result.Message = Resource.Success;
            }
            return Json(result);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult DefaultUpper()
        {
            var limits = LimitManager.GetDefaultUpperLimits();
            return View("DefaultUpper", limits);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult AddDefault(DefaultUpperLimitModel model)
        {
            if (model.CompanyType == default(CompanyType))
                ModelState.AddModelError("CompanyType", string.Format(ModelResource.PleaseSelected, Resource.CompanyType));
            if (model.GamePlayWay == 0)
                ModelState.AddModelError("GamePlayWay", string.Format(ModelResource.PleaseSelected, Resource.GameType));
            if (model.Amount == 0)
                ModelState.AddModelError("Amount", string.Format(ModelResource.MustGreatThan, Resource.Amount, 0));
            if (ModelState.Sum(it => it.Value.Errors.Count) == 0)
            {
                LimitManager.AddDefaultUpperLimit(model.CompanyType, model.GamePlayWay, model.Amount);
                return SuccessAction();
            }
            else
                return DefaultUpper();
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult RemoveDefault(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            LimitManager.RemoveDefaultUpperLimit(Id.Value);
            return SuccessAction();
        }


        /// <summary>
        /// 是否编辑公司自身信息
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        private bool EditCompanySelf(User user)
        {
            return user.UserId == CurrentUser.UserId && user.Role == Role.Company;
        }
    }
}
