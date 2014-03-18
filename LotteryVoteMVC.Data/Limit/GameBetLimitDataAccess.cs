using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class GameBetLimitDataAccess : DataBase
    {
        public void InsertLimits(IEnumerable<GameBetLimit> limits)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(GameBetLimit.USERID, typeof(int));
            dt.Columns.Add(GameBetLimit.GAMEPLAYWAYID, typeof(int));
            dt.Columns.Add(GameBetLimit.COMPANYTYPE, typeof(int));
            dt.Columns.Add(GameBetLimit.LIMITVALUE, typeof(decimal));
            foreach (var limit in limits)
            {
                var row = dt.NewRow();
                row[GameBetLimit.USERID] = limit.UserId;
                row[GameBetLimit.GAMEPLAYWAYID] = limit.GamePlayWayId;
                row[GameBetLimit.COMPANYTYPE] = (int)limit.CompanyType;
                row[GameBetLimit.LIMITVALUE] = limit.LimitValue;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(GameBetLimit.TABLENAME, dt);
        }
        public void Delete(User user, CompanyType companyType)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND {2}=@{2}", GameBetLimit.TABLENAME, GameBetLimit.USERID, GameBetLimit.COMPANYTYPE);
            base.ExecuteNonQuery(sql, new SqlParameter(GameBetLimit.USERID, user.UserId),
                new SqlParameter(GameBetLimit.COMPANYTYPE, (int)companyType));
        }

        public IEnumerable<GameBetLimit> ListLimitByUser(int userId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", GameBetLimit.TABLENAME, GameBetLimit.USERID);
            return base.ExecuteList<GameBetLimit>(sql, new SqlParameter(GameBetLimit.USERID, userId));
        }
        public IEnumerable<GameBetLimit> GetLimits(int userId, CompanyType companyType)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", GameBetLimit.TABLENAME, GameBetLimit.USERID, GameBetLimit.COMPANYTYPE);
            return base.ExecuteList<GameBetLimit>(sql, new SqlParameter(GameBetLimit.USERID, userId),
                new SqlParameter(GameBetLimit.COMPANYTYPE, (int)companyType));
        }
    }
}
