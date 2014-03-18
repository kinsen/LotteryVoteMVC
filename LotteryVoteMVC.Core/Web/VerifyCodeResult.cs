using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;

namespace LotteryVoteMVC.Core.Web
{
    public class VerifyCodeResult : ActionResult
    {
        public override void ExecuteResult(ControllerContext context)
        {
            VerifyCode code = new VerifyCode(5);
            code.FontSize = 14;
            code.FontFamilyStr = LotterySystem.Current.CaptchaFontFamily;
            //code.Wave = false;
            code.WaveValue = 1;
            code.CreateImage(context.RequestContext.HttpContext);
            context.RequestContext.HttpContext.Session.Add("VerifyCode", code.GenerateCode.ToLower());
        }
    }
}
