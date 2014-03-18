using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace LotteryVoteMVC.Utility.Validator
{
    /// <summary>
    /// 整数验证
    /// </summary>
    public class IntegerAttribute : RegularExpressionAttribute
    {
        private const string M_ErrorMessage = "必须是整数!";
        public IntegerAttribute()
            : base(@"^\d+$") { }

        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrEmpty(ErrorMessage) && (string.IsNullOrEmpty(ErrorMessageResourceName) || ErrorMessageResourceType == null))
                ErrorMessage = M_ErrorMessage;
            return base.FormatErrorMessage(name);
        }
    }
}
