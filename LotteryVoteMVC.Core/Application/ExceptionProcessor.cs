using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Core.Exceptions;
using System.Web.Mvc;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Resources;
using System.Web.Script.Serialization;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Controllers;
using System.Web;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Core.Application
{
    public class ExceptionProcessor
    {
        private bool? _isAjax;
        public bool IsAjax
        {
            get
            {
                if (!_isAjax.HasValue)
                    _isAjax = TargetController.Request.IsAjaxRequest();
                return _isAjax.Value;
            }
        }
        public string Url
        {
            get
            {
                return TargetController.Request.RawUrl;
            }
        }
        public Controller TargetController { get; private set; }
        public ExceptionContext ExContext { get; private set; }
        public LogManager LogManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LogManager>();
            }
        }
        public bool BaseCatch { get; private set; }
        public ExceptionProcessor(Controller controller, ExceptionContext exContext)
        {
            this.TargetController = controller;
            this.ExContext = exContext;
        }

        public void Process(Exception ex)
        {
            var exType = ex.GetType();
            if (exType == typeof(NoPermissionException))
            {
                if (IsAjax)
                    ProcessWithAjax(Resource.NoPermission);
                else
                    ExContext.Result = new RedirectResult("~/Error/NoPermissions");
                LogManager.LogError(ex, ErrorLevel.User, Url, GetRemark((ex as NoPermissionException).Action));
            }
            else if (exType == typeof(BusinessException))
            {
                var bsEx = (BusinessException)ex;
                if (IsAjax)
                    ProcessWithAjax(bsEx.Message);
                else
                {
                    var baseController = TargetController as BaseController;
                    ExContext.Result = baseController == null ? new ViewResult { ViewName = "~/Views/Error/Index" } : baseController.ErrorAction(bsEx.Message);
                }
                LogConsole.Error(GetRemark(bsEx.Message));
            }
            else if (exType == typeof(InvalidDataException))
            {
                var dataEx = (InvalidDataException)ex;
                if (IsAjax)
                    ProcessWithAjax(Resource.WrongMessage);
                else
                {
                    var baseController = TargetController as BaseController;
                    ExContext.Result = baseController == null ? new ViewResult { ViewName = "~/Views/Error/Index" } : baseController.ErrorAction(Resource.WrongMessage);
                }
                LogManager.LogError(ex, ErrorLevel.InvalidData, Url, GetRemark(dataEx.DataName));
            }
            else if (exType == typeof(HttpException))
            {
                LogManager.LogError(ex, ErrorLevel.User, Url, GetRemark(string.Empty));
                BaseCatch = true;
            }
            else if (exType == typeof(SqlException))
            {
                var sqlEx = ex as SqlException;
                string msg = Resource.WrongMessage;
                if (sqlEx.Number == -2)
                    msg = Resource.TimeoutError;

                if (IsAjax)
                    ProcessWithAjax(msg);
                else
                {
                    var baseController = TargetController as BaseController;
                    ExContext.Result = baseController == null ? new ViewResult { ViewName = "~/Views/Error/Index" } : baseController.ErrorAction(msg);
                }
                LogManager.LogError(ex, ErrorLevel.InvalidData, Url, GetRemark(sqlEx.Source));
            }
            else
            {
                if (IsAjax)
                    ProcessWithAjax(Resource.WrongMessage);
                else
                {
                    var baseController = TargetController as BaseController;
                    ExContext.Result = baseController == null ? new ViewResult { ViewName = "~/Views/Error/Index" } : baseController.ErrorAction(Resource.WrongMessage);
                }
                LogManager.LogError(ex, ErrorLevel.Application, Url, GetRemark(string.Empty));
            }
        }


        private void ProcessWithAjax(string message)
        {
            JsonResultModel result = new JsonResultModel();
            result.IsSuccess = false;
            result.Message = message;
            JavaScriptSerializer s = new JavaScriptSerializer();
            var html = s.Serialize(result);
            TargetController.Response.ContentType = "text/json";
            TargetController.Response.Write(html);
        }
        private string GetRemark(string sourceMessage)
        {
            var baseController = TargetController as BaseController;
            return baseController == null ? sourceMessage : string.Format("{0} User:{1}", sourceMessage, baseController.CurrentUser);
        }
    }
}
