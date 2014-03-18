using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Utility.Validator;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 用户的具体佣金差
    /// </summary>
    public class CommissionValue
    {
        public const string TABLENAME = "tb_CommissionValue";
        public const string COMMISSIONID = "CommissionId";
        public const string GAMEID = "GameId";
        public const string COMPANYTYPEID = "CompanyTypeId";
        public const string COMM = "Comm";
        public const string ODDS = "Odds";

        public int CommissionId { get; set; }
        public int GameId { get; set; }
        public int CompanyTypeId { get; set; }
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
        [Required]
        [Numeric]
        public double Comm { get; set; }
        public double Odds { get; set; }
    }
}
