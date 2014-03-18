using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Data
{
    public class LoginLogDataAccess : DataBase
    {
        public void AddLog(User user)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()", LoginLog.TABLENAME, LoginLog.USERID, LoginLog.IPFIELD);
            base.ExecuteNonQuery(sql, new SqlParameter(LoginLog.USERID, user.UserId),
                new SqlParameter(LoginLog.IPFIELD, IPHelper.IPAddress));
        }

        public IEnumerable<LoginLog> GetLog(User user, int startRow, int endRow)
        {
            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY LoginId desc) AS RowNumber,* 
     FROM tb_LoginLog where {0}=@{0}
) T
WHERE RowNumber BETWEEN {1} AND {2}", LoginLog.USERID, startRow, endRow);
            return base.ExecuteList<LoginLog>(sql, new SqlParameter(LoginLog.USERID, user.UserId));
        }
        public int CountLog(User user)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1}", LoginLog.TABLENAME, LoginLog.USERID);
            object count = base.ExecuteScalar(sql, new SqlParameter(LoginLog.USERID, user.UserId));
            return Convert.ToInt32(count);
        }
    }
}
