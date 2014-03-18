using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LotteryVoteMVC.Utility
{
    /// <summary>
    /// Cookie序列化器（写入）
    /// </summary>
    public class CookieSerializer
    {
        private string _cookieName;
        private string _cookieValue;
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cookieName">Cookie名称.</param>
        /// <param name="cookieValue">Cookie值.</param>
        public CookieSerializer(string cookieName, string cookieValue)
        {
            this._cookieName = cookieName;
            this._cookieValue = cookieValue;
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cookieName">Cookie名称.</param>
        /// <param name="valueDic">Cookie值（保存多个）.</param>
        public CookieSerializer(string cookieName, IDictionary<string, string> valueDic)
        {
            this._cookieName = cookieName;
            this._cookieValue = ParseDicToValue(valueDic);
        }

        /// <summary>
        /// Cookie死亡时间.
        /// </summary>
        /// <value>
        /// The expires.
        /// </value>
        public DateTime Expires { get; set; }

        /// <summary>
        /// 保存Cookie
        /// </summary>
        public void SaveCookie()
        {
            HttpCookie cookie = new HttpCookie(_cookieName, _cookieValue);
            if (Expires != default(DateTime) || Expires != DateTime.Now)
                cookie.Expires = this.Expires;
            CookieParser.ResponseCookies.Add(cookie);
        }

        /// <summary>
        /// 将字典转换成String值.
        /// </summary>
        /// <param name="dic">The dic.</param>
        /// <returns></returns>
        private string ParseDicToValue(IDictionary<string, string> dic)
        {
            if (dic == null || dic.Count == 0)
                throw new ArgumentNullException("Cookie值不能为空!");
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var item in dic)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append("&");
                sb.AppendFormat("{0}={1}", item.Key, item.Value);
            }
            return sb.ToString();
        }
    }
}
