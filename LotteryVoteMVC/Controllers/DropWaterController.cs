using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Controllers
{
    public class DropWaterController : BaseController
    {
        public DropWaterManager DropManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<DropWaterManager>();
            }
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult Index()
        {
            var drops = this.IsSearch() ? DropManager.GetAutoDropByCondition(this.Num(), this.Company(), this.GamePlayWay(), this.DropWater(), this.Amount(), CurrentPage) :
                DropManager.GetAutoDrops(CurrentPage);
            return View(drops);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult Today()
        {
            var drops = this.IsSearch() ? DropManager.GetManualDropByCondition(DateTime.Today, this.Num(), this.Company(), this.GamePlayWay(), this.DropWater(), this.Amount(), CurrentPage) :
                DropManager.GetManualDrops(DateTime.Today, CurrentPage);
            ViewBag.Companys = TodayLotteryCompany.Instance.GetTodayCompany().ToMultiSelectList(it => it.Name, it => it.CompanyId.ToString());
            return View(drops);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult Tomorrow()
        {
            var date = DateTime.Today.AddDays(1);
            var drops = this.IsSearch() ? DropManager.GetManualDropByCondition(date, this.Num(), this.Company(), this.GamePlayWay(), this.DropWater(), this.Amount(), CurrentPage) :
                DropManager.GetManualDrops(date, CurrentPage);
            var comManager = ManagerHelper.Instance.GetManager<CompanyManager>();
            var coms = comManager.GetLotteryCompanyByDate(date);
            ViewBag.Companys = coms.ToMultiSelectList(it => it.Name, it => it.CompanyId.ToString());
            return View(drops);
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult BetAuto()
        {
            var drops = this.IsSearch() ?
                DropManager.SearchAutoBetDrop(this.CompanyType(), this.GamePlayWay(), this.Amount(), this.DropValue(), CurrentPage)
                : DropManager.GetAutoBetDrop(CurrentPage);
            return View("BetAuto", drops);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult AddBetAuto(BetAutoDropModel model)
        {
            if (!ModelState.IsValid)
            {
                string error = ModelState.ToErrorString();
                ModelState.Clear();
                ModelState.AddModelError("Error", error);
            }
            if (model.CompanyType == default(CompanyType))
                ModelState.AddModelError("CompanyType", string.Format(ModelResource.PleaseSelected, Resource.CompanyType));
            if (model.GamePlayWay == 0)
                ModelState.AddModelError("GamePlayWay", string.Format(ModelResource.PleaseSelected, Resource.GameType));
            if (model.Amount == 0)
                ModelState.AddModelError("Amount", string.Format(ModelResource.MustGreatThan, Resource.Amount, 0));
            if (model.DropWater == 0)
                ModelState.AddModelError("DropWater", string.Format(ModelResource.MustGreatThan, Resource.DropWater, 0));
            if (ModelState.Sum(it => it.Value.Errors.Count) == 0)
            {
                DropManager.AddBetAutoDrop(model.CompanyType, model.GamePlayWay, model.Amount, model.DropWater);
                ModelState.AddModelError("Success", Resource.Success);
                return RedirectToAction("BetAuto");
            }
            else
                return BetAuto();
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public string GetDropByLimit(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            var limit = UpperLimitManager.GetManager().GetLimit(Id.Value);
            if (limit == null) PageNotFound();
            var dw = DropManager.SearchTodayDropWater(limit.Num, limit.CompanyId, limit.GamePlayWayId, limit.NextLimit);
            return dw == null ? "0" : dw.DropValue.ToString();
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult AddTodayDrop(int? Id)
        {
            BetUpperLimit limit = null;
            TodayDropWaterModel model = new TodayDropWaterModel();
            if (Id.HasValue)
                limit = UpperLimitManager.GetManager().GetLimit(Id.Value);
            if (limit != null)
            {
                model.Num = limit.Num;
                model.GamePlayWay = limit.GamePlayWayId;
                model.Amount = limit.TotalBetAmount + LotterySystem.Current.QuickAddDropAmount;
                model.Companys = new[] { limit.CompanyId };
            }
            var companys = TodayLotteryCompany.Instance.GetTodayCompany().Select(it => new SelectListItem
            {
                Text = it.Name,
                Value = it.CompanyId.ToString()
            });
            ViewBag.Companys = new MultiSelectList(companys, "Value", "Text", new[] { limit.CompanyId.ToString() });
            return View(model);
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult AddTodayDrop(TodayDropWaterModel model)
        {
            if (!ModelState.IsValid)
                return ErrorAction(ModelState.ToErrorString());
            if (model.GamePlayWay == 0)
                ModelState.AddModelError("GamePlayWay", string.Format(ModelResource.PleaseSelected, Resource.GameType));
            else if (!Extended.NumIsCorrectGameTypeFormat(model.Num, model.GamePlayWay))
                ModelState.AddModelError("GamePlayWay", string.Format(Resource.PleaseSelectedCorrectGameType));
            if (ModelState.Sum(it => it.Value.Errors.Count) > 0)
                return ErrorAction(ModelState.ToErrorString());
            DropManager.AddTodayDrop(model.Num, model.GamePlayWay, model.DropWater, model.Amount, model.Companys);
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success,
                    Model = model
                });
            else
                return RedirectToAction("Today");
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult AddTomorrowDrop(TodayDropWaterModel model)
        {
            if (!ModelState.IsValid)
                return ErrorAction(ModelState.ToErrorString());
            if (model.GamePlayWay == 0)
                ModelState.AddModelError("GamePlayWay", string.Format(ModelResource.PleaseSelected, Resource.GameType));
            else if (!Extended.NumIsCorrectGameTypeFormat(model.Num, model.GamePlayWay))
                ModelState.AddModelError("GamePlayWay", string.Format(Resource.PleaseSelectedCorrectGameType));
            if (ModelState.Sum(it => it.Value.Errors.Count) > 0)
                return ErrorAction(ModelState.ToErrorString());
            DropManager.AddManualDrop(DateTime.Today.AddDays(1), model.Num, model.GamePlayWay, model.DropWater, model.Amount, model.Companys);
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success,
                    Model = model
                });
            else
                return RedirectToAction("Tomorrow");
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult AddAutoDrop(AutoDropWaterModel model)
        {
            if (!ModelState.IsValid)
                return ErrorAction(ModelState.ToErrorString());
            if (model.GamePlayWay == 0)
                ModelState.AddModelError("GamePlayWay", string.Format(ModelResource.PleaseSelected, Resource.GameType));
            if (ModelState.Sum(it => it.Value.Errors.Count) > 0)
                return ErrorAction(ModelState.ToErrorString());
            DropManager.AddAutoDrop(model.GamePlayWay, model.DropWater, model.Amount, model.CompanyType);
            return RedirectToAction("");
        }

        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult Remove(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            DropManager.Remove(Id.Value, MatrixUser);
            return SuccessAction();
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow), HttpPost]
        public ActionResult Remove(AutoDropWaterModel model)
        {
            DropManager.RemoveAutoDrop(model.GamePlayWay, model.DropWater, model.Amount, model.CompanyType);
            return SuccessAction();
        }
        [UserAuthorize(UserState.Active, Role.Company, Role.Shadow)]
        public ActionResult RemoveBetAuto(int? Id)
        {
            if (!Id.HasValue) PageNotFound();
            DropManager.RemoveBetAutoDrop(Id.Value);
            return SuccessAction();
        }
    }
}
