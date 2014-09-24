using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;
using System.Data;

namespace LotteryVoteMVC.Data
{
    public class BetOrderDataAccess : DataBase
    {
        public void Insert(BetOrder order)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}) VALUES 
(@{1},@{2},@{3},@{4},@{5},@{6},@{7},@{8},@{9},@{10},@{11},@{12},@{13}) SELECT Scope_Identity()", BetOrder.TABLENAME, BetOrder.SHEETID,
            BetOrder.NUM, BetOrder.GAMEPLAYWAYID, BetOrder.COMPANYID, BetOrder.AMOUNT, BetOrder.TURNOVER, BetOrder.ODDS,
            BetOrder.COMMISSION, BetOrder.NET, BetOrder.NETAMOUNT, BetOrder.STATUS, BetOrder.DROPWATER, BetOrder.EXT1);
            object id = base.ExecuteScalar(sql, new SqlParameter(BetOrder.SHEETID, order.SheetId),
                new SqlParameter(BetOrder.NUM, order.Num),
                new SqlParameter(BetOrder.GAMEPLAYWAYID, order.GamePlayWayId),
                new SqlParameter(BetOrder.COMPANYID, order.CompanyId),
                new SqlParameter(BetOrder.AMOUNT, order.Amount),
                new SqlParameter(BetOrder.TURNOVER, order.Turnover),
                new SqlParameter(BetOrder.ODDS, order.Odds),
                new SqlParameter(BetOrder.COMMISSION, order.Commission),
                new SqlParameter(BetOrder.NET, order.Net),
                new SqlParameter(BetOrder.NETAMOUNT, order.NetAmount),
                new SqlParameter(BetOrder.STATUS, order.Status),
                new SqlParameter(BetOrder.DROPWATER, order.DropWater),
                new SqlParameter(BetOrder.EXT1, order.Ext1));
            order.OrderId = Convert.ToInt32(id);
        }
        public void Update(IEnumerable<BetOrder> orders)
        {
            string clearSql = string.Format("DELETE {0}", BetOrder.TEMP_TABLENAME);
            base.ExecuteNonQuery(clearSql);

            DataTable dt = new DataTable();
            dt.Columns.Add(BetOrder.ORDERID, typeof(int));
            dt.Columns.Add(BetOrder.SHEETID, typeof(int));
            dt.Columns.Add(BetOrder.COMPANYID, typeof(int));
            dt.Columns.Add(BetOrder.NUM, typeof(string));
            dt.Columns.Add(BetOrder.GAMEPLAYWAYID, typeof(int));
            dt.Columns.Add(BetOrder.AMOUNT, typeof(decimal));
            dt.Columns.Add(BetOrder.TURNOVER, typeof(decimal));
            dt.Columns.Add(BetOrder.COMMISSION, typeof(decimal));
            dt.Columns.Add(BetOrder.ODDS, typeof(decimal));
            dt.Columns.Add(BetOrder.NET, typeof(decimal));
            dt.Columns.Add(BetOrder.NETAMOUNT, typeof(decimal));
            dt.Columns.Add(BetOrder.STATUS, typeof(int));
            dt.Columns.Add(BetOrder.DROPWATER, typeof(double));
            dt.Columns.Add(BetOrder.DRAWRESULT, typeof(decimal));
            dt.Columns.Add(BetOrder.EXT1, typeof(string));
            dt.Columns.Add(BetOrder.CREATETIME, typeof(DateTime));
            foreach (var order in orders)
            {
                var row = dt.NewRow();
                row[BetOrder.ORDERID] = order.OrderId;
                row[BetOrder.SHEETID] = order.SheetId;
                row[BetOrder.COMPANYID] = order.CompanyId;
                row[BetOrder.NUM] = order.Num;
                row[BetOrder.GAMEPLAYWAYID] = order.GamePlayWayId;
                row[BetOrder.AMOUNT] = order.Amount;
                row[BetOrder.TURNOVER] = order.Turnover;
                row[BetOrder.COMMISSION] = order.Commission;
                row[BetOrder.ODDS] = order.Odds;
                row[BetOrder.NET] = order.Net;
                row[BetOrder.NETAMOUNT] = order.NetAmount;
                row[BetOrder.STATUS] = order.Status;
                row[BetOrder.DROPWATER] = order.DropWater;
                row[BetOrder.DRAWRESULT] = order.DrawResult;
                row[BetOrder.EXT1] = order.Ext1;
                row[BetOrder.CREATETIME] = order.CreateTime;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(BetOrder.TEMP_TABLENAME, dt);

            string sql = @"update tb_BetOrder set tb_BetOrder.Status=tb_BetOrderTemp.Status,
tb_BetOrder.DrawResult=tb_BetOrderTemp.DrawResult,
tb_BetOrder.DropWater=tb_BetOrderTemp.DropWater
from tb_BetOrderTemp
where tb_BetOrder.OrderId=tb_BetOrderTemp.OrderId";
            base.ExecuteNonQuery(sql);
        }
        public void Update(BetOrder order)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1},{2}=@{2},{3}=@{3} WHERE {4}=@{4}", BetOrder.TABLENAME, BetOrder.STATUS, BetOrder.DROPWATER, BetOrder.DRAWRESULT, BetOrder.ORDERID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetOrder.STATUS, (int)order.Status),
                new SqlParameter(BetOrder.DROPWATER, order.DropWater),
                new SqlParameter(BetOrder.DRAWRESULT, order.DrawResult),
                new SqlParameter(BetOrder.ORDERID, order.OrderId));
        }
        public void UpdateState(int orderId, BetStatus status)
        {
            string updateCancelTime = (status == BetStatus.Invalid) ? ", " + BetOrder.CANCELTIME + "=getdate()" : string.Empty;
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1} {2} WHERE {3}=@{3}", BetOrder.TABLENAME,
                BetOrder.STATUS, updateCancelTime, BetOrder.ORDERID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.ORDERID, orderId));
        }
        /// <summary>
        /// 修改Sheet所有Order的状态
        /// </summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="status">The status.</param>
        public void UpdateStateBySheet(int sheetId, BetStatus status)
        {
            string condition = status == BetStatus.Invalid ? string.Format(@", {0}=getdate()", BetOrder.CANCELTIME) : string.Empty;
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1} {3} WHERE {2}=@{2}", BetOrder.TABLENAME, BetOrder.STATUS, BetOrder.SHEETID, condition);
            base.ExecuteNonQuery(sql, new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.SHEETID, sheetId));
        }
        /// <summary>
        /// 删除指定日期之前的数据
        /// </summary>
        /// <param name="date">The date.</param>
        public void RemoveOld(DateTime date)
        {
            string sql = string.Format(@"DELETE {0} where CAST({1} AS DATE)<CAST(@{1} as DATE)", BetOrder.TABLENAME, BetOrder.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(BetOrder.CREATETIME, date));
        }
        /// <summary>
        /// 备份
        /// </summary>
        /// <param name="backupDate">The backup date.</param>
        public void BackupOrder(DateTime backupDate)
        {
            string sql = string.Format(@"select * into {0}_{1} from {0} bo where CAST(bo.{2} as DATE)<CAST(@{2} as DATE)", BetOrder.TABLENAME, backupDate.ToString("yyyy_MM_dd"), BetOrder.CREATETIME);
            base.ExecuteNonQuery(sql, new SqlParameter(BetOrder.CREATETIME, backupDate));
        }

        #region Select
        public BetOrder GetBetOrder(int orderId)
        {
            string sql = string.Format(@"SELECT bo.*,bs.{4} FROM {0} bo JOIN {2} bs on bs.{3}=bo.{3} WHERE bo.{1}=@{1}", BetOrder.TABLENAME, BetOrder.ORDERID,
                BetSheet.TABLENAME, BetSheet.SHEETID, BetSheet.USERID);
            return base.ExecuteModel<BetOrder>(sql, new SqlParameter(BetOrder.ORDERID, orderId));
        }
        public IEnumerable<BetOrder> GetOrders(int userId, BetStatus status, DateTime day)
        {
            string sql = string.Format(@"SELECT BO.* FROM {0} BO
JOIN {1} BS ON BS.{2}=BO.{3}
WHERE BO.{4}=@{4} AND CONVERT(char(10),BO.{5},120)=CONVERT(char(10),@{5},120) AND BS.{6}=@{6}",
                 BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID,
                 BetOrder.STATUS, BetOrder.CREATETIME, BetSheet.USERID);
            return base.ExecuteDataTable(sql, new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, day),
                new SqlParameter(BetSheet.USERID, userId)).AsEnumerable().Select(it => new BetOrder(it));
        }
        /// <summary>
        /// 获取指定用户的注单之和
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="status">The status.</param>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetOrdersTotals(User user, BetStatus status, DateTime day)
        {
            string sql = string.Format(@"select bo.Num,bo.CompanyId,bo.GamePlayWayId,SUM(bo.amount)as Amount,SUM(bo.Turnover)as Turnover,
SUM(bo.Commission)as Commission,SUM(bo.NetAmount)as NetAmount
 from tb_BetOrder bo
join tb_BetSheet bs on bs.SheetId=bo.SheetId
where bs.{0}=@{0} and bo.{1}=@{1} and CAST(bo.{2} as DATE)=CAST(@{2} as DATE)
group by bo.CompanyId,bo.GamePlayWayId,bo.Num", BetSheet.USERID, BetOrder.STATUS, BetOrder.CREATETIME);
            return base.ExecuteDataTable(sql, new SqlParameter(BetSheet.USERID, user.UserId),
                new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, day))
                .AsEnumerable().Select(it => new BetOrder
                {
                    Num = it.Field<string>(BetOrder.NUM),
                    CompanyId = it.Field<int>(BetOrder.COMPANYID),
                    GamePlayWayId = it.Field<int>(BetOrder.GAMEPLAYWAYID),
                    Amount = it.Field<decimal>(BetOrder.AMOUNT),
                    Turnover = it.Field<decimal>(BetOrder.TURNOVER),
                    Commission = it.Field<decimal>(BetOrder.COMMISSION),
                    NetAmount = it.Field<decimal>(BetOrder.NETAMOUNT)
                });
        }
        /// <summary>
        /// 获取指定用户今日的注单
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="date">The date.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetOrders(int userId, DateTime date, int startRow, int endRow)
        {
            string sql = string.Format(@";WITH CTE AS(
select bo.* from {0} bo
join {1} bs on bs.{2}=bo.{3}
where CONVERT(char(10),bo.{4},120)=CONVERT(char(10),@{4},120) AND bs.{5}=@{5})
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {6} desc) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {7} AND {8}", BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID, BetOrder.CREATETIME, BetSheet.USERID, BetOrder.ORDERID,
                                 startRow, endRow);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetSheet.USERID, userId),
                new SqlParameter(BetOrder.CREATETIME, date));
        }
        public IEnumerable<NumAmountRanking> GetNumAmountRankings(int userId, int companyId, int gpwId, string num, int startRow, int endRow)
        {
            string sql = string.Format(@";WITH USERTABLE AS
	(
		SELECT * FROM tb_User  WHERE {4}=@{4}
		UNION ALL
		SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
	)
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY SUM(bo.Amount) desc) AS RowNumber,u.UserName,SUM(bo.Amount) AS Amount 
     FROM {0} bo
     join {1} bs on bs.{2}=bo.{2}
     join {3} u on u.{4}=bs.{4}
     where CAST(bs.CreateTime as DATE)=CAST(GETDATE() AS DATE) and bs.Status=1
     and bo.CompanyId=@CompanyId and bo.GamePlayWayId=@GamePlayWayId and bo.Num=@Num
     group by u.UserName
) T
WHERE RowNumber BETWEEN {5} AND {6}", BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, "USERTABLE", User.USERID, startRow, endRow);

            return ExecuteList<NumAmountRanking>(sql, new SqlParameter("CompanyId", companyId),
                new SqlParameter("GamePlayWayId", gpwId),
                new SqlParameter("Num", num),
                new SqlParameter("UserId", userId));
        }
        public IEnumerable<BetOrder> GetOrders(int userId, int companyId, BetStatus status, DateTime date)
        {
            string sql = string.Format(@"SELECT BO.*,BS.UserId FROM {0} BO
JOIN {1} BS ON BS.{2}=BO.{3}
WHERE BO.{4}=@{4} AND CAST(BO.{5} AS DATE)=CAST(@{5} AS DATE) AND BS.{6}=@{6} AND BO.{7}=@{7}",
                BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID,
                BetOrder.STATUS, BetOrder.CREATETIME, BetSheet.USERID, BetOrder.COMPANYID);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, date),
                new SqlParameter(BetSheet.USERID, userId),
                new SqlParameter(BetOrder.COMPANYID, companyId));
        }
        public IEnumerable<BetOrder> GetOrdersBySheet(int sheetId)
        {
            string sql = string.Format(@"SELECT B.*,S.UserId FROM {0} B JOIN {2} S ON S.{1}=B.{1} WHERE B.{1}=@{1}", BetOrder.TABLENAME, BetOrder.SHEETID, BetSheet.TABLENAME);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetOrder.SHEETID, sheetId));
        }
        public IEnumerable<BetOrder> GetOrdersBySheet(int sheetId, int startRow, int endRow)
        {
            string sql = string.Format(@"SELECT T.*,u.UserName FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {2} desc) AS RowNumber,bo.*,bs.UserId 
     FROM {0} bo
    join tb_BetSheet bs on bs.SheetId=bo.SheetId
    where bo.{1}=@{1}
) T
Join tb_User u on u.UserId=T.UserId
WHERE RowNumber BETWEEN {3} AND {4} order by 1", BetOrder.TABLENAME, BetOrder.SHEETID, BetOrder.ORDERID, startRow, endRow);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetOrder.SHEETID, sheetId));
        }
        /// <summary>
        /// 根据状态，公司找到某天的所有注单
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetOrders(BetStatus status, int companyId, DateTime date)
        {
            string sql = string.Format(@"SELECT bo.*,bs.{6} FROM {0} as bo with (nolock) JOIN {3} bs on bs.{4}=bo.{5} WHERE bo.{1}=@{1} AND CONVERT(char(10),bo.{2},120)=CONVERT(char(10),@{2},120) AND bo.{7}=@{7}",
                BetOrder.TABLENAME, BetOrder.STATUS, BetOrder.CREATETIME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID, BetSheet.USERID, BetOrder.COMPANYID);
            return base.ExecuteDataTable(sql, new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(BetOrder.CREATETIME, date),
                new SqlParameter(BetOrder.COMPANYID, companyId)).AsEnumerable().Select(it => new BetOrder(it));
        }
        /// <summary>
        /// 根据条件查找注单
        /// </summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="sortField">排序字段.</param>
        /// <param name="sortType">排序类型(acs/desc).</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> ListOrderByCondition(int? sheetId, int userId, BetStatus status, DateTime date, string num, int companyId,
            int gameplaywayId, string sortField, string sortType, int startRow, int endRow)
        {
            if (string.IsNullOrEmpty(sortField)) sortField = BetOrder.ORDERID;
            string condition;
            var parameterList = BuildCondition(sheetId, num, companyId, gameplaywayId, out condition, "bo.");
            //添加用户条件
            string subCondition = string.Format("bs.{0}=@{0}", BetSheet.USERID);
            condition += string.IsNullOrEmpty(condition) ? subCondition : " AND " + subCondition;
            parameterList.Add(new SqlParameter(BetSheet.USERID, userId));

            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            string sql = string.Format(@"WITH CTE AS(select bo.*,bs.{10},bs.{6},u.{11} from {0} bo
join {1} bs on bs.{2}=bo.{3}
join {4} u on u.{5}=bs.{6}
where CONVERT(char(10),bo.{7},120)=CONVERT(char(10),@{7},120) and bo.{8}=@{8} {9} )
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {12}) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {14} AND {15} ORDER BY RowNumber {13}", BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID,
            User.TABLENAME, User.USERID, BetSheet.USERID, BetOrder.CREATETIME, BetOrder.STATUS, condition, BetSheet.IPADDRESS, User.USERNAME,
            sortField, sortType, startRow, endRow);
            parameterList.Add(new SqlParameter(BetOrder.CREATETIME, date));
            parameterList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            return base.ExecuteList<BetOrder>(sql, parameterList.ToArray());
        }
        public IEnumerable<BetOrder> ListOrderByCondition(User user, BetStatus status, string num, int companyId, int gameplayway, WinLost winlost, DateTime fromDate, DateTime toDate, int startRow, int endRow)
        {
            string condition;
            var paramList = BuildCondition(null, num, companyId, gameplayway, out condition, "bo.");
            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            switch (winlost)
            {
                case WinLost.Win: condition += string.Format(@" AND {0}>0", BetOrder.DRAWRESULT); break;
                case WinLost.Lost: condition += string.Format(@" AND {0}<0", BetOrder.DRAWRESULT); break;
                default: break;
            }
            string sql = string.Format(@";WITH CTE AS(select bo.*,bs.IPAddress,bs.UserId,u.UserName from tb_BetOrder bo
join tb_BetSheet bs on bs.SheetId=bo.SheetId
join tb_User u on u.UserId=bs.UserId
where bs.{1}=@{1} and bo.{2}=@{2} and (CONVERT(char(10),BO.{4},120) BETWEEN CONVERT(char(10),@{5},120) AND CONVERT(char(10),@{6},120)) {3})
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {0}) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {7} AND {8} ORDER BY RowNumber", BetOrder.ORDERID,
              BetSheet.USERID, BetOrder.STATUS, condition, BetOrder.CREATETIME, "From", "To", startRow, endRow);
            paramList.Add(new SqlParameter(BetSheet.USERID, user.UserId));
            paramList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            paramList.Add(new SqlParameter("From", fromDate));
            paramList.Add(new SqlParameter("To", toDate));
            return base.ExecuteList<BetOrder>(sql, paramList.ToArray());
        }
        /// <summary>
        /// 按条件搜索下属用户所有注单
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplayId">The gameplay id.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> ListDescendantOrderByCondition(int userId, BetStatus status, string num, int companyId, int gameplayId, DateTime fromDate, DateTime toDate, int startRow, int endRow, WinLost winlost = WinLost.All, string sortField = BetOrder.ORDERID, string sortType = "Asc")
        {
            string condition;
            var paramList = BuildCondition(null, num, companyId, gameplayId, out condition, "bo.");
            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            switch (winlost)
            {
                case WinLost.Win: condition += string.Format(@" AND {0}>0", BetOrder.DRAWRESULT); break;
                case WinLost.Lost: condition += string.Format(@" AND {0}<0", BetOrder.DRAWRESULT); break;
                default: break;
            }
            string sql = string.Format(@";WITH UserTree AS
(
SELECT * FROM {0}  WHERE {1}=@{1}
UNION ALL
SELECT B.* FROM {0} AS B,UserTree AS C WHERE B.{2}=C.{1} and B.{1}>C.{1}
)
select * from(
select ROW_NUMBER() over(order by {15} {16}) as RowNumber,bo.*,bs.IPAddress,u.UserName from {3} bo
join {4} bs on bs.{5}=bo.{6}
join UserTree u on u.{1}=bs.{7}
where  bo.{8}=@{8} and CONVERT(char(10),bs.{9},120) BETWEEN CONVERT(char(10),@{10},120) AND CONVERT(char(10),@{11},120) {14})T
where RowNumber between {12} and {13}", User.TABLENAME, User.USERID, User.PARENTID, BetOrder.TABLENAME, BetSheet.TABLENAME,
                    BetSheet.SHEETID, BetOrder.SHEETID, BetSheet.USERID, BetOrder.STATUS, BetSheet.CREATETIME, "FromDate", "ToDate",
                    startRow, endRow, condition, sortField, sortType);
            paramList.Add(new SqlParameter(User.USERID, userId));
            paramList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            paramList.Add(new SqlParameter("FromDate", fromDate));
            paramList.Add(new SqlParameter("ToDate", toDate));
            return base.ExecuteList<BetOrder>(sql, paramList.ToArray());
        }
        /// <summary>
        /// 获取结单
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="itemCount">The item count.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> GetStatementOrder(User user, int itemCount)
        {
            string sql = string.Format(@"select TOP {2} CAST(bo.CreateTime as date) as CreateTime,
 SUM(Turnover) as Turnover,SUM(Amount) as Amount,
 SUM(Commission) as Commission,SUM(DrawResult) as DrawResult,
 SUM(case when DrawResult>0 then DrawResult else 0 end) as NetWin,
(select SUM(ibo.Turnover) from tb_BetOrder ibo WITH (NOLOCK)
join tb_BetSheet ibs WITH (NOLOCK) on ibs.SheetId=ibo.SheetId
 where ibs.{0}=@{0} and ibo.Status=0 and CAST(ibo.CreateTime as date)=CAST(bo.CreateTime as date)) as CancelAmount
from tb_BetOrder bo WITH (NOLOCK)
join tb_BetSheet bs WITH (NOLOCK) on bs.SheetId=bo.SheetId
where bo.Status<>{1} and bs.{0}=@{0}
group by CAST(bo.CreateTime as date)
having 1=1 order by CreateTime desc", BetSheet.USERID, (int)BetStatus.Invalid, itemCount);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetSheet.USERID, user.UserId));
        }
        /// <summary>
        /// 根据玩法获取最大金额的号码注单
        /// </summary>
        /// <param name="gameplayways">The gameplayways.</param>
        /// <param name="invalidStatus">The invalid status.</param>
        /// <param name="date">The date.</param>
        /// <param name="pageItemCount">The page item count.</param>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public DataTable ListMaxAmountOrderByNum(int userId, int[] gameplayways, BetStatus invalidStatus, DateTime date, int startRow, int endRow)
        {
            string gpwCondition = string.Empty;
            bool isFirst = true;
            foreach (var gpwId in gameplayways)
            {
                if (isFirst) isFirst = false;
                else gpwCondition += ",";
                gpwCondition += gpwId;
            }
            string sql = string.Format(@";WITH USERTABLE AS
	(
		SELECT * FROM tb_User  WHERE {0}=@{0}
		UNION ALL
		SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
	)
select u.username,bo.* from tb_BetOrder bo
join tb_BetSheet bs on bs.SheetId=bo.SheetId
join USERTABLE u on u.UserId=bs.UserId
where bo.Num in (
	select Num from(
		select ROW_NUMBER() over(order by Sum(bo.Amount) desc) as RowNumber,bo.Num from tb_BetOrder bo
		join tb_BetSheet bs on bs.SheetId=bo.SheetId
		join USERTABLE u on u.UserId=bs.UserId
		where bo.GamePlayWayId in ({1}) and bo.{2}<>@{2} and DATEDIFF(DD,bo.{3},@{3})=0 group by bo.Num
	)T
	where RowNumber between {4} and {5}) 
and bo.GamePlayWayId in ({1}) and bo.{2}<>@{2} and DATEDIFF(DD,bo.{3},@{3})=0",
            User.USERID, gpwCondition, BetOrder.STATUS, BetOrder.CREATETIME, startRow, endRow);
            return base.ExecuteDataTable(sql, new SqlParameter(User.USERID, userId),
                new SqlParameter(BetOrder.STATUS, (int)invalidStatus),
                new SqlParameter(BetOrder.CREATETIME, date));
        }
        /// <summary>
        /// 获取子用户所下注的汇总数据
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> ListChildBetAmounts(int userId, Role role, BetStatus status, DateTime date)
        {
            return base.ExecuteList<BetOrder>(CommandType.StoredProcedure, "SelectUserBetAmount", new SqlParameter(User.USERID, userId),
                new SqlParameter(BetOrder.STATUS, (int)status),
                new SqlParameter(User.ROLEID, (int)role),
                new SqlParameter(BetOrder.CREATETIME, date));
        }
        /// <summary>
        /// 获取公司统计
        /// </summary>
        /// <param name="companyId">The company.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public IEnumerable<BetOrder> ListCompanyRanking(int companyId, DateTime fromDate, DateTime toDate)
        {
            string sql = string.Format(@"select GamePlayWayId,SUM(Turnover) as Turnover,SUM(Commission) as Commission,SUM(DrawResult) as DrawResult,SUM(CASE when DrawResult>0 then drawresult else 0 end) as NetWin from tb_BetOrder
where {0}=@{0} and CAST({1} as DATE) between CAST(@{2} AS DATE) and CAST(@{3} as DATE) and {4}<>{5}
group by GamePlayWayId
order by GamePlayWayId", BetOrder.COMPANYID, BetOrder.CREATETIME, "From", "To", BetOrder.STATUS, (int)BetStatus.Invalid);
            return base.ExecuteList<BetOrder>(sql, new SqlParameter(BetOrder.COMPANYID, companyId),
                new SqlParameter("From", fromDate),
                new SqlParameter("To", toDate));
        }
        #endregion

        #region Count
        public decimal SumTotalBetAmount(int companyId, int gamePlayWayId, string num)
        {
            string sql = string.Format(@"SELECT ISNULL(SUM(Amount),0) as Amount From {0} WITH (NOLOCK) WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3} AND Status <>0 AND CAST(CreateTime as Date)=CAST(GETDATE() AS Date)", BetOrder.TABLENAME,
                BetOrder.COMPANYID, BetOrder.GAMEPLAYWAYID, BetOrder.NUM);
            object sum = base.ExecuteScalar(sql, new SqlParameter(BetOrder.COMPANYID, companyId),
                new SqlParameter(BetOrder.GAMEPLAYWAYID, gamePlayWayId),
                new SqlParameter(BetOrder.NUM, num));
            return Convert.ToDecimal(sum);
        }
        public int CountOrder(int sheetId)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1}", BetOrder.TABLENAME, BetOrder.SHEETID);
            object count = base.ExecuteScalar(sql, new SqlParameter(BetOrder.SHEETID, sheetId));
            return Convert.ToInt32(count);
        }
        public int CountOrder(int userId, DateTime date)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} BO
JOIN {1} BS ON BS.{2} =BO.{3}
WHERE BS.{4}=@{4} AND CONVERT(char(10),BO.{5},120)=CONVERT(char(10),@{5},120)", BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID,
                                                        BetSheet.USERID, BetOrder.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(BetSheet.USERID, userId),
                new SqlParameter(BetOrder.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        /// <summary>
        /// 根据条件计算注单的数量
        /// </summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <returns></returns>
        public int CountOrderByCondition(int? sheetId, int userId, BetStatus status, DateTime date, string num, int companyId, int gameplaywayId)
        {
            string condition;
            var parameterList = BuildCondition(sheetId, num, companyId, gameplaywayId, out condition, "bo.");
            //添加用户条件
            string subCondition = string.Format("bs.{0}=@{0}", BetSheet.USERID);
            condition += string.IsNullOrEmpty(condition) ? subCondition : " AND " + subCondition;
            parameterList.Add(new SqlParameter(BetSheet.USERID, userId));

            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} bo 
JOIN {4} bs on bs.{5}=bo.{6}
WHERE bo.{1}=@{1} AND CONVERT(char(10),bo.{2},120)=CONVERT(char(10),@{2},120) {3}", BetOrder.TABLENAME, BetOrder.STATUS, BetOrder.CREATETIME, condition,
                                                               BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID);
            parameterList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            parameterList.Add(new SqlParameter(BetOrder.CREATETIME, date));
            object count = base.ExecuteScalar(sql, parameterList.ToArray());
            return Convert.ToInt32(count);
        }
        /// <summary>
        /// 根据条件计算注单的数量
        /// </summary>
        /// <param name="sheetId">The sheet id.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="date">The date.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <returns></returns>
        public int CountOrderByCondition(User user, BetStatus status, string num, int companyId, int gameplayId, WinLost winlost, DateTime fromDate, DateTime toDate)
        {
            string condition;
            var paramList = BuildCondition(null, num, companyId, gameplayId, out condition, "bo.");
            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            switch (winlost)
            {
                case WinLost.Win: condition += string.Format(@" AND {0}>0", BetOrder.DRAWRESULT, string.IsNullOrEmpty(condition)); break;
                case WinLost.Lost: condition += string.Format(@" AND {0}<0", BetOrder.DRAWRESULT, string.IsNullOrEmpty(condition)); break;
                default: break;
            }
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} bo join {1} bs on bs.{2}=bo.{3}
where bs.{4}=@{4} and bo.{5}=@{5} and (CONVERT(char(10),BO.{7},120) BETWEEN CONVERT(char(10),@{8},120) AND CONVERT(char(10),@{9},120)) {6}",
             BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, BetOrder.SHEETID, BetSheet.USERID, BetOrder.STATUS, condition, BetOrder.CREATETIME, "From", "To");
            paramList.Add(new SqlParameter(BetSheet.USERID, user.UserId));
            paramList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            paramList.Add(new SqlParameter("From", fromDate));
            paramList.Add(new SqlParameter("To", toDate));
            object count = base.ExecuteScalar(sql, paramList.ToArray());
            return Convert.ToInt32(count);
        }
        /// <summary>
        /// 按条件计算下属用户注单数量
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="status">The status.</param>
        /// <param name="num">The num.</param>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplayId">The gameplay id.</param>
        /// <param name="fromDate">From date.</param>
        /// <param name="toDate">To date.</param>
        /// <returns></returns>
        public int CountDescendantOrderCondition(int userId, BetStatus status, string num, int companyId, int gameplayId, DateTime fromDate, DateTime toDate, WinLost winlost = WinLost.All)
        {
            string condition;
            var paramList = BuildCondition(null, num, companyId, gameplayId, out condition, "bo.");
            condition = string.IsNullOrEmpty(condition) ? condition : " AND " + condition;
            switch (winlost)
            {
                case WinLost.Win: condition += string.Format(@" AND {0}>0", BetOrder.DRAWRESULT); break;
                case WinLost.Lost: condition += string.Format(@" AND {0}<0", BetOrder.DRAWRESULT); break;
                default: break;
            }
            string sql = string.Format(@";WITH UserTree AS
(
SELECT * FROM {0}  WHERE {1}=@{1}
UNION ALL
SELECT B.* FROM {0} AS B,UserTree AS C WHERE B.{2}=C.{1} and B.{1}>C.{1}
)
select COUNT(0) from {3} bo
join {4} bs on bs.{5}=bo.{6}
join UserTree ut on ut.{1}=bs.{7}
where bo.{8}=@{8} and CONVERT(char(10),bs.{9},120) BETWEEN CONVERT(char(10),@{10},120) AND CONVERT(char(10),@{11},120) {12}", User.TABLENAME, User.USERID, User.PARENTID, BetOrder.TABLENAME, BetSheet.TABLENAME,
                    BetSheet.SHEETID, BetOrder.SHEETID, BetSheet.USERID, BetOrder.STATUS, BetSheet.CREATETIME, "FromDate", "ToDate",
                    condition);
            paramList.Add(new SqlParameter(User.USERID, userId));
            paramList.Add(new SqlParameter(BetOrder.STATUS, (int)status));
            paramList.Add(new SqlParameter("FromDate", fromDate));
            paramList.Add(new SqlParameter("ToDate", toDate));
            object count = base.ExecuteScalar(sql, paramList.ToArray());
            return Convert.ToInt32(count);
        }
        public int CountNumAmountRanking(int userId, int companyId, int gpwId, string num)
        {
            string sql = string.Format(@";WITH USERTABLE AS
	(
		SELECT * FROM tb_User  WHERE {3}=@{3}
		UNION ALL
		SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
	)
SELECT COUNT(distinct bs.UserId)
     FROM {0} bo
     join {1} bs on bs.{2}=bo.{2}
     join USERTABLE u on u.{3}=bs.{3}
     where CAST(bs.CreateTime as DATE)=CAST(GETDATE() AS DATE) and bs.Status=1
     and bo.CompanyId=@CompanyId and bo.GamePlayWayId=@GamePlayWayId and bo.Num=@Num
     ", BetOrder.TABLENAME, BetSheet.TABLENAME, BetSheet.SHEETID, User.USERID);

            object count = ExecuteScalar(sql, new SqlParameter("CompanyId", companyId),
                new SqlParameter("GamePlayWayId", gpwId),
                new SqlParameter("Num", num),
                new SqlParameter("UserId", userId));
            return Convert.ToInt32(count);
        }
        public int CountMaxAmountOrderByNum(int userId, int[] gameplayways, BetStatus invalidStatus, DateTime date)
        {
            string gpwCondition = string.Empty;
            bool isFirst = true;
            foreach (var gpwId in gameplayways)
            {
                if (isFirst) isFirst = false;
                else gpwCondition += ",";
                gpwCondition += gpwId;
            }
            string sql = string.Format(@";WITH USERTABLE AS
	(
		SELECT * FROM tb_User  WHERE {0}=@{0}
		UNION ALL
		SELECT B.* FROM tb_User AS B,USERTABLE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
	)
select COUNT(Distinct(bo.Num)) from tb_BetOrder bo WITH (NOLOCK)
join tb_BetSheet bs WITH (NOLOCK) on bs.SheetId=bo.SheetId
join USERTABLE u on u.UserId=bs.UserId
where bo.{1}<>@{1} and GamePlayWayId in ({2}) and
CONVERT(char(10),bo.{3},120)=CONVERT(char(10),@{3},120) ", User.USERID, BetOrder.STATUS, gpwCondition, BetOrder.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.USERID, userId),
                new SqlParameter(BetOrder.STATUS, (int)invalidStatus),
                new SqlParameter(BetOrder.CREATETIME, date));
            return Convert.ToInt32(count);
        }
        #endregion

        private IList<SqlParameter> BuildCondition(int? sheetId, string num, int companyId, int gameplaywayId, out string condit, string prefix = "")
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            StringBuilder condition = new StringBuilder();
            Action<string> appendCondition = (it) =>
            {
                if (condition.Length > 0)
                    condition.Append(" And ");
                condition.AppendFormat("{1}{0}=@{0}", it, prefix);
            };
            if (sheetId > 0)
            {
                appendCondition(BetOrder.SHEETID);
                parameterList.Add(new SqlParameter(BetOrder.SHEETID, sheetId));
            }
            if (!string.IsNullOrEmpty(num))
            {
                appendCondition(BetOrder.NUM);
                parameterList.Add(new SqlParameter(BetOrder.NUM, num));
            }
            if (companyId > 0)
            {
                appendCondition(BetOrder.COMPANYID);
                parameterList.Add(new SqlParameter(BetOrder.COMPANYID, companyId));
            }
            if (gameplaywayId > 0)
            {
                appendCondition(BetOrder.GAMEPLAYWAYID);
                parameterList.Add(new SqlParameter(BetOrder.GAMEPLAYWAYID, gameplaywayId));
            }
            condit = condition.ToString();
            return parameterList;
        }
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
