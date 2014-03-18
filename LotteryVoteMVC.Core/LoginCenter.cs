using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using System.Web.Security;
using System.Web;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Core
{
    public class LoginCenter
    {
        #region Fields
        private UserDataAccess _daUser;
        private LoginLogDataAccess _daLoginLog;

        #endregion

        #region Properties
        public UserDataAccess DaUser
        {
            get
            {
                if (_daUser == null)
                    _daUser = new UserDataAccess();
                return _daUser;
            }
        }
        public LoginLogDataAccess DaLoginLog
        {
            get
            {
                if (_daLoginLog == null)
                    _daLoginLog = new LoginLogDataAccess();
                return _daLoginLog;
            }
        }
        public LogManager LogManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<LogManager>();
            }
        }
        #endregion

        #region Methods
        public bool MemberLogin(string userName, string password, out string message)
        {
            return Login(userName, password, it => it != Role.Guest, out message);
        }
        public bool AgentLogin(string userName, string password, out string message)
        {
            return Login(userName, password, it => it == Role.Guest, out message);
        }
        private bool Login(string userName, string password, Predicate<Role> rolePredicate, out string message)
        {
            if (IsLogin)
                Logout();
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException("userName");
            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException("password");
            var user = DaUser.GetUserByUserName(userName);
            if (user == null || rolePredicate(user.Role))
            {
                message = Resource.LoginFailed;
                return false;
            }
            else if (!(EncryptHelper.Equal(password, user.UserInfo.Password)))
            {
                var errorTimes = LogManager.AddLoginFailedLog(user);
                message = string.Format(ModelResource.PwdErrorTimes, errorTimes);
                return false;
            }
            else if (user.UserInfo.State != UserState.Active)
            {
                message = Resource.UserWasLock;
                return false;
            }
            SetCookie(user.UserName);
            SetCurrentUser(user);
            DaLoginLog.AddLog(user);
            AddOnLineCount();
            message = Resource.Success;
            return true;
        }
        public static bool IsUserLogin()
        {
            var user = HttpContext.Current.Items["CurrentUser"] as User;
            return user != null;
        }
        public static User CurrentUser
        {
            get
            {
                var user = HttpContext.Current.Items["CurrentUser"] as User;
                return user;
            }
        }
        public static User Parent
        {
            get
            {
                var user = HttpContext.Current.Items["Parent"] as User;
                if (user == null)
                {
                    UserManager userManager = ManagerHelper.Instance.GetManager<UserManager>();
                    user = userManager.GetUser(CurrentUser.UserId);
                }
                return user;
            }
        }
        public bool IsLogin
        {
            get
            {
                return GetCurrentUser() != null;
            }
        }
        public void Logout()
        {
            FormsAuthentication.SignOut();
            SetCurrentUser(null);
            ClearSession();
        }
        private void SetCookie(string userName)
        {
            //FormsAuthentication.SetAuthCookie(userName, false);
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, userName, DateTime.Now, DateTime.Now.AddHours(1), false, userName, FormsAuthentication.FormsCookiePath);
            string ectTicket = FormsAuthentication.Encrypt(ticket);
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, ectTicket);
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        public User GetCurrentUser()
        {
            var user = HttpContext.Current.Items["CurrentUser"] as User;
            if (user != null) return user;

            object userObj = HttpContext.Current.Session["CurrentUser"];
            if (userObj != null)
            {
                int userId;
                try
                {
                    userId = Convert.ToInt32(userObj);
                }
                catch (Exception ex)
                {
                    LogConsole.Error("获取UserId失败!", ex);
                    return null;
                }
                user = DaUser.GetUserById(userId);
                SetCurrentUser(user);
            }
#if DEBUG
            else
            {
                string userName = HttpContext.Current.User.Identity.Name;
                if (string.IsNullOrEmpty(userName)) return null;
                user = DaUser.GetUserByUserName(userName);
                if (user != null)
                {
                    SetCurrentUser(user);
                    AddOnLineCount();
                }
            }
#endif
            return user;
        }
        private void SetCurrentUser(User user)
        {
            HttpContext.Current.Items["CurrentUser"] = user;

            object userId = null;
            if (user != null) userId = user.UserId;

            HttpContext.Current.Session["CurrentUser"] = userId;
        }
        private void AddOnLineCount()
        {
            var onLine = HttpContext.Current.Application[LotterySystem.M_ONLINEUSERCOUNT] as IDictionary<int, Pair<string, Role>>;
            var user = GetCurrentUser();

            HttpContext.Current.Application.Lock();
            if (onLine.ContainsKey(user.UserId))
                onLine.Remove(user.UserId);
            string sessionId = HttpContext.Current.Session.SessionID;
            onLine[user.UserId] = new Pair<string, Role>(sessionId, user.Role);
            HttpContext.Current.Application.UnLock();
        }
        private void ClearSession()
        {
            var onLine = HttpContext.Current.Application["OnLineUserCount"] as IDictionary<int, Pair<string, Role>>;
            string sessionId = HttpContext.Current.Session.SessionID;
            int userId = onLine.Where(it => it.Value.Key == sessionId).Select(it => it.Key).FirstOrDefault();
            if (onLine.ContainsKey(userId))
                onLine.Remove(userId);

            HttpContext.Current.Session.RemoveAll();
            HttpContext.Current.Session.Abandon();
        }
        #endregion
    }
}
