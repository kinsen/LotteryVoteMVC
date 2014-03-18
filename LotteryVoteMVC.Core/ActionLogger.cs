using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Data;
using System.Web;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    public class ActionLogger
    {
        private const string M_ACTIONLOGDA = "ActionLogDa";
        private static ActionLogDataAccess GetLogDA()
        {
            var actionLog = HttpContext.Current.Items[M_ACTIONLOGDA] as ActionLogDataAccess;
            if (actionLog == null)
            {
                actionLog = new ActionLogDataAccess();
                HttpContext.Current.Items[M_ACTIONLOGDA] = actionLog;
            }
            return actionLog;
        }
        public static void Log(User user, User targetUser, string action, string detail)
        {
            var log = new ActionLog
            {
                UserId = user.UserId,
                TargetUserId=targetUser.UserId,
                Action = action,
                Detail = detail,
                IPAddress = IPHelper.IPAddress
            };
            var daActionLog = GetLogDA();
            daActionLog.Insert(log);
        }
    }
}
