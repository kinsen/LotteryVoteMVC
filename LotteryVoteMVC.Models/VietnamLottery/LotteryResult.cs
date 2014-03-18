using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 开奖结果
    /// </summary>
    public class LotteryResult
    {
        /// <summary>
        /// 开奖结果所对应公司.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public LotteryCompany Company { get; set; }
        /// <summary>
        /// 开奖记录.
        /// </summary>
        /// <value>
        /// The records.
        /// </value>
        public IEnumerable<LotteryRecord> Records { get; set; }
        /// <summary>
        /// 开奖日期.
        /// </summary>
        /// <value>
        /// The period date.
        /// </value>
        public DateTime PeriodDate { get; set; }

        public LotteryResult(LotteryCompany company, IEnumerable<LotteryRecord> records, DateTime periodDate)
        {
            this.Company = company;
            this.Records = records;
            this.PeriodDate = periodDate;
        }
    }
}
