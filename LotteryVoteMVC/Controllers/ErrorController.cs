using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult NoPermissions()
        {
            if (Request.IsAjaxRequest())
            {
                JsonResultModel result = new JsonResultModel();
                result.IsSuccess = false;
                result.Message = Resource.NoPermission;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
                return View();
        }

    }
}
