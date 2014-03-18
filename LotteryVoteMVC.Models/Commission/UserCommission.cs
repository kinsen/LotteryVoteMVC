using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 用户佣金设置
    /// </summary>
    public class UserCommission 
    {
        public const string TABLENAME = "tb_UserCommission";
        public const string COMMISSIONID = "CommissionId";
        public const string USERID = "UserId";
        public const string SPECIEID = "SpecieId";

        public int CommissionId { get; set; }
        public int UserId { get; set; }
        public int SpecieId { get; set; }
        public LotterySpecies Specie
        {
            get
            {
                return (LotterySpecies)SpecieId;
            }
            set
            {
                SpecieId = (int)value;
            }
        }
    }
}
