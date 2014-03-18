using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace LotteryVoteMVC.Utility.Validator
{
    /// <summary>
    /// 数字验证器，可以带小数点
    /// </summary>
    public class NumericAttribute : ValidationAttribute
    {
        private const string M_ErrorMessage = "必须是一个正数";
        //public NumericAttribute()
        //    : base(@"^\d+(\.\d+)?$") { }
        public override bool IsValid(object value)
        {
            string valStr = value.ToString();
            return Regex.IsMatch(valStr, @"^\d+(\.\d+)?$");
        }
        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrEmpty(ErrorMessage) && (string.IsNullOrEmpty(ErrorMessageResourceName) || ErrorMessageResourceType == null))
                ErrorMessage = M_ErrorMessage;
            return base.FormatErrorMessage(name);
        }
    }
}
