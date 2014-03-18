using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Utility.Validator;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 开奖结果记录
    /// </summary>
    public class LotteryRecord 
    {
        public const string TABLENAME = "tb_LotteryRecord";
        public const string RECORDID = "RecordId";
        public const string VALUE = "Value";
        public const string PERIODID = "PeriodId";

        public int RecordId { get; set; }
        /// <summary>
        /// 开奖记录值.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        [Required, Integer]
        public string Value { get; set; }
        /// <summary>
        /// 开奖记录所对应的期数.
        /// </summary>
        /// <value>
        /// The period.
        /// </value>
        public int PeriodId { get; set; }
    }
}
