using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Controllers
{
    public class SettleController : BaseController
    {

        [UserAuthorize(Models.UserState.Active, Role.Company)]
        public ActionResult Index()
        {
            var companys = TodayLotteryCompany.Instance.GetTodayCompany();
            return View(companys);
        }
        [UserAuthorize(Models.UserState.Active, Role.Company), HttpPost]
        public ActionResult Settle(int Company)
        {
            var cp = LotterySystem.Current.FindCompany(Company);
            if (cp == null) PageNotFound();
            if (cp.IsOpen()) throw new BusinessException(Resource.NotCloseTime);
            using (SettleManager SettleManager = new SettleManager())
            {
                if (!SettleManager.CanSettle(Company)) throw new BusinessException(Resource.CompanyWasSettled);

                SettleManager.Settle(Company);
            }

            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success
                });
            else
                return SuccessAction();
        }
        [UserAuthorize(Models.UserState.Active, Role.Company), HttpPost]
        public ActionResult Resettle(int Company)
        {
            var cp = LotterySystem.Current.FindCompany(Company);
            if (cp == null) PageNotFound();
            using (SettleManager SettleManager = new SettleManager())
            {
                if (cp.IsOpen()) throw new BusinessException(Resource.NotCloseTime);
                SettleManager.Resettle(Company);
            }
            if (Request.IsAjaxRequest())
                return Json(new JsonResultModel
                {
                    IsSuccess = true,
                    Message = Resource.Success
                });
            else
                return SuccessAction();
        }
    }
}
