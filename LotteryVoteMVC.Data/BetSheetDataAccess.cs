using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class BetSheetDataAccess : DataBase
    {
        public void Insert(BetSheet sheet)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6}) VALUES (@{1},@{2},@{3},@{4},@{5},@{6}) SELECT SCOPE_IDENTITY()",
                BetSheet.TABLENAME, BetSheet.USERID, BetSheet.NUM, BetSheet.STATUS, BetSheet.IPADDRESS, BetSheet.BETCOMPANY, BetSheet.BETAMOUNT);
            object id = base.ExecuteScalar(sql, new SqlParameter(BetSheet.USERID, sheet.UserId),
                new SqlParameter(BetSheet.NUM, sheet.Num),
                new SqlParameter(BetSheet.STATUS, (int)sheet.Status),
                new SqlParameter(BetSheet.IPADDRESS, sheet.IPAddress),
                new SqlParameter(BetSheet.BETCOMPANY, sheet.BetCompany),
                new SqlParameter(BetSheet.BETAMOUNT, sheet.BetAmount));
            sheet.SheetId = Convert.ToInt32(id);
        }
        public void Update(IEnumerable<BetSheet> sheets)
        {
            string clearSql = string.Format("DELETE {0}", BetSheet.TEMP_TABLENAME);
            base.ExecuteNonQuery(clearSql);

            DataTable dt = new DataTable();
            dt.Columns.Add(BetSheet.SHEETID, typeof(int));
            dt.Columns.Add(BetSheet.USERID, typeof(int));
            dt.Columns.Add(BetSheet.NUM, typeof(string));
            dt.Columns.Add(BetSheet.STATUS, typeof(int));
            dt.Columns.Add(BetSheet.IPADDRESS, typeof(string));
            dt.Columns.Add(BetSheet.BETCOMPANY, typeof(string));
            dt.Columns.Add(BetSheet.BETAMOUNT, typeof(string));
            dt.Columns.Add(BetSheet.CREATETIME, typeof(DateTime));
            foreach (var sheet in sheets)
            {
                var row = dt.NewRow();
                row[BetSheet.SHEETID] = sheet.SheetId;
                row[BetSheet.USERID] = sheet.UserId;
                row[BetSheet.NUM] = sheet.Num;
                row[BetSheet.STATUS] = sheet.Status;
                row[BetSheet.IPADDRESS] = sheet.IPAddress;
                row[BetSheet.BETCOMPANY] = sheet.BetCompany;
                row[BetSheet.BETAMOUNT] = sheet.BetAmount;
                row[BetSheet.CREATETIME] = sheet.CreateTime;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(BetSheet.TEMP_TABLENAME, dt);

            string sql = @"update tb_BetSheet set tb_BetSheet.Status=tb_BetSheetTemp.Status
from tb_BetSheetTemp
where tb_BetSheet.SheetId=tb_BetSheetTemp.SheetId";
            base.ExecuteNonQuery(sql);
        }
        public void UpdateBetSheetStatus(int sheetId, BetStatus status)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1} WHERE {2}=@{2}", BetSheet.TABLENAME, BetSheet.STATUS, BetSheet.SHEETID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetSheet.STATUS, (int)status),
                new SqlParameter(BetSheet.SHEETID, sheetId));
        }
        /// <summary>
        /// 删除指定日期之前的数据
        /// </summary>
        /// <param name="date">The date.</param>
        public void RemoveOld(DateTime date)
        {
            string sql = string.Format(@"DELETE {0} where CAST({1} AS DATE)<CAST(@{1} as DATE)", BetSheet.TABLENAME, BetSheet.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(BetSheet.CREATETIME, date));
        }
        /// <summary>
        /// 备份
        /// </summary>
        /// <param name="backupDate">The backup date.</param>
        public void BackupSheet(DateTime backupDate)
        {
            string sql = string.Format(@"select * into {0}_{1} from {0} bs where CAST(bs.{2} as DATE)<CAST(@{2} as DATE)", BetSheet.TABLENAME, backupDate.ToString("yyyy_MM_dd"), BetSheet.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(BetSheet.CREATETIME, backupDate));
        }

        #region Select
        public BetSheet GetBetSheet(int sheetId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", BetSheet.TABLENAME, BetSheet.SHEETID);
            return base.ExecuteModel<BetSheet>(sql, new SqlParameter(BetSheet.SHEETID, sheetId));
        }

        /// <summary>
        /// 获取指定用户下注列表
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="status">sheet状态</param>
        /// <param name="date">The date.</param>
        /// <param name="startRow">开始获取数据的行数.</param>
        /// <param name="endRow">结束获取数据的行数.</param>
        /// <returns></returns>
        public IEnumerable<BetSheet> ListBetSheet(User user, BetStatus[] status, DateTime date, int startRow, int endRow)
        {
            string sql = string.Format(@";WITH CTE AS(select bs.*,bo.{2},bo.{3},bo.{4} from {0} bs
left join (select {5} as {6},SUM({2}) as {2},
Sum({3}) as {3},SUM({4}) as {4} from {1} where status=1 group by {5}) bo
on bo.{5}=bs.{6}
where bs.{7}=@{7} and CONVERT(char(10),bs.{8},120)=CONVERT(char(10),@{8},120) AND bs.Status in {11})
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {6} DESC) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {9} AND {10}", BetSheet.TABLENAME, BetOrder.TABLENAME, BetOrder.TURNOVER, BetOrder.COMMISSION, BetOrder.NETAMOUNT,
                    BetOrder.SHEETID, BetSheet.SHEETID, BetSheet.USERID, BetSheet.CREATETIME, startRow, endRow, getStatusInStatement(status));
            return base.ExecuteList<BetSheet>(sql, new SqlParameter(BetSheet.USERID, user.UserId),
                new SqlParameter(BetSheet.CREATETIME, date));
        }
        /// <summary>
        /// 获取下注列表
        /// </summary>
        /// <param name="status">sheet状态</param>
        /// <param name="date">The date.</param>
        /// <param name="startRow">开始获取数据的行数.</param>
        /// <param name="endRow">结束获取数据的行数.</param>
        /// <returns></returns>
        public IEnumerable<BetSheet> ListBetSheet(BetStatus[] status, DateTime date, int startRow, int endRow)
        {
            string statusStatement = getStatusInStatement(status);

            string sql = string.Format(@";WITH Sheet AS(
	select * from
	(
		SELECT ROW_NUMBER() OVER(ORDER BY SheetId DESC) AS RowNumber,bs.*,u.RoleId,u.ParentId,u.UserName from {0} bs
		join tb_User u on u.UserId=bs.UserId
		where bs.Status in {1} And CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)
	)T 
	where RowNumber BETWEEN {3} AND {4}
)
select * from Sheet bs
inner join (select SheetId as SheetId,SUM(Turnover) as Turnover,
Sum(Commission) as Commission,SUM(NetAmount) as NetAmount from tb_BetOrder where SheetId in (select SheetId from Sheet)  group by SheetId) bo
on bo.SheetId=bs.SheetId", BetSheet.TABLENAME, statusStatement, BetSheet.CREATETIME, startRow, endRow);

            return base.ExecuteList<BetSheet>(sql, new SqlParameter(BetSheet.CREATETIME, date));
        }
        /// <summary>
        /// 获取指定用户(包括下属)下注列表
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public IEnumerable<BetSheet> GetSheets(User user, BetStatus status, DateTime date, int startRow, int endRow)
        {
            return base.ExecuteList<BetSheet>(System.Data.CommandType.StoredProcedure, "SelectChildBetSheet",
                new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, date),
                new SqlParameter("StartPageIndex", startRow),
                new SqlParameter("EndPageIndex", endRow));
        }
        public IEnumerable<BetSheet> GetSheets(User user, BetStatus status, DateTime date, string num, int startRow, int endRow)
        {
            string sql = string.Format(@";WITH CTE AS(select bs.*,u.UserName from tb_BetSheet bs
join tb_User u on u.UserId=bs.UserId
where bs.{0}=@{0} and u.{1}=@{1} and bs.{7}=@{7} and CAST(bs.{2} as DATE)=CAST(@{2} as DATE))
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {3} {4}) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {5} AND {6}", BetSheet.STATUS, User.USERID, BetSheet.CREATETIME, BetSheet.SHEETID, "ASC", startRow, endRow, BetSheet.NUM);
            return base.ExecuteList<BetSheet>(sql, new SqlParameter(BetSheet.STATUS, (int)status),
                new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(BetSheet.CREATETIME, date),
                new SqlParameter(BetSheet.NUM, num));
        }
        /// <summary>
        /// 根据用户名查找注单
        /// </summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <param name="sortField">The sort field.</param>
        /// <param name="sortType">Type of the sort.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public IEnumerable<BetSheet> GetSheets(string userName, BetStatus status, DateTime date, string sortField, string sortType, int startRow, int endRow)
        {
            string sql = string.Format(@";WITH CTE AS(select bs.*,u.UserName from tb_BetSheet bs
join tb_User u on u.UserId=bs.UserId
where bs.{0}=@{0} and u.{1}=@{1} and CAST(bs.{2} as DATE)=CAST(@{2} as DATE))
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {3} {4}) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {5} AND {6}", BetSheet.STATUS, User.USERNAME, BetSheet.CREATETIME, sortField, sortType, startRow, endRow);
            return base.ExecuteList<BetSheet>(sql, new SqlParameter(BetSheet.STATUS, (int)status),
                new SqlParameter(User.USERNAME, userName),
                new SqlParameter(BetSheet.CREATETIME, date));
        }
        /// <summary>
        /// 根据公司和日期获取所有能结算的大注单
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public IEnumerable<BetSheet> GetCanSettleSheetByCompany(int companyId, DateTime date)
        {
            string sql = string.Format(@"select * from tb_BetSheet bs WITH (NOLOCK)
where (
(select COUNT(0) from tb_BetOrder bo1 WITH (NOLOCK) where bo1.SheetId=bs.SheetId)=
(select COUNT(0) from tb_BetOrder bo2 WITH (NOLOCK) where bo2.SheetId=bs.SheetId and bo2.{0}=@{0}))
and DATEDIFF(DD,bs.CreateTime,GETDATE())=0
UNION
select * from tb_BetSheet bs WITH (NOLOCK)
where ((select COUNT(0) from tb_BetOrder bo3 WITH (NOLOCK) where bo3.SheetId=bs.SheetId and bo3.{0}<>@{0} and bo3.{1}=@{1})=0)
and DATEDIFF(DD,bs.{2},@{2})=0", BetOrder.COMPANYID, BetOrder.STATUS, BetSheet.CREATETIME);
            return base.ExecuteDataTable(sql, new SqlParameter(BetOrder.COMPANYID, companyId),
                new SqlParameter(BetOrder.STATUS, (int)BetStatus.Valid),
                new SqlParameter(BetSheet.CREATETIME, date)).AsEnumerable().Select(it => new BetSheet(it));
        }

        #endregion

        #region Count
        /// <summary>
        /// 计算某条Sheet中指定状态的注单数量
        /// </summary>
        /// <param name="sheetdId">The sheetd id.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public int CountOrder(int sheetdId, BetStatus status)
        {
            string sql = string.Format(@"select * from {0} where {1}=@{1} and {2}=@{2}", BetOrder.TABLENAME, BetOrder.SHEETID, BetOrder.STATUS);
            object count = base.ExecuteScalar(sql, new SqlParameter(BetOrder.SHEETID, sheetdId),
                new SqlParameter(BetOrder.STATUS, (int)status));
            return Convert.ToInt32(count);
        }
        /// <summary>
        /// 计算指定日期，状态的注单数
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public int CountBetSheet(DateTime date, params BetStatus[] status)
        {
            string statusStatement = getStatusInStatement(status);

            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE Status in {1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", BetSheet.TABLENAME, statusStatement, BetSheet.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(BetSheet.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        public int CountBetSheet(User user, DateTime date, params BetStatus[] status)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120) AND Status in {3}", BetSheet.TABLENAME, BetSheet.USERID, BetSheet.CREATETIME, getStatusInStatement(status));
            object count = base.ExecuteScalar(sql, new SqlParameter(BetSheet.USERID, user.UserId),
                new SqlParameter(BetSheet.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        /// <summary>
        /// 计算指定日期，状态的注单数(包含下属用户)
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public int CountSheets(User user, BetStatus status, DateTime date)
        {
            string sql = string.Format(@";WITH USERTABLE AS
(
SELECT * FROM tb_User  WHERE UserId=@{0}
UNION ALL
SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
)
SELECT COUNT(0) FROM USERTABLE ut
join tb_BetSheet bs on bs.UserId=ut.UserId
where ut.{1}=@{1} and bs.{2}=@{2} and CONVERT(char(10),{3},120)=CONVERT(char(10),@{3},120)", User.USERID,
                                User.ROLEID, BetSheet.STATUS, BetSheet.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(User.ROLEID, (int)Role.Guest),
                new SqlParameter(BetSheet.STATUS, (int)status),
                new SqlParameter(BetSheet.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        public int CountSheets(User user, BetStatus status, DateTime date, string num)
        {
            string sql = string.Format(@";WITH USERTABLE AS
(
SELECT * FROM tb_User  WHERE UserId=@{0}
UNION ALL
SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
)
SELECT COUNT(0) FROM USERTABLE ut
join tb_BetSheet bs on bs.UserId=ut.UserId
where ut.{1}=@{1} and bs.{2}=@{2} and CONVERT(char(10),{3},120)=CONVERT(char(10),@{3},120) and bs.{4}=@{4}", User.USERID,
                                User.ROLEID, BetSheet.STATUS, BetSheet.CREATETIME, BetSheet.NUM);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(User.ROLEID, (int)Role.Guest),
                new SqlParameter(BetSheet.STATUS, (int)status),
                new SqlParameter(BetSheet.CREATETIME, date),
                new SqlParameter(BetSheet.NUM, num));
            return Convert.ToInt32(count);
        }
        #endregion

        /// <summary>
        /// 获取状态的In语句内容
        /// </summary>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        private string getStatusInStatement(params BetStatus[] status)
        {
            string statusStatement = "(";
            bool isFirst = true;
            foreach (var state in status)
            {
                if (isFirst) isFirst = false;
                else statusStatement += ",";
                statusStatement += (int)state;
            }
            statusStatement += ")";
            return statusStatement;
        }
    }
}
