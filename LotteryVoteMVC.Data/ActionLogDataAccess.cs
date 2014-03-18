using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class ActionLogDataAccess : DataBase
    {
        public void Insert(ActionLog log)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5}) VALUES (@{1},@{2},@{3},@{4},@{5}) SELECT SCOPE_IDENTITY()",
               ActionLog.TABLENAME, ActionLog.USERID, ActionLog.TARGETUSERID, ActionLog.ACTION, ActionLog.DETAIL, ActionLog.IPADDRESS);
            object id = base.ExecuteScalar(sql, new SqlParameter(ActionLog.USERID, log.UserId),
                new SqlParameter(ActionLog.TARGETUSERID, log.TargetUserId),
                new SqlParameter(ActionLog.ACTION, log.Action),
                new SqlParameter(ActionLog.DETAIL, log.Detail),
                new SqlParameter(ActionLog.IPADDRESS, log.IPAddress));
            log.LogId = Convert.ToInt32(id);
        }

        public IEnumerable<ActionLog> GetActionLog(User user, int startRow, int endRow)
        {
            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY LogId desc) AS RowNumber,al.*,u.ParentId,u.UserName,u.RoleId 
     FROM tb_ActionLog al
JOIN tb_User u on u.UserId=al.TargetUserId
where al.{0}=@{0}
) T
WHERE RowNumber BETWEEN {1} AND {2}", ActionLog.USERID, startRow, endRow);
            return base.ExecuteList<ActionLog>(sql, new SqlParameter(ActionLog.USERID, user.UserId));
        }
        public IEnumerable<ActionLog> GetActionLog(User target, DateTime fromDate, DateTime toDate, int startRow, int endRow)
        {
            string condition = target == null ? string.Empty : string.Format(" And {0}={1}", ActionLog.TARGETUSERID, target.UserId);
            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY LogId desc) AS RowNumber,al.*,u.ParentId,u.UserName,u.RoleId 
     FROM tb_ActionLog al
JOIN tb_User u on u.UserId=al.TargetUserId
where CAST(al.{3} as DATE) between CAST(@FromDate AS DATE) and CAST(@ToDate AS DATE) {0}
) T
WHERE RowNumber BETWEEN {1} AND {2}", condition, startRow, endRow, ActionLog.CREATETIME);
            return base.ExecuteList<ActionLog>(sql, new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate));
        }

        #region COUNT
        public int CountActionLog(User user)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1}", ActionLog.TABLENAME, ActionLog.USERID);
            object count = base.ExecuteScalar(sql, new SqlParameter(ActionLog.USERID, user.UserId));
            return Convert.ToInt32(count);
        }
        public int CountActionLog(User target, DateTime fromDate, DateTime toDate)
        {
            string condition = target == null ? string.Empty : string.Format(" And {0}={1}", ActionLog.TARGETUSERID, target.UserId);
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE  CAST({2} as DATE) between CAST(@FromDate AS DATE) and CAST(@ToDate AS DATE) {1}", ActionLog.TABLENAME, condition, ActionLog.CREATETIME);
            object count = base.ExecuteScalar(sql, new SqlParameter("FromDate", fromDate), new SqlParameter("ToDate", toDate));
            return Convert.ToInt32(count);
        }
        #endregion
    }
}
