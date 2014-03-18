using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 下注限制（全局）默认金额
    /// </summary>
    public class DefaultUpperLimit 
    {
        public const string TABLENAME = "tb_DefaultUpperLimit";
        public const string LIMITID = "LimitId";
        public const string COMPANYTYPE = "CompanyType";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string LIMITAMOUNT = "LimitAmount";

        public int LimitId { get; set; }
        public CompanyType CompanyType { get; set; }
        /// <summary>
        /// 游戏玩法.
        /// </summary>
        /// <value>
        /// The game play way id.
        /// </value>
        public int GamePlayWayId { get; set; }
        /// <summary>
        /// 限制金额.
        /// </summary>
        /// <value>
        /// The limit amount.
        /// </value>
        public decimal LimitAmount { get; set; }
    }
}
