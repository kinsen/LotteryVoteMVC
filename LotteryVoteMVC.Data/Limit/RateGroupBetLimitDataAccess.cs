using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class RateGroupBetLimitDataAccess : DataBase
    {
        public void InsertLimits(IEnumerable<RateGroupBetLimit> limits)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(RateGroupBetLimit.GROUPID, typeof(int));
            dt.Columns.Add(RateGroupBetLimit.GAMEID, typeof(int));
            dt.Columns.Add(RateGroupBetLimit.LEASTLIMIT, typeof(decimal));
            dt.Columns.Add(RateGroupBetLimit.LARGESTLIMIT, typeof(decimal));
            foreach (var limit in limits)
            {
                var row = dt.NewRow();
                row[RateGroupBetLimit.GROUPID] = limit.GroupId;
                row[RateGroupBetLimit.GAMEID] = (int)limit.GameType;
                row[RateGroupBetLimit.LEASTLIMIT] = limit.LeastLimit;
                row[RateGroupBetLimit.LARGESTLIMIT] = limit.LargestLimit;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(RateGroupBetLimit.TABLENAME, dt);
        }
        public void DeleteLimit(int groupId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", RateGroupBetLimit.TABLENAME, RateGroupBetLimit.GROUPID);
            base.ExecuteNonQuery(sql, new SqlParameter(RateGroupBetLimit.GROUPID, groupId));
        }

        public IEnumerable<RateGroupBetLimit> ListLimitByGroup(int groupId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", RateGroupBetLimit.TABLENAME, RateGroupBetLimit.GROUPID);
            return base.ExecuteList<RateGroupBetLimit>(sql, new SqlParameter(RateGroupBetLimit.GROUPID, groupId));
        }
    }
}
