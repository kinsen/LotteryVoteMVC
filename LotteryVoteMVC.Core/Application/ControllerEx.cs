using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;
using System.Globalization;

namespace LotteryVoteMVC.Core
{
    public static class ControllerEx
    {

        /// <summary>
        /// (Param["search"]==true)
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns>
        ///   <c>true</c> if the specified controller is search; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsSearch(this Controller controller)
        {
            string search = controller.Request.Params["search"];
            return !string.IsNullOrEmpty(search) && search.ToLower() == "true";
        }
        /// <summary>
        /// 获取Params中的参数(PS:只支持值类型)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public static T GetValueParam<T>(this Controller controller, string name, T defaultValue = default(T)) where T : struct
        {
            string arg = controller.Request.Params[name];
            if (string.IsNullOrEmpty(arg)) return defaultValue;
            var targetType = typeof(T);
            if (targetType.IsEnum)
                return EnumHelper.GetEnum<T>(arg);
            else
                return (T)Convert.ChangeType(arg, targetType);
        }

        #region Comm Search Elements
        public static string UserName(this Controller controller)
        {
            return controller.Request.Params["UserName"] ?? string.Empty;
        }
        /// <summary>
        /// 号码
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static string Num(this Controller controller)
        {
            return controller.Request.Params["num"] ?? string.Empty;
        }
        /// <summary>
        /// 选中的公司
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static int Company(this Controller controller)
        {
            return controller.GetValueParam<int>("Company");
        }
        /// <summary>
        /// 获取选中的游戏玩法
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static int GamePlayWay(this Controller controller)
        {
            return controller.GetValueParam<int>("gameplayway");
        }
        /// <summary>
        /// 注单状态.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static BetStatus BetStatus(this Controller controller)
        {
            return controller.GetValueParam<BetStatus>("BetStatus", LotteryVoteMVC.Models.BetStatus.Valid);
        }
        /// <summary>
        /// 输赢
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static WinLost WinLost(this Controller controller)
        {
            return controller.GetValueParam<WinLost>("WinLost", LotteryVoteMVC.Models.WinLost.All);
        }
        /// <summary>
        ///日期
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static DateTime Date(this Controller controller)
        {
            string arg = controller.Request.Params["Date"];
            if (string.IsNullOrEmpty(arg)) return DateTime.Today;
            try
            {
                return DateTime.Parse(arg, new CultureInfo("en-GB"));
            }
            catch
            {
                return DateTime.Today;
            }
        }
        public static DateTime FromDate(this Controller controller)
        {
            string arg = controller.Request.Params["FromDate"];
            if (string.IsNullOrEmpty(arg)) return DateTime.Today;
            try
            {
                return DateTime.Parse(arg, new CultureInfo("en-GB"));
            }
            catch
            {
                return DateTime.Today;
            }
        }
        public static DateTime ToDate(this Controller controller)
        {
            string arg = controller.Request.Params["ToDate"];
            if (string.IsNullOrEmpty(arg)) return DateTime.Today;
            try
            {
                return DateTime.Parse(arg, new CultureInfo("en-GB"));
            }
            catch
            {
                return DateTime.Today;
            }
        }
        /// <summary>
        ///获取要排序的字段
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static string Sort(this Controller controller)
        {
            return controller.Request.Params["sort"] ?? string.Empty;
        }
        /// <summary>
        /// 排序类型（Asc/Desc,,默认asc）
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns></returns>
        public static string SortType(this Controller controller, string defaultValue = "asc")
        {
            var sortType = controller.Request.Params["SortType"];
            if (string.IsNullOrEmpty(sortType))
                sortType = defaultValue;
            else
                sortType = sortType.ToLower();
            var sortTypes = new[] { "asc", "desc" };
            if (!sortTypes.Contains(sortType))
                sortType = "asc";
            return sortType;
        }
        public static int CompanyType(this Controller controller)
        {
            return controller.GetValueParam<int>("CompanyType", 0);
        }
        public static double DropValue(this Controller controller)
        {
            return GetValueParam<double>(controller, "DropValue", 0);
        }
        public static double DropWater(this Controller controller)
        {
            return GetValueParam<double>(controller, "DropWater", 0);
        }
        public static decimal Amount(this Controller controller)
        {
            return GetValueParam<decimal>(controller, "Amount", 0);
        }
        #endregion
    }
}
