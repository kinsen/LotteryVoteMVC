using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace LotteryVoteMVC.Core.Bet
{
    internal abstract class DigitMultipleCalculator : MultipleCalculator
    {

        protected static IDictionary<int, Func<string, int, int>> CalFuncDic = new Dictionary<int, Func<string, int, int>>();

        protected override abstract IDictionary<int, IEnumerable<string>> LotteryResult { get; }

        public abstract override int Calculate(BetOrder order);

        protected virtual void CheckNumLength(string num, int len)
        {
            if (num.Length != len)
            {
                throw new ArgumentException("num's length not equals " + len + " Num:" + num);
            }
        }
        protected Func<string, int, int> TransToMethod(GamePlayWay gpw)
        {
            if (!CalFuncDic.ContainsKey(gpw.Id))
            {
                string wayName = gpw.PlayWay.ToString().Replace("+", "And");
                string methodName = "Calc" + wayName;
                var method = this.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                ParameterExpression numExp = Expression.Parameter(typeof(string), "num");
                ParameterExpression companyTypeExp = Expression.Parameter(typeof(int), "companyTypeId");
                LambdaExpression lambda = LambdaExpression.Lambda<Func<string, int, int>>(Expression.Call(Expression.Constant(this), method, numExp, companyTypeExp), numExp, companyTypeExp);
                CalFuncDic.Add(gpw.Id, lambda.Compile() as Func<string, int, int>);
            }
            return CalFuncDic[gpw.Id];
        }


        protected abstract int CalcHead(string num, int companyId);
        protected abstract int CalcLast(string num, int companyId);
        protected abstract int CalcHeadAndLast(string num, int companyId);
        protected abstract int CalcRoll(string num, int companyId);
        protected abstract int CalcRoll7(string num, int companyId);


        protected override abstract void ClearLotteryResult();
    }
}
