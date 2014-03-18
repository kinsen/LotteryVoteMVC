using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Core.Exceptions
{
    /// <summary>
    /// 无授权
    /// </summary>
    public class NoPermissionException : Exception
    {
        public string Action { get; set; }
        public NoPermissionException(string action)
            : base()
        {
            this.Action = action;
        }
        public NoPermissionException(string action, string message)
            : base(message)
        {
            this.Action = action;
        }
        public NoPermissionException(string action, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Action = action;
        }
    }
}
