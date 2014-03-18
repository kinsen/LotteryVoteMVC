using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Linq.Expressions;
using System.Web.Routing;
using System.Web;
using LotteryVoteMVC.Utility.MVC;
using System.Reflection;

namespace LotteryVoteMVC.Utility
{
    public static class HtmlHelperEx
    {
        public static MvcHtmlString CheckBoxList<TModel>(this HtmlHelper<TModel> htmlHelper, string name, MultiSelectList multiSelectList, object htmlAttributes = null, object outHtmlAttributes = null)
        {
            IEnumerable<string> selectedValues = (IEnumerable<string>)multiSelectList.SelectedValues;
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(outHtmlAttributes), true);
            string attribute = ParseHtmlAttributes(htmlAttributes);
            //Add checkboxes
            foreach (SelectListItem item in multiSelectList)
            {
                divTag.InnerHtml += String.Format("<span><input type=\"checkbox\" name=\"{0}\" id=\"{0}_{1}\" " +
                                                    "value=\"{1}\" {2} {4}/><label for=\"{0}_{1}\">{3}</label></span>",
                                                    name,
                                                    item.Value,
                                                    selectedValues != null && selectedValues.Contains(item.Value) ? "checked=\"checked\"" : "",
                                                    item.Text,
                                                    attribute);
            }

            return MvcHtmlString.Create(divTag.ToString());
        }
        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty[]>> expression, MultiSelectList multiSelectList, object htmlAttributes = null, object outHtmlAttributes = null)
        {
            //Derive property name for checkbox name
            MemberExpression body = expression.Body as MemberExpression;
            string propertyName = body.Member.Name;

            //Get currently select values from the ViewData model
            //TProperty[] list = expression.Compile().Invoke(htmlHelper.ViewData.Model);

            //Convert selected value list to a List<string> for easy manipulation
            IEnumerable<string> selectedValues = multiSelectList.SelectedValues as IEnumerable<string>;

            //if (list != null)
            //{
            //    selectedValues = new List<TProperty>(list).ConvertAll<string>(delegate(TProperty i) { return i.ToString(); });
            //}

            //Create div
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(outHtmlAttributes), true);
            string attribute = ParseHtmlAttributes(htmlAttributes);
            //Add checkboxes
            foreach (SelectListItem item in multiSelectList)
            {
                divTag.InnerHtml += String.Format("<span><input type=\"checkbox\" name=\"{0}\" id=\"{0}_{1}\" " +
                                                    "value=\"{1}\" {2} {4} /><label for=\"{0}_{1}\">{3}</label></span>",
                                                    propertyName,
                                                    item.Value,
                                                    selectedValues != null && selectedValues.Contains(item.Value) ? "checked=\"checked\"" : "",
                                                    item.Text, attribute);
            }

            return MvcHtmlString.Create(divTag.ToString());
        }
        public static MvcHtmlString CheckBoxListFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty[]>> expression, IDictionary<string, MultiSelectList> sources, IEnumerable<string> selectedValues = null, object htmlAttributes = null)
        {
            MemberExpression body = expression.Body as MemberExpression;
            string propertyName = body.Member.Name;
            TagBuilder divTag = new TagBuilder("div");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            StringBuilder cb = new StringBuilder();
            cb.Append("<dl>");

            foreach (var item in sources)
            {
                StringBuilder sb = new StringBuilder();
                foreach (SelectListItem m in item.Value.Items)
                {
                    sb.AppendFormat("<span><input type=\"checkbox\" name=\"{0}\" id=\"{0}_{1}\" " +
                                                    "value=\"{1}\" {2} /><label for=\"{0}_{1}\">{3}</label></span>",
                                                    propertyName,
                                                    m.Value,
                                                    (selectedValues != null ? selectedValues.Contains(m.Value) : m.Selected) ? "checked=\"checked\"" : "",
                                                    m.Text);
                }
                cb.AppendFormat("<dt><input type=\"checkbox\" name=\"{0}\" id=\"{0}\" " +
                                                    "value=\"{0}\" checked=\"checked\" /><label for=\"{0}\">{0}</label><span class='advance'>More</span></dt><dd style='display:none'>{1}</dd>", item.Key, sb.ToString());
            }
            cb.Append("</dl>");
            divTag.InnerHtml += cb.ToString();
            return MvcHtmlString.Create(divTag.ToString());
        }

        public static MvcHtmlString Pager<T>(this HtmlHelper<PagedList<T>> htmlHelper, PagedList<T> source, object htmlAttributes = null)
        {
            TagBuilder divTag = new TagBuilder("div");
            divTag.Attributes.Add("id", "pager");
            divTag.MergeAttributes(new RouteValueDictionary(htmlAttributes), true);

            int fromCount = (source.PageIndex - 1) * source.PageSize;
            int toCount = fromCount + source.PageSize;
            toCount = source.TotalCount > toCount ? toCount : source.TotalCount;
            int pageCount = source.PageSize > 0 ? (source.TotalCount % source.PageSize == 0 ? source.TotalCount / source.PageSize : (source.TotalCount / source.PageSize) + 1) : source.PageSize;
            if (pageCount == 0)
                pageCount = 1;
            divTag.InnerHtml = PagerBuilder.Build(source.PageIndex, pageCount, 9);
            return MvcHtmlString.Create(divTag.ToString());
        }

        private static string ParseHtmlAttributes(object attributes)
        {
            var dicAttributes = attributes.ToDictionary();
            if (dicAttributes == null) return string.Empty;
            StringBuilder sb = new StringBuilder();
            foreach (var attr in dicAttributes)
            {
                sb.AppendFormat("{0}=\"{1}\" ", attr.Key, attr.Value);
            }
            return sb.ToString();
        }
        public static IDictionary<string, object> ToDictionary(this object data)
        {
            if (data == null) return null; // Or throw an ArgumentNullException if you want

            BindingFlags publicAttributes = BindingFlags.Public | BindingFlags.Instance;
            Dictionary<string, object> dictionary = new Dictionary<string, object>();

            foreach (PropertyInfo property in
                     data.GetType().GetProperties(publicAttributes))
            {
                if (property.CanRead)
                {
                    dictionary.Add(property.Name, property.GetValue(data, null));
                }
            }
            return dictionary;
        }

        #region  列表容器
        private const string idsToReuseKey = "__htmlPrefixScopeExtensions_IdsToReuse_";

        public static IDisposable BeginCollectionItem(this HtmlHelper html, string collectionName)
        {
            var idsToReuse = GetIdsToReuse(html.ViewContext.HttpContext, collectionName);
            string itemIndex = idsToReuse.Count > 0 ? idsToReuse.Dequeue() : Guid.NewGuid().ToString();

            // autocomplete="off" is needed to work around a very annoying Chrome behaviour whereby it reuses old values after the user clicks "Back", which causes the xyz.index and xyz[...] values to get out of sync.
            html.ViewContext.Writer.WriteLine(string.Format("<input type=\"hidden\" name=\"{0}.index\" autocomplete=\"off\" value=\"{1}\" />", collectionName, html.Encode(itemIndex)));

            return BeginHtmlFieldPrefixScope(html, string.Format("{0}[{1}]", collectionName, itemIndex));
        }

        public static IDisposable BeginHtmlFieldPrefixScope(this HtmlHelper html, string htmlFieldPrefix)
        {
            return new HtmlFieldPrefixScope(html.ViewData.TemplateInfo, htmlFieldPrefix);
        }

        private static Queue<string> GetIdsToReuse(HttpContextBase httpContext, string collectionName)
        {
            // We need to use the same sequence of IDs following a server-side validation failure,  
            // otherwise the framework won't render the validation error messages next to each item.
            string key = idsToReuseKey + collectionName;
            var queue = (Queue<string>)httpContext.Items[key];
            if (queue == null)
            {
                httpContext.Items[key] = queue = new Queue<string>();
                var previouslyUsedIds = httpContext.Request[collectionName + ".index"];
                if (!string.IsNullOrEmpty(previouslyUsedIds))
                    foreach (string previouslyUsedId in previouslyUsedIds.Split(','))
                        queue.Enqueue(previouslyUsedId);
            }
            return queue;
        }

        private class HtmlFieldPrefixScope : IDisposable
        {
            private readonly TemplateInfo templateInfo;
            private readonly string previousHtmlFieldPrefix;

            public HtmlFieldPrefixScope(TemplateInfo templateInfo, string htmlFieldPrefix)
            {
                this.templateInfo = templateInfo;

                previousHtmlFieldPrefix = templateInfo.HtmlFieldPrefix;
                templateInfo.HtmlFieldPrefix = htmlFieldPrefix;
            }

            public void Dispose()
            {
                templateInfo.HtmlFieldPrefix = previousHtmlFieldPrefix;
            }
        }
        #endregion
    }
}
