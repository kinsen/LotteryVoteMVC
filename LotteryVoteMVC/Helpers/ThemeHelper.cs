using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;

namespace LotteryVoteMVC.Helpers
{
    public static class ThemeHelper
    {
        public static IList<SelectListItem> GetSelectedList(string selectedTheme)
        {
            // TODO: should get from web.config or database
            var list = new List<SelectListItem>
                            {new SelectListItem {Text = "Default", Value = "Default"},
                            new SelectListItem {Text = "Red", Value = "Red"}};

            foreach (var selectListItem in list.Where(selectListItem => selectListItem.Value.Equals(selectedTheme)))
            {
                selectListItem.Selected = true;
            }

            return list;
        }

        public static string GetCssWithTheme(ViewContext viewContext)
        {
            var themeName = ConfigurationManager.AppSettings["themeName"];

            return string.Format("~/Content/themes/{0}/Site.css", viewContext.HttpContext.Items[themeName]);
        }
    }
}