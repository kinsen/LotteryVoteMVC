using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Utility.Validator;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 具体佣金
    /// </summary>
    public class ConcreteCommission
    {
        public const string TABLENAME = "tb_ConcreteCommission";
        public const string GROUPID = "GroupId";
        public const string GAMEID = "GameId";
        public const string COMPANYTYPEID = "CompanyTypeId";
        public const string COMMISSION = "Commission";
        public const string ODDS = "Odds";

        public int GroupId { get; set; }
        public int GameId { get; set; }
        public int CompanyTypeId { get; set; }
        [Required]
        public GameType GameType
        {
            get
            {
                return (GameType)GameId;
            }
            set
            {
                GameId = (int)value;
            }
        }
        [Required]
        public CompanyType CompanyType
        {
            get
            {
                return (CompanyType)CompanyTypeId;
            }
            set
            {
                CompanyTypeId = (int)value;
            }
        }
        /// <summary>
        /// 佣金
        /// </summary>
        /// <value>
        /// The commission.
        /// </value>
        [Required]
        [Numeric]
        public double Commission { get; set; }
        /// <summary>
        /// 赔率.
        /// </summary>
        /// <value>
        /// The odds.
        /// </value>
        [Required]
        [Numeric]
        public double Odds { get; set; }
    }
}
