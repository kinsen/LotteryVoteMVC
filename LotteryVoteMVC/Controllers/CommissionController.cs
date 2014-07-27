using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Controllers
{
    public class CommissionController : BaseController
    {
        CommManager _commManager;
        public CommManager CommManager
        {
            get
            {
                if (_commManager == null)
                    _commManager = new CommManager();
                return _commManager;
            }
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult Index()
        {
            var comms = CommManager.GetCommissionGroupByUser(MatrixUser, LotterySpecies.VietnamLottery);
            ViewBag.User = MatrixUser;
            return View(comms);
        }

        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult EachLevelComm()
        {
            var comms = CommManager.GetParentsCommission(CurrentUser, LotterySpecies.VietnamLottery);
            ViewBag.Comm = comms;
            return View();
        }

        [AgentAuthorize(UserState.Active)]
        public ActionResult EditUserComm(int Id = 0, LotterySpecies specie = LotterySpecies.VietnamLottery)
        {
            if (Id == 0) PageNotFound();
            User user = null;
            if (!UserManager.IsParent(MatrixUser.UserId, Id, out user))
                PageNotFound();
            ViewBag.CompanyTypes = EnumHelper.GetDescription<CompanyType>();
            ViewBag.GameTypes = EnumHelper.GetDescription<GameType>();
            ViewBag.CommGroups = CommManager.GetCommissionGroupByUser(user, specie);
            ViewBag.Role = user.Role;
            ViewBag.User = MatrixUser;
            var commValues = CommManager.GetCommissionValue(user, specie);
            return View(commValues);
        }

        [AgentAuthorize(UserState.Active), HttpPost]
        public ActionResult EditUserComm(IEnumerable<CommissionValue> model, int Id = 0, LotterySpecies specie = LotterySpecies.VietnamLottery)
        {
            if (Id == 0) PageNotFound();
            User user = UserManager.GetUser(Id);
            if ((user == null || user.ParentId != MatrixUser.UserId) && MatrixUser.Role != Role.Company)     //只有父级用户才能修改下级的佣金
                PageNotFound();
            ViewBag.CompanyTypes = EnumHelper.GetDescription<CompanyType>();
            ViewBag.GameTypes = EnumHelper.GetDescription<GameType>();
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest()) throw new BusinessException(ModelState.ToErrorString());
                return View(model);
            }
            else
            {
                CommManager.UpdateUserCommission(user, specie, model);
                ActionLogger.Log(CurrentUser, user, LogResources.UpdateUserComm, LogResources.GetUpdateUserComm(specie.ToString()));
                if (Request.IsAjaxRequest())
                {
                    return Json(new JsonResultModel
                    {
                        IsSuccess = true,
                        Message = Resource.Success,
                        Model = model
                    });
                }
                ViewBag.CommGroups = CommManager.GetCommissionGroupByUser(user, specie);
                return View(model);
            }
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult AddGroup()
        {
            return View();
        }

        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult AddGroup(CommGroupModel model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    return Json(new JsonResultModel
                    {
                        IsSuccess = false,
                        Message = ModelState.ToErrorString()
                    });
                else
                    return View(model);
            }
            CommManager.AddCommGroup(model.Specie, model.GroupName, model.Comms);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.AddCommGroup, LogResources.GetAddCommGroup(model.Specie.ToString(), model.GroupName));
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                });
            else
                return SuccessAction();
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult EditGroup(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var groupInfo = CommManager.GetGroupComms(Id.Value);
            if (groupInfo == null) PageNotFound();
            var model = new CommGroupModel
            {
                GroupName = groupInfo.Key.GroupName,
                Specie = groupInfo.Key.Specie,
                Comms = groupInfo.Value
            };
            return View(model);
        }

        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult EditGroup(int? Id, CommGroupModel model)
        {
            ModelState.Remove("GroupName");
            ModelState.Remove("Specie");
            if (!Id.HasValue) PageNotFound();
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                    return Json(new JsonResultModel
                    {
                        IsSuccess = false,
                        Message = ModelState.ToErrorString()
                    });
                else
                    return View(model);
            }
            CommManager.UpdateCommGroup(Id.Value, model.Comms);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.UpdateCommGroup, LogResources.GetUpdateCommGroup(Id.Value));
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                });
            else
                return View();
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult DelGroup(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            CommManager.RemoveCommGroup(Id.Value);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.RemoveCommGroup, LogResources.GetRemoveCommGroup(Id.Value));
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                });
            else
                return RedirectToAction("Index");
        }


        [UserAuthorize(UserState.Active, Role.Guest)]
        public ActionResult GetCommission(int companyId, GameType gameType)
        {
            var company = TodayLotteryCompany.Instance.GetTodayCompany().Find(it => it.CompanyId == companyId);
            var comms = CommManager.GetMemberCommissionInSession(CurrentUser, LotterySpecies.VietnamLottery);
            var comm = comms.Value.Find(it => it.CompanyType == company.CompanyType && it.GameType == gameType);
            return Json(comm.Commission / 100, JsonRequestBehavior.AllowGet);
        }
    }
}
