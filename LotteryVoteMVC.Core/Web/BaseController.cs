using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core;
using System.Web.Script.Serialization;
using System.Web.Routing;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Web;

namespace LotteryVoteMVC.Controllers
{
    [HandleError]
    public class BaseController : Controller
    {
        #region Comm Properties
        private LoginCenter _loginCenter;
        public LoginCenter LoginCentre
        {
            get
            {
                if (_loginCenter == null)
                    _loginCenter = new LoginCenter();
                return _loginCenter;
            }
        }

        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        private User _matrixUser;
        /// <summary>
        ///用户母体(即可能存在分身用户，本属性可获取到最终真实的母体用户).
        /// </summary>
        public User MatrixUser
        {
            get
            {
                if (_matrixUser == null)
                {
                    if (CurrentUser.Role == Role.Shadow)
                    {
                        _matrixUser = UserManager.GetUser(CurrentUser.ParentId);
                        HttpContext.Items["Parent"] = _matrixUser;
                    }
                    else
                        _matrixUser = CurrentUser;
                }
                return _matrixUser;
            }
        }

        private User _currentUser;
        public User CurrentUser
        {
            get
            {
                if (_currentUser == null)
                    _currentUser = LoginCentre.GetCurrentUser();
                return _currentUser;
            }
        }
        #endregion

        /// <summary>
        /// 当前请求分页
        /// </summary>
        public int CurrentPage
        {
            get
            {
                int page;
                if (!int.TryParse(Request.Params["p"], out page) || page == 0)
                    page = 1;
                return page;
            }
        }

        protected override void ExecuteCore()
        {
            CultureHelper.InitCulture();
            base.ExecuteCore();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
#if !DEBUG
            ExceptionProcessor exProcessor = new ExceptionProcessor(this, filterContext);
            exProcessor.Process(filterContext.Exception);
            if (exProcessor.BaseCatch) base.OnException(filterContext);
            else
            {
                filterContext.ExceptionHandled = true;
            }
#endif
        }

        protected ActionResult RedirectToCurrentAction()
        {
            string controller = string.Empty;
            string action = string.Empty;
            RouteValueDictionary routeValue = new RouteValueDictionary();
            foreach (var item in this.RouteData.Values)
            {
                string key = item.Key.ToLower();
                if (key == "controller")
                    controller = item.Value.ToString();
                else if (key == "action")
                    action = item.Value.ToString();
                else
                    routeValue.Add(item.Key, item.Value);
            }
            return RedirectToAction(action, controller, routeValue);
        }
        /// <summary>
        /// 成功提示页
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public ActionResult SuccessAction(string message = null)
        {
            //获取成功页要跳转的页面
            var redirectPage = Request.UrlReferrer == null ? "/" : Request.UrlReferrer.ToString();
            var promptMsg = message ?? string.Format("{0} {1}", Resource.Action, Resource.Success);
            return RedirectToAction("Index", "Prompt", new { Msg = HttpUtility.UrlEncode(promptMsg), Url = HttpUtility.UrlEncode(redirectPage) });
        }
        /// <summary>
        /// 失败提示页面.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public ActionResult ErrorAction(string message = null)
        {
            //获取成功页要跳转的页面
            var redirectPage = Request.UrlReferrer == null ? "/" : Request.UrlReferrer.ToString();
            var promptMsg = message ?? string.Format("{0} {1}", Resource.Action, Resource.Error);
            return RedirectToAction("Error", "Prompt", new { Msg = HttpUtility.UrlEncode(promptMsg), Url = HttpUtility.UrlEncode(redirectPage) });
        }

        protected void PageNotFound()
        {
            throw new HttpException(404, "HTTP/1.1 404 Not Found");
        }
    }
}
