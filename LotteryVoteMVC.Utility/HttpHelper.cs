using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace LotteryVoteMVC.Utility
{
    public static class HttpHelper
    {
        /// <summary>
        /// 根据传入的地址获取域名.
        /// </summary>
        /// <param name="strHtmlPagePath">The STR HTML page path.</param>
        /// <returns></returns>
        public static string GetUrlDomainName(string strHtmlPagePath)
        {
            string p = @"http://(?<domain>[^/]*)";
            Regex reg = new Regex(p, RegexOptions.IgnoreCase);
            Match m = reg.Match(strHtmlPagePath);
            return m.Groups["domain"].Value;
        }

        /// <summary>
        /// 获取当前域名.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDomain()
        {
            return GetUrlDomainName(HttpContext.Current.Request.Url.ToString());
        }
    }
}
