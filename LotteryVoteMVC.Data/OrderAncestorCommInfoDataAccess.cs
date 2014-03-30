using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class OrderAncestorCommInfoDataAccess : DataBase
    {
        public void Insert(IEnumerable<OrderAncestorCommInfo> commInfos)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(OrderAncestorCommInfo.ORDERID, typeof(int));
            dt.Columns.Add(OrderAncestorCommInfo.ROLEID, typeof(int));
            dt.Columns.Add(OrderAncestorCommInfo.COMMISSION, typeof(double));
            dt.Columns.Add(OrderAncestorCommInfo.COMMAMOUNT, typeof(decimal));
            foreach (var commInfo in commInfos)
            {
                var dr = dt.NewRow();
                dr[OrderAncestorCommInfo.ORDERID] = commInfo.OrderId;
                dr[OrderAncestorCommInfo.ROLEID] = commInfo.RoleId;
                dr[OrderAncestorCommInfo.COMMISSION] = commInfo.Commission;
                dr[OrderAncestorCommInfo.COMMAMOUNT] = commInfo.CommAmount;
                dt.Rows.Add(dr);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(OrderAncestorCommInfo.TABLENAME, dt);
        }

        public IEnumerable<OrderAncestorCommInfo> GetAncestorComms(int orderId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", OrderAncestorCommInfo.TABLENAME, OrderAncestorCommInfo.ORDERID);
            return base.ExecuteList<OrderAncestorCommInfo>(sql, new SqlParameter(OrderAncestorCommInfo.ORDERID, orderId));
        }
        public IEnumerable<OrderAncestorCommInfo> GetAncestorComms(User user, int companyId, BetStatus status, DateTime date)
        {
            string sql = string.Format(@"select oac.* from tb_BetOrder bo WITH (NOLOCK)
join tb_BetSheet bs on bs.SheetId=bo.SheetId
join tb_OrderAncestorCommInfo oac on oac.OrderId=bo.OrderId
where bs.{0}=@{0} and bo.{1}=@{1} and bo.{2}=@{2} and CAST(bo.{3} as DATE)=CAST(@{3} as DATE)", BetSheet.USERID, BetOrder.COMPANYID,
                 BetOrder.STATUS, BetOrder.CREATETIME);
            return base.ExecuteList<OrderAncestorCommInfo>(sql, new SqlParameter(BetSheet.USERID, user.UserId),
                new SqlParameter(BetOrder.COMPANYID, companyId),
                new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, date));
        }
    }
}
