using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LotteryVoteMVC.Utility.MVC
{
    public class PagerBuilder
    {
        private static string url = string.Empty;
        /// <summary>
        /// 创建分页导航Html标签
        /// </summary>
        /// <param name="currentPage">索引页码,从1开始.</param>
        /// <param name="pageCount">总页数.</param>
        /// <param name="showPageCount">显示总分页个数(奇数).</param>
        /// <returns></returns>
        public static string Build(int currentPage, int pageCount, int showPageCount)
        {
            int indexNum = HttpContext.Current.Request.RawUrl.IndexOf("?");
            string queryString = string.Empty;
            if (indexNum > 0)
            {
                //截取参数以前的原始Url
                url = HttpContext.Current.Request.RawUrl.Substring(0, indexNum + 1);
                //Url参数
                queryString = HttpContext.Current.Request.RawUrl.Substring(indexNum + 1);
                string[] queryStrings = queryString.Split('&');
                foreach (string str in queryStrings)
                {
                    //如果请求Url本身带有p则不添加参数
                    if (str.Split('=')[0].ToLower() == "p")
                        continue;
                    url += str + "&";
                }
            }
            else
                //原始不带请求参数的Url
                url = HttpContext.Current.Request.RawUrl + "?";

            #region Nonation
            //原始做法.用于没有使用URL重写时
            //url = HttpContext.Current.Request.Path + "?";
            //NameValueCollection queryString = HttpContext.Current.Request.QueryString;
            //if (queryString.Count > 0)
            //{
            //    foreach (string key in queryString)
            //    {
            //        if (key.ToLower() == "p")
            //            continue;
            //        string keyValue = string.Format("{0}={1}&", key, queryString[key]);
            //        url += keyValue;
            //    }
            //}
            #endregion

            //添加分页参数
            url += "p={0}";
            if (pageCount == 1)
                return string.Empty;
            StringBuilder sb = new StringBuilder();
            //前后对称个数
            int span = showPageCount / 2;

            int from, to;

            //导航中出现省略号
            if (pageCount > showPageCount + 1)
            {
                //省略号出现在左边
                if (currentPage >= pageCount - span)
                {
                    from = pageCount + 1 - showPageCount;
                    to = pageCount;

                    if (currentPage != 1)
                        sb.AppendFormat("<a href='" + url + "'> < </a>", currentPage - 1);
                    sb.AppendFormat("<a href='" + url + "'>{0}</a>", 1);
                    sb.Append("...");
                    sb.Append(ShowPageNavigation(currentPage, from, to));
                    if (currentPage != pageCount)
                        sb.AppendFormat("<a href='" + url + "'> > </a>", currentPage + 1);
                }
                else if (currentPage <= showPageCount - span)//省略号出现在右边
                {
                    from = 1;
                    to = showPageCount;

                    if (currentPage != 1)
                        sb.AppendFormat("<a href='" + url + "'> < </a>", currentPage - 1);
                    sb.Append(ShowPageNavigation(currentPage, from, to));
                    sb.Append("...");
                    sb.AppendFormat("<a href='" + url + "'>{0}</a>", pageCount);
                    sb.AppendFormat("<a href='" + url + "'> > </a>", currentPage + 1);
                }
                else   //省略号出现在两边
                {
                    from = currentPage - span;
                    to = currentPage + span;
                    sb.AppendFormat("<a href='" + url + "'> < </a>", currentPage - 1);
                    sb.Append("<a href='?p=1'>1</a>");
                    sb.Append("...");
                    sb.Append(ShowPageNavigation(currentPage, from, to));
                    sb.Append("...");
                    sb.AppendFormat("<a href='" + url + "'>{0}</a>", pageCount);
                    sb.AppendFormat("<a href='" + url + "'> > </a>", currentPage + 1);
                }
            }
            else
            {
                from = 1;
                to = pageCount;
                if (currentPage != 1)
                    sb.AppendFormat("<a href='" + url + "'> < </a>", currentPage - 1);
                sb.Append(ShowPageNavigation(currentPage, from, to));
                if (currentPage != pageCount)
                    sb.AppendFormat("<a href='" + url + "'> > </a>", currentPage + 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 创建导航按钮
        /// </summary>
        /// <param name="p">当前页</param>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <returns></returns>
        private static string ShowPageNavigation(int p, int from, int to)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = from; i <= to; i++)
            {
                if (i == p)
                {
                    sb.AppendFormat("<span class='current'>{0}</span>", i);
                }
                else
                {
                    sb.AppendFormat("<a href='" + url + "'>{0}</a>", i);
                }
            }
            return sb.ToString();
        }
    }
}
