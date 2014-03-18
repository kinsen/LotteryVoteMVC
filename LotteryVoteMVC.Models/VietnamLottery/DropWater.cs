using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 跌水
    /// </summary>
    public class DropWater
    {
        public const string TABLENAME = "tb_DropWater";
        public const string DROPID = "DropId";
        public const string NUM = "Num";
        public const string GAMEPLAYWAYID = "GamePlayWayId";
        public const string DROPVALUE = "DropValue";
        public const string DROPTYPE = "DropType";
        public const string COMPANYTYPE = "CompanyType";
        public const string AMOUNT = "Amount";
        public const string CREATETIME = "CreateTime";

        public int DropId { get; set; }
        /// <summary>
        /// 跌水的号码.
        /// </summary>
        /// <value>
        /// The num.
        /// </value>
        public string Num { get; set; }
        /// <summary>
        /// 相关玩法.
        /// </summary>
        /// <value>
        /// The game play way id.
        /// </value>
        public int GamePlayWayId { get; set; }
        /// <summary>
        /// 具体跌水值.
        /// </summary>
        /// <value>
        /// The drop value.
        /// </value>
        public double DropValue { get; set; }
        /// <summary>
        /// 跌水类型.
        /// </summary>
        /// <value>
        /// The type of the drop.
        /// </value>
        public DropType DropType { get; set; }
        public int? CompanyType { get; set; }
        /// <summary>
        /// 跌水金额，也就是当下注金额（全局）大于该金额时，佣金跌水值增加本跌水值.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreateTime { get; set; }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}:{1},", DROPID, DropId);
            sb.AppendFormat("{0}:{1},", NUM, Num);
            sb.AppendFormat("{0}:{1},", GAMEPLAYWAYID, GamePlayWayId);
            sb.AppendFormat("{0}:{1},", DROPVALUE, DropValue);
            sb.AppendFormat("{0}:{1},", DROPTYPE, DropType);
            sb.AppendFormat("{0}:{1},", AMOUNT, Amount);
            sb.AppendFormat("{0}:{1}", CREATETIME, CreateTime);
            return sb.ToString();
        }
    }
}
