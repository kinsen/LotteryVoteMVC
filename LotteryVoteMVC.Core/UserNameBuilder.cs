using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 用户名生成器
    /// </summary>
    public class UserNameBuilder
    {
        public UserManager UserManager
        {
            get
            {
                return ManagerHelper.Instance.GetManager<UserManager>();
            }
        }
        private string _current = string.Empty;
        public string GetNextUserName(User currentUser, bool isBuildShadow = false)
        {
            _current = string.Empty;
            if (isBuildShadow)
            {
                int childCount = UserManager.DaUser.CountShadow(currentUser) + 1;
                _current = string.Format("{0}@{1}", childCount, currentUser.UserName);
                if (UserManager.DaUser.GetUserByUserName(_current) != null)
                    _current = string.Format("{0}a@{1}", childCount, currentUser.UserName);
            }
            else
            {
                int childCount = UserManager.DaUser.CountChild(currentUser);
                if (currentUser.Role < Role.Agent)
                {
                    if (childCount > 0)
                        Parse(childCount);
                    else
                        _current = "AA";
                    if (_current.Length == 1)
                        _current = "A" + _current;

                    if (currentUser.Role == Role.Company)
                        _current = "VD" + _current;
                    else
                        _current = currentUser.UserName + _current;
                }
                else
                    _current = currentUser.UserName + childCount.ToString("D3");
                if (UserManager.DaUser.GetUserByUserName(_current) != null)
                    _current+=1;
            }
            return _current;
        }

        private void Parse(int num)
        {
            if (num >= 1)
            {
                int parent = num / 26;
                int self = num % 26;
                _current = Convert.ToChar(self + 65) + _current;
                Parse(parent);
            }
        }
    }
}
