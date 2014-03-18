using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace LotteryVoteMVC.Utility.Validator
{
    /// <summary>
    /// Email验证器
    /// </summary>
    public class EmailAttribute : RegularExpressionAttribute
    {
        private const string M_ErrorMessage = "请输入一个正确的Email";
        public EmailAttribute() : base(@"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$") { }
        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrEmpty(ErrorMessage) && (string.IsNullOrEmpty(ErrorMessageResourceName) || ErrorMessageResourceType == null))
                ErrorMessage = M_ErrorMessage;
            return base.FormatErrorMessage(name);
        }
    }
}
