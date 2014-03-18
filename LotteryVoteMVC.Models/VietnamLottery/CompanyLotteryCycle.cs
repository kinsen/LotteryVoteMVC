using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 公司开奖周期
    /// </summary>
    public class CompanyLotteryCycle
    {
        public const string TABLENAME = "tb_CompanyLotteryCycle";
        public const string CYCLEID = "CycleId";
        public const string COMPANYID = "CompanyId";
        public const string DAYOFWEEK = "DayOfWeek";

        public int CycleId { get; set; }
        public int CompanyId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }
}
