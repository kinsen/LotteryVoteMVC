using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 开奖期次结果
    /// </summary>
    public class LotteryPeriod
    {
        public const string TABLENAME = "tb_LotteryPeriod";
        public const string PERIODID = "PeriodId";
        public const string COMPANYID = "CompanyId";
        public const string CREATEDATE = "CreateDate";

        public int PeriodId { get; set; }
        /// <summary>
        /// 开奖期次所对应的公司.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public LotteryCompany Company { get; set; }
        /// <summary>
        /// 开奖期次的日期.
        /// </summary>
        /// <value>
        /// The create date.
        /// </value>
        public DateTime CreateDate { get; set; }
    }
}
