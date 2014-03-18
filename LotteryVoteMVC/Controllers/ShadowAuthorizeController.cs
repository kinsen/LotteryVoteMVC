using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Authorizes;
using LotteryVoteMVC.Models;
using System.Reflection;
using System.Text;
using LotteryVoteMVC.Core;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Controllers
{
    public class ShadowAuthorizeController : BaseController
    {
        private ShadowAuthManager _authManager;
        public ShadowAuthManager AuthManager
        {
            get
            {
                if (_authManager == null)
                    _authManager = new ShadowAuthManager();
                return _authManager;
            }
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Index()
        {
            var auths = AuthManager.GetAllAuth().OrderBy(it => it.Action).OrderBy(it => it.Controller);
            return View(auths);
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Add()
        {
            var controllers = this.GetType().Assembly.GetTypes().Where(it => it.IsSubclassOf(typeof(BaseController))).OrderBy(it => it.Name);
            ViewBag.Controllers = controllers;
            ViewBag.Roles = EnumHelper.ToMultiSelectList<Role>(it => false, it => it.ToString(), it => it.ToString());
            return View();
        }
        [UserAuthorize(UserState.Active, Role.Manager), HttpPost]
        public ActionResult Add(ShadowAuthModel model)
        {
            JsonResultModel result = new JsonResultModel();
            if (!(result.IsSuccess = ModelState.IsValid))
            {
                result.Message = ModelState.ToErrorString();
                return Json(result);
            }

            AuthManager.AddAuth(model.ToAuthAction(), model.AuthRole);
            result.Message = "成功!";
            return Json(result);
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Edit(int Id)
        {
            var auth = AuthManager.GetAuth(Id);
            if (auth == null) PageNotFound();
            var authRoles = AuthManager.GetAuthRoles(Id);
            ViewBag.Roles = EnumHelper.ToMultiSelectList<Role>(it => authRoles.Contains(it), it => it.ToString(), it => it.ToString());
            var model = new ShadowAuthModel
            {
                Name = auth.Name,
                Controller = auth.Controller,
                Action = auth.Action,
                MethodSign = auth.MethodSign,
                DefaultState = auth.DefaultState
            };
            return View(model);
        }
        [UserAuthorize(UserState.Active, Role.Manager), HttpPost]
        public ActionResult Edit(ShadowAuthModel model, int Id)
        {
            var auth = model.ToAuthAction();
            auth.AuthorizeId = Id;
            AuthManager.UpdateAuth(auth, model.AuthRole);
            return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult Delete(int Id)
        {
            AuthManager.DeleteAuth(Id);
            return RedirectToAction("Index");
        }

        [UserAuthorize(UserState.Active, Role.Manager)]
        public ActionResult GetActions(string controllerName)
        {
            controllerName = string.Concat(this.GetType().Namespace, ".", controllerName);
            JsonResultModel result = new JsonResultModel();
            var controllerType = Type.GetType(controllerName);

            if (result.IsSuccess = (controllerType != null))
            {
                var methods = controllerType.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(it => it.ReturnType == typeof(ActionResult) || it.ReturnType.IsSubclassOf(typeof(ActionResult)));
                List<KeyValuePair<string, string>> signs = new List<KeyValuePair<string, string>>();
                foreach (var method in methods)
                {
                    signs.Add(new KeyValuePair<string, string>(method.Name, ParseActionSign(method)));
                }
                result.Model = new { controller = controllerName, actions = signs };
            }
            else
                result.Message = "找不到Controller";
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckName(string name)
        {
            bool isValidate = false;
            if (Url.IsLocalUrl(Request.Url.AbsoluteUri))
            {
                isValidate = !AuthManager.IsExist(name);
            }
            return Json(isValidate, JsonRequestBehavior.AllowGet);
        }

        private string ParseActionSign(MethodInfo action)
        {
            var httpAttrs = action.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), false);
            object attrType = typeof(HttpGetAttribute);//默认使用HttpGet
            if (httpAttrs.Length > 0)
                attrType = httpAttrs[0];

            var args = action.GetParameters();
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var arg in args)
            {
                if (isFirst) isFirst = false;
                else sb.Append(',');
                var argType = arg.ParameterType.ToString();
                sb.AppendFormat("{0}:{1}", argType.Substring(argType.LastIndexOf('.') + 1), arg.Name);
            }
            //无参数Action则用Empty表示
            if (sb.Length == 0)
                sb.Append("Empty");

            string fullTypeName = attrType.ToString();
            string typeName = fullTypeName.Substring(fullTypeName.LastIndexOf('.') + 1);
            var actionHttpAttr = typeName.Replace("Attribute", string.Empty);

            return string.Format("{0}({1})", actionHttpAttr, sb.ToString());
        }
    }
}
