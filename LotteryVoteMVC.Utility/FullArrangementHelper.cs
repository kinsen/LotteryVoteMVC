using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Utility
{
    /// <summary>
    /// 全排列算法类
    /// </summary>
    public class FullArrangementHelper
    {
        private int _length;
        public IList<long> Result { get; private set; }

        public FullArrangementHelper()
        {
            Result = new List<long>();
        }

        public IList<long> GetFullArrangement(string num)
        {
            _length = num.Length;

            FullArrangement(num.OrderBy(it => it).ToArray(), 0);
            return Result;
        }

        /// <summary>
        /// 计算全排列算法
        /// </summary>
        /// <param name="nums">要计算全排列的数字.</param>
        /// <param name="pos">开始计算排列的位置，例如，现在的数字是1234，如果pos为0，就代表计算这四位的全排列，1的下标为0，
        /// 如果为1，则计算后3位的全排列，依次下推.</param>
        private void FullArrangement(char[] nums, int pos)
        {
            //将现在的数字添加到结果中
            Result.Add(ConvertToNum(nums));

            //最大是数字的长度-2是因为按照下标计算，此处pos-1是因为后续的步骤中需要+1来对相邻的两个数字做比较
            for (int i = _length - 2; i > pos - 1; i--)
                NextArrangement(nums, i);
        }

        /// <summary>
        /// 计算下一轮全排列
        /// </summary>
        /// <param name="nums">数字的分解数组.</param>
        /// <param name="pos">开始计算排列的位置.</param>
        private void NextArrangement(char[] nums, int pos)
        {
            char[] cop = new char[_length];
            //根据下标依次计算数字的全排列，实际就是大排列都由小排列一步步扩大
            for (int i = pos + 1; i < _length; i++)
                if (nums[i] > nums[pos] && nums[i] != nums[i - 1])
                {
                    for (int t = 0; t < _length; t++)
                        cop[t] = nums[t];
                    //交换数字位
                    for (int j = i; j > pos; j--)
                    {
                        char temp = cop[j];
                        cop[j] = cop[j - 1];
                        cop[j - 1] = temp;
                    }
                    FullArrangement(cop, pos + 1);
                }
        }

        private long ConvertToNum(char[] nums)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var n in nums)
                sb.Append(n);
            return long.Parse(sb.ToString());
        }
    }
}
