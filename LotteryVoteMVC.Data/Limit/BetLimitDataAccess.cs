using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class BetLimitDataAccess : DataBase
    {
        public void InsertLimits(IEnumerable<BetLimit> limits)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(BetLimit.USERID, typeof(int));
            dt.Columns.Add(BetLimit.GAMEID, typeof(int));
            dt.Columns.Add(BetLimit.LEASTLIMIT, typeof(decimal));
            dt.Columns.Add(BetLimit.LARGESTLIMIT, typeof(decimal));
            foreach (var limit in limits)
            {
                var row = dt.NewRow();
                row[BetLimit.USERID] = limit.UserId;
                row[BetLimit.GAMEID] = (int)limit.GameType;
                row[BetLimit.LEASTLIMIT] = limit.LeastLimit;
                row[BetLimit.LARGESTLIMIT] = limit.LargestLimit;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(BetLimit.TABLENAME, dt);
        }
        public void DeleteLimit(User user)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", BetLimit.TABLENAME, BetLimit.USERID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetLimit.USERID, user.UserId));
        }

        public IEnumerable<BetLimit> ListLimitByUser(int userId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", BetLimit.TABLENAME, BetLimit.USERID);
            return base.ExecuteList<BetLimit>(sql, new SqlParameter(BetLimit.USERID, userId));
        }
    }
}
