using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class OutcomeDataAccess : DataBase
    {

        public void Insert(IEnumerable<OutCome> results)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(OutCome.USERID, typeof(int));
            dt.Columns.Add(OutCome.COMPANYID, typeof(int));
            dt.Columns.Add(OutCome.ORDERCOUNT, typeof(int));          
            dt.Columns.Add(OutCome.TURNOVER, typeof(decimal));
            dt.Columns.Add(OutCome.TOTALWINLOST, typeof(decimal));
            dt.Columns.Add(OutCome.JUNIORTOTALWINLOST, typeof(decimal));
            dt.Columns.Add(OutCome.NETAMOUNT, typeof(decimal));
            dt.Columns.Add(OutCome.JUNIORNETAMOUNT, typeof(decimal));
            dt.Columns.Add(OutCome.JUSTWIN, typeof(decimal));
            dt.Columns.Add(OutCome.JUNIORJUSTWIN, typeof(decimal));
            foreach (var result in results)
            {
                var row = dt.NewRow();
                row[OutCome.USERID] = result.UserId;
                row[OutCome.COMPANYID] = result.CompanyId;
                row[OutCome.ORDERCOUNT] = result.OrderCount;
                row[OutCome.TURNOVER] = result.Turnover;
                row[OutCome.TOTALWINLOST] = result.TotalWinLost;
                row[OutCome.JUNIORTOTALWINLOST] = result.JuniorTotalWinLost;
                row[OutCome.NETAMOUNT] = result.NetAmount;
                row[OutCome.JUNIORNETAMOUNT] = result.JuniorNetAmount;
                row[OutCome.JUSTWIN] = result.JustWin;
                row[OutCome.JUNIORJUSTWIN] = result.JuniorJustWin;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(OutCome.TABLENAME, dt);
        }

        public void Delete(int companyId, DateTime date)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", OutCome.TABLENAME, OutCome.COMPANYID, OutCome.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(OutCome.COMPANYID, companyId),
                new SqlParameter(OutCome.CREATETIME, date));
        }

        public IEnumerable<OutCome> ListOutCome(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT UR.*,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120)) ORDER BY UR.USERID", OutCome.TABLENAME, User.TABLENAME,
               User.USERID, OutCome.USERID, User.PARENTID, OutCome.CREATETIME);
            return base.ExecuteList<OutCome>(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }
    }
}
