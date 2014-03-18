using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 结算结果
    /// </summary>
    public class SettleResult
    {
        public const string TABLENAME = "tb_SettleResult";
        public const string RESULTID = "ResultId";
        public const string USERID = "UserId";
        public const string COMPANYID = "CompanyId";
        public const string ORDERCOUNT = "OrderCount";
        public const string BETTURNOVER = "BetTurnover";
        public const string TOTALCOMMISSION = "TotalCommission";
        public const string SHARERATE = "ShareRate";
        public const string COMMISSION = "Commission";
        public const string WINLOST = "WinLost";
        public const string TOTALWINLOST = "TotalWinLost";
        public const string UPPERSHARERATE = "UpperShareRate";
        public const string UPPERWINLOST = "UpperWinLost";
        public const string UPPERCOMMISSION = "UpperCommission";
        public const string UPPERTOTALWINLOST = "UpperTotalWinLost";
        public const string COMPANYWINLOST = "CompanyWinLost";
        public const string CREATETIME = "CreateTime";

        public DataRow DataRow { get; set; }
        public int ResultId { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        /// <summary>
        /// 订单数
        /// </summary>
        /// <value>
        /// The order count.
        /// </value>
        public int OrderCount { get; set; }
        /// <summary>
        /// 下注额.
        /// </summary>
        /// <value>
        /// The bet turnover.
        /// </value>
        public decimal BetTurnover { get; set; }
        /// <summary>
        /// 总佣金.
        /// </summary>
        /// <value>
        /// The total commission.
        /// </value>
        public decimal TotalCommission { get; set; }
        /// <summary>
        /// 分成.
        /// </summary>
        /// <value>
        /// The share rate.
        /// </value>
        public double ShareRate { get; set; }
        /// <summary>
        /// 佣金
        /// </summary>
        /// <value>
        /// The commission.
        /// </value>
        public decimal Commission { get; set; }
        /// <summary>
        /// 输赢.
        /// </summary>
        /// <value>
        /// The win lost.
        /// </value>
        public decimal WinLost { get; set; }
        /// <summary>
        /// 总输赢.
        /// </summary>
        /// <value>
        /// The total win lost.
        /// </value>
        public decimal TotalWinLost { get; set; }
        /// <summary>
        /// 上级分成.
        /// </summary>
        /// <value>
        /// The upper share rate.
        /// </value>
        public double UpperShareRate { get; set; }
        /// <summary>
        /// 上级输赢
        /// </summary>
        /// <value>
        /// The upper win lost.
        /// </value>
        public decimal UpperWinLost { get; set; }
        /// <summary>
        /// 上级佣金
        /// </summary>
        /// <value>
        /// The upper commission.
        /// </value>
        public decimal UpperCommission { get; set; }
        /// <summary>
        /// 上级总输赢
        /// </summary>
        /// <value>
        /// The upper total win lost.
        /// </value>
        public decimal UpperTotalWinLost { get; set; }
        /// <summary>
        /// 公司输赢
        /// </summary>
        /// <value>
        /// The company win lost.
        /// </value>
        public decimal CompanyWinLost { get; set; }
        private User _user;
        /// <summary>
        /// 注单隶属者.
        /// </summary>
        /// <value>
        /// The user.
        /// </value>
        public User User
        {
            get
            {
                if (_user == null && DataRow != null)
                    _user = ModelParser<User>.ParseModel(DataRow);
                return _user;
            }
            set
            {
                _user = value;
            }
        }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
