using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class SystemErrorDataAccess : DataBase
    {
        public void Insert(SystemError error)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6}) VALUES (@{1},@{2},@{3},@{4},@{5},@{6})", SystemError.TABLENAME, SystemError.ERRORMESSAGE,
                SystemError.ERRORLEVEL, SystemError.STACKTRACK, SystemError.PAGEURL, SystemError.REMARKS, SystemError.IPFIELD);
            base.ExecuteNonQuery(sql, new SqlParameter(SystemError.ERRORMESSAGE, error.ErrorMessage),
                new SqlParameter(SystemError.ERRORLEVEL, (int)error.ErrorLevel),
                new SqlParameter(SystemError.STACKTRACK, error.StackTrack),
                new SqlParameter(SystemError.PAGEURL, error.PageUrl),
                new SqlParameter(SystemError.REMARKS, error.Remarks),
                new SqlParameter(SystemError.IPFIELD, error.IP));
        }
        public SystemError GetLog(int logId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", SystemError.TABLENAME, SystemError.ID);
            return base.ExecuteModel<SystemError>(sql, new SqlParameter(SystemError.ID, logId));
        }

        public IEnumerable<SystemError> GetLog(int start, int end)
        {
            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY Id  desc) AS RowNumber,* 
     FROM tb_SystemError
) T
WHERE RowNumber BETWEEN {0} AND {1}", start, end);
            return ExecuteList<SystemError>(sql);
        }

        #region Count
        public int CountLog()
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0}", SystemError.TABLENAME);
            object count = base.ExecuteScalar(sql);
            return Convert.ToInt32(count);
        }
        #endregion
    }
}
