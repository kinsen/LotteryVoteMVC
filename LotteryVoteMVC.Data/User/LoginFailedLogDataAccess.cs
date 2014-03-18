using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Data
{
    public class LoginFailedLogDataAccess : DataBase
    {
        public void Insert(User user)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()", LoginFailedLog.TABLENAME,
                LoginFailedLog.USERID, LoginFailedLog.IPField);
            base.ExecuteScalar(sql, new SqlParameter(LoginFailedLog.USERID, user.UserId),
                new SqlParameter(LoginFailedLog.IPField, IPHelper.IPAddress));
        }

        public IEnumerable<LoginFailedLog> GetLog(User user, int start, int end)
        {
            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY LogId desc) AS RowNumber,* 
     FROM {0} WHERE {1}=@{1}
) T
WHERE RowNumber BETWEEN {2} AND {3}", LoginFailedLog.TABLENAME, LoginFailedLog.USERID, start, end);
            return base.ExecuteList<LoginFailedLog>(sql, new SqlParameter(LoginFailedLog.USERID, user.UserId));
        }
        public int CountLog(User user)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1}", LoginFailedLog.TABLENAME, LoginFailedLog.USERID);
            object count = base.ExecuteScalar(sql, new SqlParameter(LoginFailedLog.USERID, user.UserId));
            return Convert.ToInt32(count);
        }

        public int GetFailedCount(User user, DateTime date)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)",
                LoginFailedLog.TABLENAME, LoginFailedLog.USERID, LoginFailedLog.CREATEDATE);
            object count = base.ExecuteScalar(sql, new SqlParameter(LoginFailedLog.USERID, user.UserId),
                new SqlParameter(LoginFailedLog.CREATEDATE, date));
            return Convert.ToInt32(count);
        }
    }
}
