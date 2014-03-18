using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Resources;
using System.Web.Mvc;
using System.Resources;
using System.Text.RegularExpressions;
using System.Configuration;
using System.Web;

namespace LotteryVoteMVC.Core
{
    public static class Extended
    {
        public static bool IsOpen(this LotteryCompany company, double closeTimeExtend = 0)
        {
            var openTime = DateTime.Today.Add(company.OpenTime);
            var closeTime = DateTime.Today.Add(company.CloseTime).AddMinutes(closeTimeExtend);
            var now = DateTime.Now;
            return openTime <= now && now <= closeTime;
        }
        /// <summary>
        /// 公司指定玩法是否在投注时间内
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="gpw">The GPW.</param>
        /// <returns>
        ///   <c>true</c> if [is in bet time] [the specified company]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsInBetTime(this LotteryCompany company, GamePlayWay gpw)
        {
            if (gpw.GameType == GameType.A_B_PL2)
                return IsA_B_PL2InBetTime(company);

            DateTime openTime = DateTime.Today.Add(company.OpenTime);
            DateTime closeTime = DateTime.Today.Add(company.CloseTime).AddSeconds(-1 * LotterySystem.Current.PreCloseTime);//必须先将关盘时间提前5秒钟

            if (gpw.PlayWay == PlayWay.Last)        //走地时间
                closeTime = closeTime.AddMinutes(LotterySystem.Current.AdditionalLastBetMinutes);
            DateTime now = DateTime.Now;
            return now >= openTime && now <= closeTime;
        }
        /// <summary>
        /// 是否A&B PL2开盘时间
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns>
        ///   <c>true</c> if [is a_ b_ P l2 in bet time] [the specified company]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsA_B_PL2InBetTime(this LotteryCompany company)
        {
            var companyA = company.CompanyType == CompanyType.EighteenA ? company :
                TodayLotteryCompany.Instance.GetTodayCompany().Where(it => it.CompanyType == CompanyType.EighteenA).First();
            var companyB = company != companyA ? company :
                TodayLotteryCompany.Instance.GetTodayCompany().Where(it => it.CompanyType == CompanyType.EighteenB).First();

            DateTime a_openTime = DateTime.Today.Add(companyA.OpenTime);
            DateTime a_closeTime = DateTime.Today.Add(companyA.CloseTime).AddSeconds(-1 * LotterySystem.Current.PreCloseTime);//必须先将关盘时间提前5秒钟
            DateTime b_openTime = DateTime.Today.Add(companyB.OpenTime);
            DateTime b_closeTime = DateTime.Today.Add(companyB.CloseTime).AddSeconds(-1 * LotterySystem.Current.PreCloseTime);//必须先将关盘时间提前5秒钟

            var openTime = a_openTime < b_openTime ? a_openTime : b_openTime;
            var closeTime = a_closeTime < b_closeTime ? a_closeTime : b_closeTime;

            var now = DateTime.Now;
            return now >= openTime && now <= closeTime;
        }
        public static bool CanCancel(this BetSheet sheet)
        {

            return sheet.Status == BetStatus.Valid && sheet.CreateTime.AddMinutes(LotterySystem.Current.CanCancleAfterBetMinutes) > DateTime.Now;
        }
        public static bool CanCancel(this BetOrder order)
        {
            return order.Status == BetStatus.Valid && order.CreateTime.AddMinutes(LotterySystem.Current.CanCancleAfterBetMinutes) > DateTime.Now;
        }
        public static string GetGPWDescription(int gameplaywayId)
        {
            var gpw = LotterySystem.Current.FindGamePlayWay(gameplaywayId);
            if (gpw == null)
                throw new InvalidDataException("找不到游戏玩法;GameplaywayId:" + gameplaywayId);
            return gpw.GetGPWDescription();
        }
        public static string GetGPWDescription(this GamePlayWay gpw)
        {
            var gtDesc = EnumHelper.GetEnumDescript(gpw.GameType);
            return string.Format("{0} {1}", gtDesc.Description, default(PlayWay) == gpw.PlayWay ? string.Empty : Resource.ResourceManager.GetString(gpw.PlayWay.ToString()));
        }
        /// <summary>
        /// 号码是不是指定玩法
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <returns></returns>
        public static bool NumIsCorrectGameTypeFormat(string num, int gameplaywayId)
        {
            var gpw = LotterySystem.Current.FindGamePlayWay(gameplaywayId);
            int len = num.Length;
            var isBatter = num.IsBatterNum() || num.IsStartBatterNum();
            string ruleFormat = @"^\d{{{0}}}$";
            if (num.IsNumArray()) ruleFormat = @"^(\d{{{0}}}\,{{0,1}})+$";
            else if (num.IsRangeNum()) ruleFormat = @"^\d{{{0}}}-\d{{{0}}}$";
            switch (gpw.GameType)
            {
                case GameType.TwoDigital: return isBatter ? len == 2 : new Regex(string.Format(ruleFormat, 2)).IsMatch(num);
                case GameType.ThreeDigital: return isBatter ? len == 3 : new Regex(string.Format(ruleFormat, 3)).IsMatch(num);
                case GameType.FourDigital: return isBatter ? len == 4 : new Regex(string.Format(ruleFormat, 4)).IsMatch(num);
                case GameType.FiveDigital: return isBatter ? len == 5 : new Regex(string.Format(ruleFormat, 5)).IsMatch(num);
                case GameType.PL2: return num.IsPL2();
                case GameType.PL3: return num.IsPL3();
                case GameType.A_B_PL2:
                case GameType.B_C_PL2:
                case GameType.C_A_PL2: return num.IsPL2();
                default: return false;
            }
        }
        /// <summary>
        /// 获取一个连号的号码集合，支持连号有--系列，*细类，,系列
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static IList<string> GetRangeNum(string num)
        {
            if (num.IsBatterNum()) return num.ParseBatterNums(5);
            else if (num.IsRangeNum()) return num.ParseRangeNums();
            else if (num.IsNumArray()) return num.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
            else if (num.IsStartBatterNum()) return num.ParseStartBatterNums();
            else
                throw new InvalidDataException("号码不是连号!Num:" + num);
        }
        /// <summary>
        /// 解析Sheet的号码，(例如十二生肖，单双等)
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static string ParseSheetNum(string num)
        {
            if (num.IsLetter() || num.Length > 8) return Resource.ResourceManager.GetString(num);
            else
                return num;
        }
        /// <summary>
        /// 将赌注信息列表按照游戏类型分类转换成table的Html
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static MvcHtmlString ParseWagersToTable(this IEnumerable<WagerItem> source)
        {
            Dictionary<GameType, Dictionary<PlayWay, WagerItem>> wagerDic = new Dictionary<GameType, Dictionary<PlayWay, WagerItem>>();
            foreach (var wager in source.OrderBy(it => it.GamePlayTypeId))
            {
                var gpw = LotterySystem.Current.FindGamePlayWay(wager.GamePlayTypeId);
                if (!wagerDic.ContainsKey(gpw.GameType))
                    wagerDic.Add(gpw.GameType, new Dictionary<PlayWay, WagerItem>());
                wagerDic[gpw.GameType].Add(gpw.PlayWay, wager);
            }
            StringBuilder htmlSb = new StringBuilder();
            htmlSb.Append("<table class=\"innerTable\" cellpadding=\"0\" cellspacing=\"0\"><thead>");
            htmlSb.Append("<tr>{0}</tr>");
            htmlSb.Append("</thead><tr class='head'>{1}</tr>");
            htmlSb.Append("<tr>{2}</tr>");
            htmlSb.Append("</table>");

            StringBuilder gametypeSb = new StringBuilder();
            StringBuilder playwaySb = new StringBuilder();
            StringBuilder wagerSb = new StringBuilder();
            foreach (var wager in wagerDic)
            {
                gametypeSb.AppendFormat("<td colspan=\"{1}\">{0}</td>", wager.Key.GetDesc(), wager.Value.Count);
                foreach (var item in wager.Value)
                {
                    if (item.Key != default(PlayWay))
                        playwaySb.AppendFormat("<td>{0}</td>", Resource.ResourceManager.GetString(item.Key.ToString().Replace("+", "And")));
                    wagerSb.AppendFormat("<td><font color='red'>{1}</font>{0:N}</td>", item.Value.Wager, item.Value.IsFullPermutation ?
                        "(" + Resource.D + ")" : "");
                }
            }
            string html = string.Format(htmlSb.ToString(), gametypeSb.ToString(), playwaySb.ToString(), wagerSb.ToString());
            return MvcHtmlString.Create(html);
        }
        /// <summary>
        /// 获取枚举的描述值
        /// </summary>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public static string GetDesc(this Enum e)
        {
            return EnumHelper.GetEnumDescript(e).Description;
        }
        public static string ToStr(this PlayWay pw)
        {
            return pw == default(PlayWay) ? string.Empty : Resource.ResourceManager.GetString(pw.ToString());
        }
        public static bool IsStopAcceptBet(int companyId, int gpwId)
        {
            var settings = StopAcceptBetSettings.Instance.GetStopAcceptBetSettings();
            if (settings == null || settings.Count == 0) return false;

            return settings.Find(it => it.Key == companyId && it.Value == gpwId) != null;
        }

        /// <summary>
        /// 将枚举转换成列表项
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="resource">要使用的资源文件，默认Resource</param>
        /// <returns></returns>
        public static IEnumerable<SelectListItem> GetSelectList<T>(T selectedValue, ResourceManager resource = null) where T : struct
        {
            if (resource == null) resource = Resource.ResourceManager;

            return EnumHelper.ToSelectList<T>(it => it.Equals(selectedValue), it => resource.GetString(it.ToString()), it => it.ToString());
        }
        public static IEnumerable<SelectListItem> GetDescSelectList<T>(T selectValue) where T : struct
        {
            var desc = EnumHelper.GetDescription<T>();
            return desc.Select(it => new SelectListItem
            {
                Selected = it.Value == Convert.ToInt32(selectValue),
                Text = it.Description,
                Value = it.Name
            });
        }
        public static MultiSelectList GetMultiSelectList<T>(IEnumerable<T> selectedValues, ResourceManager resource = null) where T : struct
        {
            if (resource == null) resource = Resource.ResourceManager;
            return EnumHelper.ToMultiSelectList<T>(it => selectedValues.Contains(it), it => resource.GetString(it.ToString()), it => it.ToString());
        }
        /// <summary>
        /// 是否拥有指定操作权限
        /// </summary>
        /// <param name="page">The page.</param>
        /// <param name="authRole">The auth role.</param>
        /// <param name="controller">The controller.</param>
        /// <param name="action">The action.</param>
        /// <returns>
        ///   <c>true</c> if the specified page has right; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasRight(this WebViewPage page, Role authRole, string controller, string action)
        {
            var user = LoginCenter.CurrentUser;
            if (user == null) return false;
            if (user.Role == Role.Shadow)
            {
                var parent = LoginCenter.Parent;
                ShadowAuthManager authManager = ManagerHelper.Instance.GetManager<ShadowAuthManager>();
                var auths = authManager.GetShadowAuth();
                Spec<AgentAuthorizeAction> condition = it => it.Controller.Equals(controller, StringComparison.InvariantCultureIgnoreCase);
                condition = condition.And(it => it.Action.Equals(action, StringComparison.InvariantCultureIgnoreCase));
                condition = condition.And(it => it.MethodSign.StartsWith("HttpPost"));
                return auths.Contains(it => condition(it)) && parent.Role == authRole;
            }
            return user.Role == authRole;
        }

        /// <summary>
        /// 根据当前域名获取登录地址
        /// </summary>
        /// <returns></returns>
        public static string GetLoginUrl()
        {
            string domain = HttpHelper.GetCurrentDomain();
            return ConfigurationManager.AppSettings[domain] ?? "~/Member/Login";
        }
    }
}
