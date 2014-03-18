using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class OrderAncestorCommInfo
    {
        public const string TABLENAME = "tb_OrderAncestorCommInfo";
        public const string ORDERID = "OrderId";
        public const string ROLEID = "RoleId";
        public const string COMMISSION = "Commission";
        public const string COMMAMOUNT = "CommAmount";

        public int OrderId { get; set; }
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
        public double Commission { get; set; }
        public decimal CommAmount { get; set; }
    }
}
