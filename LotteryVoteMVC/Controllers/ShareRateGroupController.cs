using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Controllers
{
    public class ShareRateGroupController : BaseController
    {
        public ShareRateGroupManager ShareRateGroupManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<ShareRateGroupManager>();
            }
        }
        public GroupLimitManager GroupLimitManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<GroupLimitManager>();
            }
        }


        #region 分成组
        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Index()
        {
            var groups = ShareRateGroupManager.ListGroup();
            return View(groups);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult AddGroup(ShareGroupModel model)
        {
            if (!ModelState.IsValid)
            {
                string error = ModelState.ToErrorString();
                ModelState.Clear();
                ModelState.AddModelError("Error", error);
            }
            if (ModelState.Sum(it => it.Value.Errors.Count) == 0)
            {
                ShareRateGroupManager.AddGroup(model.Name, model.ShareRate);
                ModelState.AddModelError("Success", Resource.Success);
            }
            return RedirectToAction("Index");
        }
        #endregion

        #region 限制
        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Limit(int Id)
        {
            if (Id == 0) PageNotFound();
            if (this.ShareRateGroupManager.GetGroup(Id) == null)
                PageNotFound();
            ViewBag.CompanyTypes = EnumHelper.GetDescription<CompanyType>();
            ViewBag.GameTypes = EnumHelper.GetDescription<GameType>();
            ViewBag.GamePlayWays = LotterySystem.Current.GamePlayWays;
            ViewBag.GameLimits = GroupLimitManager.GetGameLimits(Id);
            var betLimits = GroupLimitManager.GetLimits(Id);
            return View(betLimits);
        }

        [HttpPost]
        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult BetLimit(int Id, IEnumerable<RateGroupBetLimit> model)
        {
            if (Id == 0) PageNotFound();
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                GroupLimitManager.UpdateBetLimit(Id, model);
                //ActionLogger.Log(CurrentUser, CurrentUser, LogResources.UpdateUserBetLimit, LogResources.GetUpdateUserBetLimit(user.UserName));
                result.Message = Resource.Success;
            }
            return Json(result);
        }


        [HttpPost]
        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult GameLimit(int Id, CompanyType companyType, IEnumerable<RateGroupGameBetLimit> model)
        {
            if (Id == 0) PageNotFound();
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
            }
            else
            {
                GroupLimitManager.UpdateGameLimit(Id, companyType, model);
                //ActionLogger.Log(CurrentUser, user, LogResources.UpdateUserGameLimit, LogResources.GetUpdateUserGameLimit(user.UserName));
                result.Message = Resource.Success;
            }
            return Json(result);
        }
        #endregion
    }
}
