using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Resources
{
    public static class LogResources
    {
        public const string AddUser = "AddUser";
        public const string AddShadow = "AddShadow";
        public const string EditUser = "EditUser";
        public const string EditShadow = "EditShadow";
        public const string UpdatePassword = "UpdatePassword";
        public const string StartUser = "StartUser";
        public const string StopUser = "StopUser";
        public const string UpdateUserComm = "UpdateUserComm";
        public const string UpdateUserGameLimit = "UpdateUserGameLimit";
        public const string UpdateUserBetLimit = "UpdateUserBetLimit";
        public const string AddCommGroup = "AddCommGroup";
        public const string UpdateCommGroup = "UpdateCommGroup";
        public const string RemoveCommGroup = "RemoveCommGroup";
        public const string AddLotteryCompany = "AddLotteryCompany";
        public const string UpdateLotteryCompany = "UpdateLotteryCompany";
        public const string UpdateLotteryCompanyCycle = "UpdateLotteryCompanyCycle";
        public const string RemoveLotteryCompany = "RemoveLotteryCompany";
        public const string CancelSheet = "CancelSheet";
        public const string CancelOrder = "CancelOrder";

        public static string GetAddUser(string userName)
        {
            return string.Format("Add user {0}!", userName);
        }
        public static string GetAddShadow(string userName)
        {
            return "Add sub user " + userName;
        }
        public static string GetEditUser(string userName)
        {
            return "Edit user " + userName;
        }
        public static string GetEditShadow(string userName)
        {
            return "Edit sub user " + userName;
        }
        public static string GetStartUser(string userName)
        {
            return "Start User " + userName;
        }
        public static string GetUpdatePassword(string userName)
        {
            return string.Format("Update {0}'s password", userName);
        }
        public static string GetStopUser(string userName)
        {
            return "Stop User " + userName;
        }
        public static string GetUpdateUserComm(string specie)
        {
            return string.Format("Update user's {0} commission!", specie);
        }
        public static string GetUpdateUserGameLimit(string userName)
        {
            return string.Format("Update {0}'s game limit.", userName);
        }
        public static string GetUpdateUserBetLimit(string userName)
        {
            return string.Format("Update {0}'s bet limit.", userName);
        }
        public static string GetAddCommGroup(string specie, string groupName)
        {
            return string.Format("Add a new group name {0} in to market {1}!", groupName, specie);
        }
        public static string GetUpdateCommGroup(int groupid)
        {
            return string.Format("Update comm group! groupId:{0}", groupid);
        }
        public static string GetRemoveCommGroup(int groupId)
        {
            return string.Format("Remove comm group, group id :{0}", groupId);
        }
        public static string GetAddLotteryCompany(string company, int id)
        {
            return string.Format("Add new lottery company name {0},company id:{1}", company, id);
        }
        public static string GetUpdateLotteryCompany(string company, int id)
        {
            return string.Format("Update lottery company {0},company id:{1}", company, id);
        }
        public static string GetUpdateLotteryCompanyCycle(int id, IEnumerable<DayOfWeek> days)
        {
            return string.Format("update company(Id:{0})'s lottery cycle,{1}", id, string.Join(",", days));
        }
        public static string GetRemoveLotteryCompany(int id)
        {
            return string.Format("Remove lottery company(Id:{0})", id);
        }
    }
}
