using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Utility
{
    public static class MathHelper
    {
        /// <summary>
        /// 将百分数转换成小数
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="decimals">精确到小数点位数.</param>
        /// <returns></returns>
        public static double PercentageToDecimal(this double num, int decimals)
        {
            return Math.Round(num / 100, decimals, MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// 精确小数点后两位.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static double AccurateDecimalPointTwo(this double num)
        {
            return Math.Round(num, 2, MidpointRounding.AwayFromZero);
        }
        /// <summary>
        /// 输入一个数字，算出该数值有多少种排列可能.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static long CountPermutation(long num)
        {
            string number = num.ToString();
            IDictionary<char, int> rptNum = new Dictionary<char, int>();
            foreach (char ch in number)
            {
                if (rptNum.Keys.Where(it => it == ch).Count() > 0)
                    rptNum[ch] += 1;
                else
                    rptNum.Add(ch, 1);
            }
            long totalFac = Factorial(number.Length);
            foreach (var item in rptNum.Where(it => it.Value > 1))
            {
                totalFac /= Factorial(item.Value);
            }
            return totalFac;
        }
        /// <summary>
        /// 计算阶乘.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static long Factorial(int num)
        {
            if (num == 1) return num;
            return num * Factorial(--num);
        }
        /// <summary>
        /// 给定一个数字，计算出该数字的全排列.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        public static IList<long> GetFullArrangement(string num)
        {
            FullArrangementHelper fullArrangement = new FullArrangementHelper();
            return fullArrangement.GetFullArrangement(num);
        }
        public static IList<long> GetFullArrangement(int num)
        {
            FullArrangementHelper fullArrangement = new FullArrangementHelper();
            return fullArrangement.GetFullArrangement(num.ToString());
        }
        
    }
}
