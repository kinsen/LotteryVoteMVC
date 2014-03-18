using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LotteryVoteMVC.Core
{
    public static class ModelStateHelper
    {
        public static string ToErrorString(this ModelStateDictionary modelState, string splitSymbol = "<br/>")
        {
            var errors = modelState.Values.Where(it => it.Errors.Count > 0).Select(it => it.Errors);
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (var error in errors)
            {
                if (isFirst) isFirst = false;
                else sb.Append(splitSymbol);
                var errorMsg = string.Join(splitSymbol, error.Select(it => it.ErrorMessage).ToArray());
                sb.Append(errorMsg);
            }
            return sb.ToString();
        }
    }
}
