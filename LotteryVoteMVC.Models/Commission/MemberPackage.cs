using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// Guest所属的佣金组
    /// </summary>
    public class MemberPackage
    {
        public const string TABLENAME = "tb_MemberPackage";
        public const string USERID = "UserId";
        public const string GROUPID = "GroupId";
        public const string SPECIEID = "SpecieId";

        public int UserId { get; set; }
        public int GroupId { get; set; }
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
