using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class SettleResultDataAccess : DataBase
    {
        public void Insert(IEnumerable<SettleResult> results)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(SettleResult.RESULTID, typeof(int));
            dt.Columns.Add(SettleResult.USERID, typeof(int));
            dt.Columns.Add(SettleResult.COMPANYID, typeof(int));
            dt.Columns.Add(SettleResult.ORDERCOUNT, typeof(int));
            dt.Columns.Add(SettleResult.BETTURNOVER, typeof(decimal));
            dt.Columns.Add(SettleResult.TOTALCOMMISSION, typeof(decimal));
            dt.Columns.Add(SettleResult.SHARERATE, typeof(double));
            dt.Columns.Add(SettleResult.COMMISSION, typeof(double));
            dt.Columns.Add(SettleResult.WINLOST, typeof(decimal));
            dt.Columns.Add(SettleResult.TOTALWINLOST, typeof(decimal));
            dt.Columns.Add(SettleResult.UPPERSHARERATE, typeof(double));
            dt.Columns.Add(SettleResult.UPPERWINLOST, typeof(decimal));
            dt.Columns.Add(SettleResult.UPPERCOMMISSION, typeof(decimal));
            dt.Columns.Add(SettleResult.UPPERTOTALWINLOST, typeof(decimal));
            dt.Columns.Add(SettleResult.COMPANYWINLOST, typeof(decimal));
            foreach (var result in results)
            {
                var row = dt.NewRow();
                row[SettleResult.RESULTID] = result.ResultId;
                row[SettleResult.USERID] = result.UserId;
                row[SettleResult.COMPANYID] = result.CompanyId;
                row[SettleResult.ORDERCOUNT] = result.OrderCount;
                row[SettleResult.BETTURNOVER] = result.BetTurnover;
                row[SettleResult.TOTALCOMMISSION] = result.TotalCommission;
                row[SettleResult.SHARERATE] = result.ShareRate;
                row[SettleResult.COMMISSION] = result.Commission;
                row[SettleResult.WINLOST] = result.WinLost;
                row[SettleResult.TOTALWINLOST] = result.TotalWinLost;
                row[SettleResult.UPPERSHARERATE] = result.UpperShareRate;
                row[SettleResult.UPPERWINLOST] = result.UpperWinLost;
                row[SettleResult.UPPERCOMMISSION] = result.UpperCommission;
                row[SettleResult.UPPERTOTALWINLOST] = result.UpperTotalWinLost;
                row[SettleResult.COMPANYWINLOST] = result.CompanyWinLost;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(SettleResult.TABLENAME, dt);
        }
        public void Delete(int companyId, DateTime date)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", SettleResult.TABLENAME, SettleResult.COMPANYID, SettleResult.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(SettleResult.COMPANYID, companyId),
                new SqlParameter(SettleResult.CREATETIME, date));
        }

        public IEnumerable<SettleResult> ListSettleResult(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT UR.*,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120)) ORDER BY UR.USERID", SettleResult.TABLENAME, User.TABLENAME,
               User.USERID, SettleResult.USERID, User.PARENTID, SettleResult.CREATETIME);
            return base.ExecuteList<SettleResult>(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }
        public IEnumerable<SettleResult> ListMemberSettleResult(User user, DateTime fromDate, DateTime toDate, string orderField, string orderType, int startRow, int endRow)
        {
            string sql = string.Format(@"if object_id('tempdb..#user') is not null   drop table #user
create table #user 
(
	UserId int not null,
	UserName varchar(50) not null,
	RoleId int not null,
	ParentId int not null
);
insert into #user exec dbo.SelectUserFamilyWithRole {7},5;with result as(
select sr.UserId,u.UserName as Name, SUM(sr.OrderCount) as OrderCount,SUM(sr.BetTurnover) as BetTurnover,SUM(sr.TotalCommission) as TotalCommission,
SUM(sr.WinLost) as WinLost,SUM(sr.Commission) as Commission,SUM(sr.TotalWinLost) as TotalWinLost
  from tb_SettleResult sr
join #user u on u.UserId=sr.UserId
join tb_UserInfo ui on ui.UserId=sr.UserId
where u.RoleId={6} and CAST(sr.CreateTime as DATE) between CAST(@{0} as DATE) and CAST(@{1} as DATE)
group by sr.UserId,u.UserName
)
select * from(
	select ROW_NUMBER() over(order by {2} {3}) as RowNumber,* from result
)T
where RowNumber between {4} and {5}", "From", "To", orderField, orderType, startRow, endRow, (int)Role.Guest, user.UserId);
            return base.ExecuteList<SettleResult>(sql, new SqlParameter("From", fromDate), new SqlParameter("To", toDate));
        }
        public IEnumerable<FullStatement> GetFullStatement(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format("exec {0} @UserId,@FromDate,@ToDate", FullStatement.PROCEDURE);
            return base.ExecuteList<FullStatement>(sql, new SqlParameter("UserId", user.UserId), new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate));
        }
        public FullStatement GetUserFullStatement(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = "exec GetUserFullState @UserId,@FromDate,@ToDate";
            return base.ExecuteModel<FullStatement>(sql, new SqlParameter("UserId", user.UserId), new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate));
        }
        /// <summary>
        /// 自身结算记录
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<SettleResult> GetSettleResult(User user, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"SELECT UR.*,SR.*,ui.Name FROM {0} SR
JOIN {1} UR ON UR.{2}=SR.{3}
JOIN tb_UserInfo ui on ui.UserId=UR.userId
WHERE UR.{4}=@{4} AND (CONVERT(char(10),SR.{5},120) BETWEEN CONVERT(char(10),@FromDate,120) AND CONVERT(char(10),@ToDate,120)) ORDER BY UR.USERID", SettleResult.TABLENAME, User.TABLENAME,
               User.USERID, SettleResult.USERID, User.USERID, SettleResult.CREATETIME);
            return base.ExecuteList<SettleResult>(sql, new SqlParameter(User.USERID, user.UserId),
                new SqlParameter("FromDate", fromDate),
                new SqlParameter("ToDate", toDate));
        }

        /// <summary>
        /// 计算指定日期，公司的结算数目
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public int CountSettleCountByCompany(int companyId, DateTime date)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)",
                SettleResult.TABLENAME, SettleResult.COMPANYID, SettleResult.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(SettleResult.COMPANYID, companyId),
                new SqlParameter(SettleResult.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        public int CountBetMember(DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"select COUNT(u.UserId) from tb_SettleResult sr 
join tb_User u on u.UserId=sr.UserId
where u.RoleId=5 and CAST(CreateTime as DATE) between CAST(@{0} as DATE) and CAST(@{1} as DATE)", "From", "To");
            var obj = base.ExecuteScalar(sql, new SqlParameter("From", fromDate), new SqlParameter("To", toDate));
            return Convert.ToInt32(obj);
        }
        public int CountBetMember(User user,DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"
;WITH CTE AS
(
SELECT * FROM tb_User  WHERE UserId=@UserId
UNION ALL
SELECT B.* FROM tb_User AS B,CTE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
)
select COUNT(u.UserId) from tb_SettleResult sr 
join CTE u on u.UserId=sr.UserId
where u.RoleId=5 and CAST(CreateTime as DATE) between CAST(@{0} as DATE) and CAST(@{1} as DATE)", "From", "To");
            var obj = base.ExecuteScalar(sql, new SqlParameter("From", fromDate), new SqlParameter("To", toDate),new SqlParameter("UserId",user.UserId));
            return Convert.ToInt32(obj);
        } 
    }
}
