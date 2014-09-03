using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class MemberWinLostDataAccess : DataBase
    {
        public void Insert(IEnumerable<MemberWinLost> results)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MemberWinLost.ID, typeof(int));
            dt.Columns.Add(MemberWinLost.USERID, typeof(int));
            dt.Columns.Add(MemberWinLost.SHARERATE, typeof(double));
            dt.Columns.Add(MemberWinLost.COMPANYID, typeof(int));
            dt.Columns.Add(MemberWinLost.ORDERCOUNT, typeof(int));
            dt.Columns.Add(MemberWinLost.BETTURNOVER, typeof(decimal));
            dt.Columns.Add(MemberWinLost.TOTALWINLOST, typeof(decimal));
            dt.Columns.Add(MemberWinLost.TOTALCOMMISSION, typeof(decimal));
            dt.Columns.Add(MemberWinLost.WINLOST, typeof(decimal));
            dt.Columns.Add(MemberWinLost.COMPANYWL, typeof(decimal));
            foreach (var result in results)
            {
                var row = dt.NewRow();
                row[MemberWinLost.ID] = result.Id;
                row[MemberWinLost.USERID] = result.UserId;
                row[MemberWinLost.SHARERATE] = result.ShareRate;
                row[MemberWinLost.COMPANYID] = result.CompanyId;
                row[MemberWinLost.ORDERCOUNT] = result.OrderCount;
                row[MemberWinLost.BETTURNOVER] = result.BetTurnover;
                row[MemberWinLost.TOTALWINLOST] = result.TotalWinLost;
                row[MemberWinLost.TOTALCOMMISSION] = result.TotalCommission;
                row[MemberWinLost.WINLOST] = result.WinLost;
                row[MemberWinLost.COMPANYWL] = result.CompanyWL;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(MemberWinLost.TABLENAME, dt);
        }
        public void InsertParentCommission(IEnumerable<MemberWLParentCommission> results)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(MemberWLParentCommission.ID, typeof(int));
            dt.Columns.Add(MemberWLParentCommission.USERID, typeof(int));
            dt.Columns.Add(MemberWLParentCommission.COMPANYID, typeof(int));
            dt.Columns.Add(MemberWLParentCommission.ROLEID, typeof(int));
            dt.Columns.Add(MemberWLParentCommission.COMMISSION, typeof(decimal));
            foreach (var result in results)
            {
                var row = dt.NewRow();
                row[MemberWLParentCommission.ID] = result.Id;
                row[MemberWLParentCommission.USERID] = result.UserId;
                row[MemberWLParentCommission.COMPANYID] = result.CompanyId;
                row[MemberWLParentCommission.ROLEID] = result.RoleId;
                row[MemberWLParentCommission.COMMISSION] = result.Commission;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(MemberWLParentCommission.TABLENAME, dt);
        }

        public void Delete(int companyId, DateTime date)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", MemberWinLost.TABLENAME, MemberWinLost.COMPANYID, ShareRateWL.CREATETIME);
            string sql_2 = string.Format(@"DELETE {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", MemberWLParentCommission.TABLENAME, MemberWLParentCommission.COMPANYID, MemberWLParentCommission.CREATETIME);
            base.ExecuteWithTransaction(() =>
            {
                base.ExecuteNonQuery(sql, new SqlParameter(MemberWinLost.COMPANYID, companyId),
                new SqlParameter(MemberWinLost.CREATETIME, date));
                base.ExecuteNonQuery(sql_2, new SqlParameter(MemberWLParentCommission.COMPANYID, companyId),
                new SqlParameter(MemberWLParentCommission.CREATETIME, date));
            });

        }

        public IEnumerable<MemberWinLost> ListMemberWinLost(int userId, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT UR.*,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120)) ORDER BY UR.USERID", MemberWinLost.TABLENAME, User.TABLENAME,
               User.USERID, MemberWinLost.USERID, User.PARENTID, MemberWinLost.CREATETIME);
            return base.ExecuteList<MemberWinLost>(sql, new SqlParameter(User.PARENTID, userId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }

        public IEnumerable<MemberWLParentCommission> ListMemberWinLostAgentCommission(int userId, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT Max(SR.Id) as Id,SR.UserId,SR.RoleId,SUM(SR.Commission) As Commission,Max(SR.CreateTime) as CreateTime FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120))
GROUP BY SR.UserId,SR.RoleId", MemberWLParentCommission.TABLENAME, User.TABLENAME,
               User.USERID, MemberWLParentCommission.USERID, User.PARENTID, MemberWLParentCommission.CREATETIME);
            return base.ExecuteList<MemberWLParentCommission>(sql, new SqlParameter(User.PARENTID, userId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }

        public IEnumerable<MemberWinLost> ListMemberWinLost(DateTime fromDate, DateTime toDate, int startRow, int endRow)
        {
            string sql = string.Format(@"SELECT * FROM (
SELECT ROW_NUMBER() OVER(ORDER BY UR.USERID) AS RowNumber,UR.ParentId,ur.UserName,ur.RoleId,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE (CONVERT(char(10),SR.{4},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120))
) T
WHERE RowNumber BETWEEN {5} AND {6}", MemberWinLost.TABLENAME, User.TABLENAME,
               User.USERID, MemberWinLost.USERID, MemberWinLost.CREATETIME, startRow, endRow);
            return base.ExecuteList<MemberWinLost>(sql,
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }

        public int CountBetMember(DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"select COUNT(distinct UserId) from {0} where (CONVERT(char(10),CreateTime,120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120))", MemberWinLost.TABLENAME);
            var obj = base.ExecuteScalar(sql, new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate));
            return Convert.ToInt32(obj);
        }
    }
}
