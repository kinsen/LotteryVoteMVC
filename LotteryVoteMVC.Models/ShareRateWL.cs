using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    public class ShareRateWL
    {
        public const string TABLENAME = "tb_ShareRateWL";
        public const string ID = "Id";
        public const string USERID = "UserId";
        public const string SHARERATE = "ShareRate";
        public const string COMPANYID = "CompanyId";
        public const string ORDERCOUNT = "OrderCount";
        public const string BETTURNOVER = "BetTurnover";
        public const string WINLOST = "WinLost";
        public const string COMPANYWL = "CompanyWL";
        public const string TOTALCOMM = "TotalComm";
        public const string TOTALWINLOST = "TotalWinLost";
        public const string CREATETIME = "CreateTime";

        public DataRow DataRow { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public double ShareRate { get; set; }
        public int OrderCount { get; set; }
        public decimal BetTurnover { get; set; }
        public decimal WinLost { get; set; }
        public decimal CompanyWL { get; set; }
        /// <summary>
        /// 本级所占佣金（真实佣金，没有任何额外计算）
        /// </summary>
        /// <value>
        /// The commission.
        /// </value>
        public decimal Commission { get; set; }
        public decimal TotalComm { get; set; }
        public decimal TotalWinLost { get; set; }
        /// <summary>
        /// 会员输赢=会员总输赢（注单输赢累加）-会员佣金
        /// </summary>
        /// <value>
        /// The member win lost.
        /// </value>
        public decimal MemberWinLost { get; set; }
        /// <summary>
        /// 总下注额（下层会员下注额累加）
        /// </summary>
        /// <value>
        /// The sum turnover.
        /// </value>
        public decimal SumTurnover { get; set; }
        /// <summary>
        /// 总输赢（下层会员输赢累加）
        /// </summary>
        /// <value>
        /// The sum win lost.
        /// </value>
        public decimal SumWinLost { get; set; }
        public DateTime CreateTime { get; set; }
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
    }
}
