using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 下注上限限制（全局，所有订单）
    /// </summary>
    public class BetUpperLimit : ICloneable
    {
        public const string TABLENAME = "tb_BetUpperLimit";
        public const string LIMITID = "LimitId";
        public const string NUM = "Num";
        public const string COMPANYID = "CompanyId";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string DROPVALUE = "DropValue";
        public const string NEXTLIMIT = "NextLimit";
        public const string UPPERLLIMIT = "UpperLlimit";
        public const string TOTALBETAMOUNT = "TotalBetAmount";
        public const string STOPBET = "StopBet";
        public const string CREATETIME = "CreateTime";

        public int LimitId { get; set; }
        /// <summary>
        /// 限制号码
        /// </summary>
        /// <value>
        /// The num.
        /// </value>
        public string Num { get; set; }
        /// <summary>
        /// 开奖公司.
        /// </summary>
        /// <value>
        /// The company id.
        /// </value>
        public int CompanyId { get; set; }
        /// <summary>
        /// 游戏玩法.
        /// </summary>
        /// <value>
        /// The game play way id.
        /// </value>
        public int GamePlayWayId { get; set; }
        /// <summary>
        /// 当前跌水值.
        /// </summary>
        /// <value>
        /// The drop value.
        /// </value>
        public double DropValue { get; set; }
        /// <summary>
        /// 下个限制（用于自动跌水）.
        /// </summary>
        /// <value>
        /// The next limit.
        /// </value>
        public decimal NextLimit { get; set; }
        /// <summary>
        /// 最高限制.
        /// </summary>
        /// <value>
        /// The upper llimit.
        /// </value>
        public decimal UpperLlimit { get; set; }
        /// <summary>
        /// 指定条件（玩法，公司）限制号码总下注金额.
        /// </summary>
        /// <value>
        /// The total bet amount.
        /// </value>
        public decimal TotalBetAmount { get; set; }
        /// <summary>
        ///是否停止下注.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [stop bet]; otherwise, <c>false</c>.
        /// </value>
        public bool StopBet { get; set; }
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 内容是否被修改
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is change; otherwise, <c>false</c>.
        /// </value>
        public bool IsChange { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
