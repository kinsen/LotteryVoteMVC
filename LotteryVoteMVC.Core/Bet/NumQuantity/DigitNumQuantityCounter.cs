using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core.Bet
{
    internal abstract class DigitNumQuantityCounter : NumQuantityCounter
    {
        private static object _lockHelper = new object();
        private IDictionary<string, Func<int>> _methodDic;
        protected IDictionary<string, Func<int>> MethodDic
        {
            get
            {
                if (_methodDic == null)
                    _methodDic = new Dictionary<string, Func<int>>();
                return _methodDic;
            }
        }

        public override int CountNumQuantity(string num, PlayWay playWay)
        {
            if (this.CompanySupportNumLen == null || this.CompanySupportNumLen.Count == 0)
                throw new ApplicationException("公司支持号码长度不能为空!");
            if (!num.IsNum())
                throw new ArgumentException(string.Format("num:{0} 不是数字", num));
            int quantity = TransToMethod(playWay)();
            return quantity;
        }

        protected abstract void CheckNumLength(string num);

        protected Func<int> TransToMethod(PlayWay playWay)
        {
            string wayName = playWay.ToString().Replace("+", "And");
            string methodName = "Get" + wayName;
            string key = string.Format("{0}_{1}", this.GetType().Name, methodName);
            Func<int> getMethod;
            if (!MethodDic.TryGetValue(key, out getMethod))
            {
                lock (_lockHelper)
                {
                    if (!MethodDic.TryGetValue(key, out getMethod))
                    {
                        var method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                        LambdaExpression lambda = LambdaExpression.Lambda<Func<int>>(Expression.Call(Expression.Constant(this), method));
                        getMethod = lambda.Compile() as Func<int>;
                        MethodDic.Add(key, getMethod);
                    }
                }
            }
            return getMethod;
        }
        protected abstract int GetHead();
        protected virtual int GetLast()
        {
            //尾始终为1
            return 1;
        }
        /// <summary>
        ///包组，全部.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        protected virtual int GetRoll()
        {
            return CompanySupportNumLen.Sum(it => it.Count);
        }
        /// <summary>
        /// Gets the roll7.
        /// </summary>
        /// <param name="num">The num.</param>
        /// <returns></returns>
        protected virtual int GetRoll7()
        {
            //包组7为前7个号码
            return 7;
        }
        protected virtual int GetHeadAndLast()
        {
            return GetHead() + GetLast();
        }

        public abstract override string GetRealyBetNum(string num);
    }
}
