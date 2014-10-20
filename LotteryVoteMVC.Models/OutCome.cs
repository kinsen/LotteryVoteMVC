using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    public class OutCome
    {
        public const string TABLENAME = "tb_Outcome";
        public const string ID = "Id";
        public const string USERID = "UserId";
        public const string COMPANYID = "CompanyId";
        public const string ORDERCOUNT = "OrderCount";
        public const string TURNOVER = "Turnover";
        public const string NETAMOUNT = "NetAmount";
        public const string JUNIORNETAMOUNT = "JuniorNetAmount";
        public const string TOTALWINLOST = "TotalWinLost";
        public const string JUNIORTOTALWINLOST = "JuniorTotalWinLost";
        public const string JUSTWIN = "JustWin";
        public const string JUNIORJUSTWIN = "JuniorJustWin";
        public const string CREATETIME = "CreateTime";

        public DataRow DataRow { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int OrderCount { get; set; }
        public decimal Turnover { get; set; }
        public decimal NetAmount { get; set; }
        /// <summary>
        /// 下级净金额
        /// </summary>
        /// <value>
        /// The junior net amount.
        /// </value>
        public decimal JuniorNetAmount { get; set; }
        public decimal TotalWinLost { get; set; }
        public decimal JuniorTotalWinLost { get; set; }
        /// <summary>
        /// 纯赢注单总额
        /// </summary>
        /// <value>
        /// The just win.
        /// </value>
        public decimal JustWin { get; set; }
        /// <summary>
        /// 下级纯赢注单总额
        /// </summary>
        /// <value>
        /// The junior just win.
        /// </value>
        public decimal JuniorJustWin { get; set; }
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

        public IList<BetOrder> WinOrders { get; set; }

    }
}
