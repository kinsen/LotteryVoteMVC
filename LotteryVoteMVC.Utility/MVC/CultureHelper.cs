using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace LotteryVoteMVC.Utility
{
    public static class CultureHelper
    {
        private static string[] M_supportLanguages = new[] { "zh", "en", "vi" };
        private static NumberFormatInfo _numberFormat;
        private static DateTimeFormatInfo _datetimeFormat;
        public static void InitCulture()
        {
            var langStr = GetLanguageFromClient();
            if (string.IsNullOrEmpty(langStr)) return;
            SetLanguage(langStr);
        }

        public static string GetCurrentCulture()
        {
            CookieParser parser = new CookieParser("Language");
            string languageStr = parser.GetValueToString();
            if (string.IsNullOrEmpty(languageStr)) languageStr = "zh";  //默认是中文
            return languageStr;
        }

        public static string ToFormat(this decimal amount)
        {
            return amount.ToString(NumberFormatInfo.InvariantInfo);
        }
        public static string ToFormat(this double amount)
        {
            return amount.ToString(NumberFormatInfo.InvariantInfo);
        }
        public static decimal ToDecimal(this string amount)
        {
            decimal returnVal;
            decimal.TryParse(amount, NumberStyles.None, NumberFormatInfo.InvariantInfo, out returnVal);
            return returnVal;
        }
        public static string ToFormat(this DateTime date)
        {
            return string.Format("{0:yyyy-MM-dd HH:mm:ss}", date);
        }
        public static string ToFormat(this DateTime? date)
        {
            if (date.HasValue)
                return date.Value.ToFormat();
            return string.Empty;
        }
        public static string ToPercent(this decimal amount)
        {
            return string.Format("{0:P0}", amount);
        }
        public static string ToPercent(this double input)
        {
            return string.Format("{0:P0}", input);
        }

        internal static void SetLanguage(string languageStr)
        {
            var culture = CultureInfo.CreateSpecificCulture(languageStr);
            //将数字化格式设置为默认的格式
            culture.NumberFormat = new NumberFormatInfo();
            culture.NumberFormat = GetNumberFormatInfo();
            culture.DateTimeFormat = GetDateTimeFormatInfo();
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
        private static NumberFormatInfo GetNumberFormatInfo()
        {
            if (_numberFormat == null)
            {
                //将数字化格式设置为默认的格式
                _numberFormat = new NumberFormatInfo();
                _numberFormat.CurrencyPositivePattern = 1;
                _numberFormat.NumberDecimalDigits = _numberFormat.CurrencyDecimalDigits = 2;
                _numberFormat.NumberGroupSizes = _numberFormat.CurrencyGroupSizes = new[] { 3 };
                _numberFormat.NumberGroupSeparator = _numberFormat.CurrencyGroupSeparator = ",";
                _numberFormat.NumberDecimalSeparator = _numberFormat.CurrencyDecimalSeparator = ".";
            }
            return _numberFormat;
        }
        private static DateTimeFormatInfo GetDateTimeFormatInfo()
        {
            if (_datetimeFormat == null)
            {
                _datetimeFormat = CultureInfo.CreateSpecificCulture("zh").DateTimeFormat;
            }
            return _datetimeFormat;
        }
        private static string GetLanguageFromClient()
        {
            CookieParser parser = new CookieParser("Language");
            string languageStr = parser.GetValueToString();
            if (!string.IsNullOrEmpty(languageStr) && IsSupportLanguage(languageStr))
                return languageStr;
            return string.Empty;
        }
        private static bool IsSupportLanguage(string languageStr)
        {
            return M_supportLanguages.Contains(languageStr);
        }
    }
}
