using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class ShareRateWLDataAccess : DataBase
    {
        public void Insert(IEnumerable<ShareRateWL> results)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ShareRateWL.ID, typeof(int));
            dt.Columns.Add(ShareRateWL.USERID, typeof(int));
            dt.Columns.Add(ShareRateWL.SHARERATE, typeof(double));
            dt.Columns.Add(ShareRateWL.COMPANYID, typeof(int));
            dt.Columns.Add(ShareRateWL.ORDERCOUNT, typeof(int));
            dt.Columns.Add(ShareRateWL.BETTURNOVER, typeof(decimal));
            dt.Columns.Add(ShareRateWL.WINLOST, typeof(decimal));
            dt.Columns.Add(ShareRateWL.COMPANYWL, typeof(decimal));
            dt.Columns.Add(ShareRateWL.TOTALCOMM, typeof(decimal));
            dt.Columns.Add(ShareRateWL.TOTALWINLOST, typeof(decimal));
            foreach (var result in results)
            {
                var row = dt.NewRow();
                row[ShareRateWL.ID] = result.Id;
                row[ShareRateWL.USERID] = result.UserId;
                row[ShareRateWL.SHARERATE] = result.ShareRate;
                row[ShareRateWL.COMPANYID] = result.CompanyId;
                row[ShareRateWL.ORDERCOUNT] = result.OrderCount;
                row[ShareRateWL.BETTURNOVER] = result.BetTurnover;
                row[ShareRateWL.WINLOST] = result.WinLost;
                row[ShareRateWL.COMPANYWL] = result.CompanyWL;
                row[ShareRateWL.TOTALCOMM] = result.TotalComm;
                row[ShareRateWL.TOTALWINLOST] = result.TotalWinLost;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(ShareRateWL.TABLENAME, dt);
        }

        public void Delete(int companyId, DateTime date)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", ShareRateWL.TABLENAME, ShareRateWL.COMPANYID, ShareRateWL.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(ShareRateWL.COMPANYID, companyId),
                new SqlParameter(ShareRateWL.CREATETIME, date));
        }

        public IEnumerable<ShareRateWL> ListWinLost(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT UR.*,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120)) ORDER BY UR.USERID", ShareRateWL.TABLENAME, User.TABLENAME,
               User.USERID, ShareRateWL.USERID, User.PARENTID, ShareRateWL.CREATETIME);
            return base.ExecuteList<ShareRateWL>(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }

        public int CountSettleCountByCompany(int companyId, DateTime date)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)",
                ShareRateWL.TABLENAME, ShareRateWL.COMPANYID, ShareRateWL.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(ShareRateWL.COMPANYID, companyId),
                new SqlParameter(ShareRateWL.CREATETIME, date));
            return Convert.ToInt32(count);
        }
    }
}
