using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core.Bet
{
    internal class A_BPL2MultipleCalculator : PL2MultipleCalculator
    {
        private const string LOTTERRESULT = "2DLotteryResult";

        public override int Calculate(Models.BetOrder order)
        {
            if (!order.Num.IsPL2())
                throw new ApplicationException(string.Format("num:{0} not a pl2 number", order.Num));
            int companyBId;
            if (!int.TryParse(order.Ext1, out companyBId))
                throw new ApplicationException(string.Format("A&BPL2的B公司Id为空或不符合规格，" + order.ToString()));
            return GetMultiple(order.Num, order.CompanyId, companyBId);
        }

        private int GetMultiple(string num, int companyId, int companyB)
        {
            var subNums = num.Split('#');
            var companyALotteryResult = GetCompanyLotteryResult(companyId);
            var companyBLotteryResult = GetCompanyLotteryResult(companyB);

            //两个数都相同的情况
            if (subNums[0] == subNums[1])
            {
                int multiple_a = companyALotteryResult.Where(it => it.EndsWith(subNums[0])).Count();
                int multiple_b = companyBLotteryResult.Where(it => it.EndsWith(subNums[0])).Count();
                return (multiple_a + multiple_b) / 2;
            }

            var tupleA = companyALotteryResult.Where(it => it.EndsWith(subNums[0])).ToList();
            var tupleB = companyALotteryResult.Where(it => it.EndsWith(subNums[1])).ToList();
            var tupleC = companyBLotteryResult.Where(it => it.EndsWith(subNums[0])).ToList();
            var tupleD = companyBLotteryResult.Where(it => it.EndsWith(subNums[1])).ToList();

            var numA_count = tupleA.Count + tupleC.Count;
            var numB_count = tupleB.Count + tupleD.Count;

            return numA_count < numB_count ? numA_count : numB_count;
        }
    }
}
