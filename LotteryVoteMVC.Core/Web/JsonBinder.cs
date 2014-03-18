using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Web
{
    public class JsonBinder<T> : IModelBinder where T : class
    {

        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var forms = controllerContext.HttpContext.Request.Form;
            if (forms == null || forms.Count == 0)
            {
                LogConsole.Error("提交的表单没有数据");
                return default(T);
            }
            //从请求中获取提交的参数数据
            var json = forms[bindingContext.ModelName] as string ?? forms[0] as string;

            if (string.IsNullOrEmpty(json)) return default(T);

            try
            {
                return JSONHelper.ToObject<T>(json);
            }
            catch (Exception ex)
            {
                LogConsole.Error(string.Format("JSON反序列化失败!Data:{0}", json), ex);
                return default(T);
            }
        }
    }
}
