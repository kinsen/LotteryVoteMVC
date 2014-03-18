using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 佣金分组
    /// </summary>
    public class CommissionGroup
    {
        public const string TABLENAME = "tb_CommissionGroup";
        public const string GROUPID = "GroupId";
        public const string GROUPNAME = "GroupName";
        public const string SPECIEID = "SpecieId";
        public const string CREATETIME = "CreateTime";

        public int GroupId { get; set; }
        public string GroupName { get; set; }
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
        public DateTime CreateTime { get; set; }
    }
}
