using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 游戏注单最大限制
    /// </summary>
    public class GameBetLimit
    {
        public const string TABLENAME = "tb_GameBetLimit";
        public const string USERID = "UserId";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string COMPANYTYPE = "CompanyType";
        public const string LIMITVALUE = "LimitValue";

        public int UserId { get; set; }
        public int GamePlayWayId { get; set; }
        public CompanyType CompanyType { get; set; }
        public decimal LimitValue { get; set; }
    }
}
