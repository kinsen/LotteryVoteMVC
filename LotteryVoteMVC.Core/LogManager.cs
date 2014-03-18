using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Core
{
    public class LogManager : ManagerBase
    {
        private LoginLogDataAccess _daLoginLog;
        private LoginFailedLogDataAccess _daLoginFailedLog;
        private ActionLogDataAccess _daActionLog;
        private SystemErrorDataAccess _daSystemErrorLog;
        public LoginLogDataAccess DaLoginLog
        {
            get
            {
                if (_daLoginLog == null)
                    _daLoginLog = new LoginLogDataAccess();
                return _daLoginLog;
            }
        }
        public LoginFailedLogDataAccess DaLoginFailedLog
        {
            get
            {
                if (_daLoginFailedLog == null)
                    _daLoginFailedLog = new LoginFailedLogDataAccess();
                return _daLoginFailedLog;
            }
        }
        public ActionLogDataAccess DaActionLog
        {
            get
            {
                if (_daActionLog == null)
                    _daActionLog = new ActionLogDataAccess();
                return _daActionLog;
            }
        }
        public SystemErrorDataAccess DaSystemErrorLog
        {
            get
            {
                if (_daSystemErrorLog == null)
                    _daSystemErrorLog = new SystemErrorDataAccess();
                return _daSystemErrorLog;
            }
        }
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }

        /// <summary>
        ///返回今日错误密码次数
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public int AddLoginFailedLog(User user)
        {
            DaLoginFailedLog.Insert(user);
            int count = DaLoginFailedLog.GetFailedCount(user, DateTime.Today);
            if (count >= LotterySystem.Current.LockUserOfFailedPWDCount && user.UserInfo.State == UserState.Active)
            {
                UserManager.DaUserInfo.UpdateState(user, UserState.Suspended);
                ActionLogger.Log(user, user, LogResources.StopUser, LogResources.GetStopUser(user.UserName));
            }
            return count;
        }

        public PagedList<LoginLog> GetLoginLog(User user, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<LoginLog>(DaLoginLog.GetLog(user, start, end), pageIndex, pageSize, DaLoginLog.CountLog(user));
        }
        public PagedList<LoginFailedLog> GetFailedLoginLog(User user, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<LoginFailedLog>(DaLoginFailedLog.GetLog(user, start, end), pageIndex, pageSize, DaLoginFailedLog.CountLog(user));
        }

        public PagedList<ActionLog> GetActionLog(User user, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<ActionLog>(DaActionLog.GetActionLog(user, start, end), pageIndex, pageSize, DaActionLog.CountActionLog(user));
        }
        public PagedList<ActionLog> GetActionLog(User user, string targetUser, DateTime fromDate, DateTime toDate, int pageIndex)
        {
            var userManager = ManagerHelper.Instance.GetManager<UserManager>();
            User target = null;
            if (!string.IsNullOrEmpty(targetUser))
                target = userManager.DaUser.GetUserByUserName(targetUser);
            if (target != null)
                userManager.IsParent(user.UserId, target.UserId, out target);
            if (target == null)
                target = user;          //如果不存在目标用户，则获取自身的修改信息
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<ActionLog>(DaActionLog.GetActionLog(target, fromDate, toDate, start, end), pageIndex, pageSize, DaActionLog.CountActionLog(target, fromDate, toDate));
        }

        public void LogError(Exception innerException, ErrorLevel level, string url, string remark = "")
        {
            SystemError error = new SystemError
            {
                ErrorLevel = level,
                ErrorMessage = innerException.Message,
                StackTrack = innerException.StackTrace ?? string.Empty,
                PageUrl = url,
                Remarks = remark,
                IP = IPHelper.IPAddress
            };
            try
            {
                DaSystemErrorLog.Insert(error);
            }
            catch (Exception ex)
            {
                Func<string> getDividingLines = () => { string line = string.Empty; for (int i = 0; i < 20; i++)line += "-"; return line; };
                LogConsole.Error(getDividingLines() + "插入日志失败" + getDividingLines());
                LogConsole.Error("插入SystemError失败!", ex);
                LogConsole.Error(getDividingLines() + getDividingLines());
            }
        }
        public SystemError GetError(User user, int id)
        {
            if (user.Role != Role.Manager) throw new NoPermissionException("查看系统日志");
            return DaSystemErrorLog.GetLog(id);
        }
        public PagedList<SystemError> GetSystemError(User user, int pageIndex)
        {
            if (user.Role != Role.Manager) throw new NoPermissionException("查看系统错误日志");
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<SystemError>(DaSystemErrorLog.GetLog(start, end), pageIndex, pageSize, DaSystemErrorLog.CountLog());
        }
    }
}
