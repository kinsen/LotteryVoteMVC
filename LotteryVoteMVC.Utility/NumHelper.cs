using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LotteryVoteMVC.Utility
{
    public static class NumHelper
    {
        public static bool IsNum(this string num)
        {
            Regex rule = new Regex(@"^\d+$");
            return rule.IsMatch(num);
        }
        public static bool IsFloat(this string num)
        {
            Regex rule = new Regex(@"^\d+(\.\d+)?$");
            return rule.IsMatch(num);
        }
        /// <summary>
        /// 是否号码组(用逗号分割，例如23,32,12)
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>
        ///   <c>true</c> if [is num array] [the specified num]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNumArray(this string num)
        {
            Regex rule = new Regex(@"^(\d{2,}\,{0,1})+$");
            return (!num.IsNum()) && rule.IsMatch(num);
        }
        public static bool IsPL2(this string num)
        {
            Regex rule = new Regex(@"^\d{2}#\d{2}$");
            return rule.IsMatch(num);
        }
        public static bool IsPL3(this string num)
        {
            Regex rule = new Regex(@"^\d{2}#\d{2}#\d{2}$");
            return rule.IsMatch(num);
        }
        /// <summary>
        /// 是否一个范围的号码,仅支持2D（例如23-32）
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>
        ///   <c>true</c> if [is range num] [the specified num]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRangeNum(this string num)
        {
            Regex rule = new Regex(@"^\d{2,}-\d{2,}$");
            return rule.IsMatch(num);
        }
        /// <summary>
        /// 是否连号(例如--12)
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>
        ///   <c>true</c> if [is batter num] [the specified num]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBatterNum(this string num)
        {
            Regex rule = new Regex(@"^-+\d+$");
            return rule.IsMatch(num);
        }
        /// <summary>
        /// 是否2D连号；例如*8
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static bool Is2DBatterNum(this string num)
        {
            Regex rule = new Regex(@"^((\d\*)|(\*\d))$");
            return rule.IsMatch(num);
        }
        /// <summary>
        ///是否3D连号；例如*18
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static bool Is3DBatterNum(this string num)
        {
            Regex rule = new Regex(@"^(\*\d{2})|(\d\*\d)|(\d{2}\*)$");
            return rule.IsMatch(num);
        }
        /// <summary>
        /// 是否星星连号，例如*23,**33,23*2,3*3,53*
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns>
        ///   <c>true</c> if [is start batter num] [the specified num]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStartBatterNum(this string num)
        {
            Regex rule = new Regex(@"^(\*+\d+)|(\d+\*+\d+)|(\d+\*+)$");
            return rule.IsMatch(num);
        }
        public static IList<string> Parse2DBatterNums(this string num)
        {
            if (!num.Is2DBatterNum()) throw new ArgumentException();
            List<string> rangNumList = new List<string>();
            for (int i = 0; i < 10; i++)
                rangNumList.Add(num.Replace("*", i.ToString()));
            return rangNumList;
        }
        public static IList<string> Parse3DBatterNums(this string num)
        {
            if (!num.Is3DBatterNum()) throw new ArgumentException();
            List<string> rangNumList = new List<string>();
            var subNum = num.Split('*');
            for (int i = 0; i < 10; i++)
                rangNumList.Add(string.Join(i.ToString(), subNum));
            return rangNumList;
        }
        public static bool IsLetter(this string text)
        {
            Regex rule = new Regex(@"^[a-zA-Z]+$");
            return rule.IsMatch(text);
        }
        /// <summary>
        /// 获取连号（例如--12）
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="maxLength">号码的最大长度.</param>
        /// <returns></returns>
        public static IList<string> ParseBatterNums(this string num, int maxLength)
        {
            if (!num.IsBatterNum()) throw new ArgumentException();
            if (num.Length > maxLength) throw new ArgumentException("Num 超出最大长度限制");
            List<string> rangNumList = new List<string>();
            int numIndex = num.LastIndexOf('-') + 1;
            string n = num.Substring(numIndex);
            int rangeNumLen = num.Length - n.Length;
            for (int i = 0; i < Math.Pow(10, rangeNumLen); i++)
            {
                rangNumList.Add(i.ToString("D" + rangeNumLen) + n);
            }
            return rangNumList;
        }
        /// <summary>
        /// 获取连号(例如（22-33）)
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static IList<string> ParseRangeNums(this string num)
        {
            if (!num.IsRangeNum())
                throw new ArgumentException("号码不是连号,Num:" + num);
            var nums = num.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
            if (nums.Length > 2 || nums.Length < 2)
                throw new ArgumentException("号码格式不正确!Num:" + num);
            int[] numArr = new int[2];
            for (int i = 0; i < nums.Length; i++)
                numArr[i] = int.Parse(nums[i]);

            if (numArr[0] > numArr[1])
                throw new ArgumentException("号码格式不正确!Num:" + num);
            List<string> numList = new List<string>();
            for (int i = numArr[0]; i <= numArr[1]; i++)
                numList.Add(i.ToString("D" + nums[0].Length));
            return numList;
        }
        public static IList<string> ParseStartBatterNums(this string num, int maxLength = 5)
        {
            if (!num.IsStartBatterNum())
                throw new ArgumentException("号码不是星连号,Num:" + num);
            if (num.Length > maxLength) throw new ArgumentException("Num 超出最大长度限制");
            int index = 0;
            string numFormat = string.Empty;
            foreach (var n in num)
            {
                if (n == '*')
                    numFormat += "{" + index++ + "}";
                else
                    numFormat += n;
            }
            object[] dimension = new object[index];
            for (int i = 0; i < index; i++)
                dimension[i] = 0;
            bool count = true;
            List<string> numList = new List<string>();
            int lastIndex = index - 1;
            Action<int> checkDimen = null;
            checkDimen = it =>
            {
                int dvalue = (int)dimension[it];
                if (dvalue >= 10)
                {
                    if (it == 0) count = false;
                    else
                    {
                        dimension[it - 1] = (int)dimension[it - 1] + 1;
                        dimension[it] = 0;
                        checkDimen(it - 1);
                    }
                }
            };
            while (count)
            {
                numList.Add(string.Format(numFormat, dimension));
                dimension[lastIndex] = (int)dimension[lastIndex] + 1;
                checkDimen(lastIndex);
            }
            return numList;
        }
    }
}
