using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    public class MemberWinLost
    {
        public const string TABLENAME = "tb_MemberWinLost";
        public const string ID = "Id";
        public const string USERID = "UserId";
        public const string SHARERATE = "ShareRate";
        public const string COMPANYID = "CompanyId";
        public const string ORDERCOUNT = "OrderCount";
        public const string BETTURNOVER = "BetTurnover";
        public const string TOTALWINLOST = "TotalWinLost";
        public const string TOTALCOMMISSION = "TotalCommission";
        public const string WINLOST = "WinLost";
        public const string COMPANYWL = "CompanyWL";
        public const string CREATETIME = "CreateTime";

        public DataRow DataRow { get; set; }
        public int Id { get; set; }
        public int UserId { get; set; }
        public double ShareRate { get; set; }
        public int CompanyId { get; set; }
        public int OrderCount { get; set; }
        public decimal BetTurnover { get; set; }
        public decimal TotalWinLost { get; set; }
        public decimal TotalCommission { get; set; }
        public decimal WinLost { get; set; }
        public decimal CompanyWL { get; set; }
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
        /// <summary>
        /// 计算报表的时候tmp临时存放，避免反复查找
        /// </summary>
        /// <value>
        /// The share rate WL.
        /// </value>
        public ShareRateWL ShareRateWL { get; set; }
        public IEnumerable<MemberWLParentCommission> ParentCommission { get; set; }
    }

    public class MemberWLParentCommission
    {
        public const string TABLENAME = "tb_MemberParentCommission";
        public const string ID = "Id";
        public const string COMPANYID = "CompanyId";
        public const string USERID = "UserId";
        public const string ROLEID = "RoleId";
        public const string COMMISSION = "Commission";
        public const string CREATETIME = "CreateTime";
        public DataRow DataRow { get; set; }
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public Role Role
        {
            get
            {
                return (Role)RoleId;
            }
            set
            {
                RoleId = (int)value;
            }
        }
        public decimal Commission { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
