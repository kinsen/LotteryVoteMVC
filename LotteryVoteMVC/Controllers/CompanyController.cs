using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class CompanyController : BaseController
    {
        CompanyManager CompanyManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<CompanyManager>();
            }
        }

        [UserAuthorize(UserState.Active, IsNormal = true)]
        public ActionResult Index()
        {
            var companys = LotterySystem.Current.AllCompanyList;
            ViewBag.CurrentUser = CurrentUser;
            return View(companys);
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Add()
        {
            return View("Edit", new LotteryCompany());
        }
        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult Add(LotteryCompany model)
        {
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    JsonResultModel result = new JsonResultModel
                    {
                        IsSuccess = false,
                        Message = ModelState.ToErrorString()
                    };
                    return Json(result);
                }
                else
                {
                    return View(model);
                }
            }
            CompanyManager.Add(model, CurrentUser);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.AddLotteryCompany, LogResources.GetAddLotteryCompany(model.Name, model.CompanyId));
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success,
                    Model = model
                };
                return Json(result);
            }
            else
                return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Edit(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var company = CompanyManager.GetCompany(Id.Value);
            return View(company);
        }
        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult Edit(int? Id, LotteryCompany model)
        {
            if (!Id.HasValue) PageNotFound();
            if (!ModelState.IsValid)
            {
                if (Request.IsAjaxRequest())
                {
                    JsonResultModel result = new JsonResultModel
                    {
                        IsSuccess = false,
                        Message = ModelState.ToErrorString()
                    };
                    return Json(result);
                }
                else
                    return View(model);
            }
            var company = CompanyManager.GetCompany(Id.Value);
            UpdateModel(company);
            CompanyManager.Update(company, CurrentUser);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.UpdateLotteryCompany, LogResources.GetUpdateLotteryCompany(company.Name, company.CompanyId));
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success,
                    Model = company
                };
                return Json(result);
            }
            else
                return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult EditCycle(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var company = CompanyManager.GetCompany(Id.Value);
            ViewBag.Company = company;
            return View(CompanyManager.GetCycles(company).Select(it => it.DayOfWeek));
        }
        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult EditCycle(int? Id, IEnumerable<DayOfWeek> model)
        {
            if (!Id.HasValue) PageNotFound();
            model = model ?? Enumerable.Empty<DayOfWeek>();
            CompanyManager.UpdateCycles(Id.Value, model);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.UpdateLotteryCompanyCycle, LogResources.GetUpdateLotteryCompanyCycle(Id.Value, model));
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                };
                return Json(result);
            }
            else
                return SuccessAction();
        }

        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult UpdateLotteryCompany()
        {
            TodayLotteryCompany.Instance.UpdateTodayLotteryCompany();
            LotterySystem.ClearAllCompany();
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                };
                return Json(result);
            }
            else
                return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult Delete(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            CompanyManager.Remove(Id.Value, CurrentUser);
            ActionLogger.Log(CurrentUser, CurrentUser, LogResources.RemoveLotteryCompany, LogResources.GetRemoveLotteryCompany(Id.Value));
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel();
                result.IsSuccess = true;
                result.Model = Id.Value;
                return Json(result);
            }
            else
                return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, IsNormal = true)]
        public ActionResult LotteryResult()
        {
            var lotteryResult = CompanyManager.GetLotteryResultByDate(this.Date());
            ViewBag.CurrentUser = CurrentUser;
            return View(lotteryResult);
        }

        [UserAuthorize(UserState.Active, Role.Company)]
        public ActionResult AddLotteryResult(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var company = TodayLotteryCompany.Instance.GetTodayCompany().Find(it => it.CompanyId == Id.Value);
            if (company == null) PageNotFound();
            var lens = CompanyManager.GetNumLengthByCompany(company);
            return View(lens);
        }

        [UserAuthorize(UserState.Active, Role.Company), HttpPost]
        public ActionResult AddLotteryResult(int? Id, IEnumerable<LotteryRecord> model)
        {
            if (!Id.HasValue) PageNotFound();
            if (!ModelState.IsValid)
            {
                return Json(new JsonResultModel { IsSuccess = false, Message = ModelState.ToErrorString() });
            }
            CompanyManager.AddLotteryResult(Id.Value, model);
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resources.Resource.Success
                };
                return Json(result);
            }
            else
                return RedirectToAction("AddLotteryResult", new { Id = Id.Value });
        }
    }
}
