using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class RateGroupGameBetLimitDataAccess : DataBase
    {
        public void InsertLimits(IEnumerable<RateGroupGameBetLimit> limits)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(RateGroupGameBetLimit.GROUPID, typeof(int));
            dt.Columns.Add(RateGroupGameBetLimit.GAMEPLAYWAYID, typeof(int));
            dt.Columns.Add(RateGroupGameBetLimit.COMPANYTYPE, typeof(int));
            dt.Columns.Add(RateGroupGameBetLimit.LIMITVALUE, typeof(decimal));
            foreach (var limit in limits)
            {
                var row = dt.NewRow();
                row[RateGroupGameBetLimit.GROUPID] = limit.GroupId;
                row[RateGroupGameBetLimit.GAMEPLAYWAYID] = limit.GamePlayWayId;
                row[RateGroupGameBetLimit.COMPANYTYPE] = (int)limit.CompanyType;
                row[RateGroupGameBetLimit.LIMITVALUE] = limit.LimitValue;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(RateGroupGameBetLimit.TABLENAME, dt);
        }
        public void Delete(int groupId, CompanyType companyType)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND {2}=@{2}", RateGroupGameBetLimit.TABLENAME, RateGroupGameBetLimit.GROUPID, RateGroupGameBetLimit.COMPANYTYPE);
            base.ExecuteNonQuery(sql, new SqlParameter(RateGroupGameBetLimit.GROUPID, groupId),
                new SqlParameter(RateGroupGameBetLimit.COMPANYTYPE, (int)companyType));
        }

        public IEnumerable<RateGroupGameBetLimit> ListLimitByGroup(int groupId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", RateGroupGameBetLimit.TABLENAME, RateGroupGameBetLimit.GROUPID);
            return base.ExecuteList<RateGroupGameBetLimit>(sql, new SqlParameter(RateGroupGameBetLimit.GROUPID, groupId));
        }
        public IEnumerable<RateGroupGameBetLimit> GetLimits(int groupId, CompanyType companyType)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", RateGroupGameBetLimit.TABLENAME, RateGroupGameBetLimit.GROUPID, RateGroupGameBetLimit.COMPANYTYPE);
            return base.ExecuteList<RateGroupGameBetLimit>(sql, new SqlParameter(RateGroupGameBetLimit.GROUPID, groupId),
                new SqlParameter(RateGroupGameBetLimit.COMPANYTYPE, (int)companyType));
        }
    }
}
