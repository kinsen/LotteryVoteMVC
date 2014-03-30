using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class RateGroupGameBetLimit
    {
        public const string TABLENAME = "tb_RateGroupGameBetLimit";
        public const string GROUPID = "GroupId";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string COMPANYTYPE = "CompanyType";
        public const string LIMITVALUE = "LimitValue";

        public int GroupId { get; set; }
        public int GamePlayWayId { get; set; }
        public CompanyType CompanyType { get; set; }
        public decimal LimitValue { get; set; }
    }
}
