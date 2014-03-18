using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Controllers
{
    public class ValidateController : BaseController
    {
        //
        // GET: /Validate/

        public JsonResult CheckUserName(string userName)
        {
            bool isValidate = false;
            if (Url.IsLocalUrl(Request.Url.AbsoluteUri))
            {
                isValidate = !UserManager.IsExsit(userName);
            }
            return Json(isValidate, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckPassword(string YourPassword)
        {
            bool isValidate = false;
            if (Url.IsLocalUrl(Request.Url.AbsoluteUri) && CurrentUser != null)
            {
                isValidate = EncryptHelper.Equal(YourPassword, CurrentUser.UserInfo.Password);
            }
            return Json(isValidate, JsonRequestBehavior.AllowGet);
        }
    }
}
