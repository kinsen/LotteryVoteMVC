using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 用户投注自动跌水
    /// </summary>
    public class BetAutoDropWater
    {
        public const string TABLENAME = "tb_BetAutoDropWater";
        public const string BETDROPID = "BetDropId";
        public const string COMPANYTYPEID = "CompanyTypeId";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string AMOUNT = "Amount";
        public const string DROPVALUE = "DropValue";
        public const string CREATETIME = "CreateTime";

        public int BetDropId { get; set; }
        public int CompanyTypeId { get; set; }
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
        public int GamePlayWayId { get; set; }
        public decimal Amount { get; set; }
        public double DropValue { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
