using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class BetAutoDropWaterDataAccess : DataBase
    {
        public void Insert(BetAutoDropWater drop)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4}) VALUES (@{1},@{2},@{3},@{4}) SELECT SCOPE_IDENTITY()",
                BetAutoDropWater.TABLENAME, BetAutoDropWater.COMPANYTYPEID, BetAutoDropWater.GAMEPLAYWAYID,
                BetAutoDropWater.AMOUNT, BetAutoDropWater.DROPVALUE);
            object id = base.ExecuteScalar(sql, new SqlParameter(BetAutoDropWater.COMPANYTYPEID, drop.CompanyTypeId),
                new SqlParameter(BetAutoDropWater.GAMEPLAYWAYID, drop.GamePlayWayId),
                new SqlParameter(BetAutoDropWater.AMOUNT, drop.Amount),
                new SqlParameter(BetAutoDropWater.DROPVALUE, drop.DropValue));
            drop.BetDropId = Convert.ToInt32(id);
        }
        public void Delete(int dropId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", BetAutoDropWater.TABLENAME, BetAutoDropWater.BETDROPID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetAutoDropWater.BETDROPID, dropId));
        }

        public IEnumerable<BetAutoDropWater> GetDrop(CompanyType companyType, int gameplayway)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", BetAutoDropWater.TABLENAME,
                BetAutoDropWater.COMPANYTYPEID, BetAutoDropWater.GAMEPLAYWAYID);
            return base.ExecuteList<BetAutoDropWater>(sql, new SqlParameter(BetAutoDropWater.COMPANYTYPEID, (int)companyType),
                new SqlParameter(BetAutoDropWater.GAMEPLAYWAYID, gameplayway));
        }
        public IEnumerable<BetAutoDropWater> GetDrops(CompanyType companyType, int gameplayway, decimal amount)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3}", BetAutoDropWater.TABLENAME,
                BetAutoDropWater.COMPANYTYPEID, BetAutoDropWater.GAMEPLAYWAYID, BetAutoDropWater.AMOUNT);
            return base.ExecuteList<BetAutoDropWater>(sql, new SqlParameter(BetAutoDropWater.COMPANYTYPEID, (int)companyType),
                new SqlParameter(BetAutoDropWater.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(BetAutoDropWater.AMOUNT, amount));
        }
        public IEnumerable<BetAutoDropWater> ListDrop(int start, int end)
        {
            string sql = string.Format(@"select * from 
(
	select ROW_NUMBER() over (order by {1},GamePlayWayId,Amount) as RowNumber,*
	from {0}
) T
where RowNumber between {2} and {3}", BetAutoDropWater.TABLENAME, BetAutoDropWater.COMPANYTYPEID, start, end);
            return base.ExecuteList<BetAutoDropWater>(sql);
        }
        public IEnumerable<BetAutoDropWater> ListByCondition(int companyType, int gameplayway, decimal amount, double dropValue, int start, int end)
        {
            string condition;
            var paramList = BuildCondition(companyType, gameplayway, amount, dropValue, out condition);
            if (!string.IsNullOrEmpty(condition))
                condition = " where " + condition;
            string sql = string.Format(@"select * from 
(
	select ROW_NUMBER() over (order by {2} ,GamePlayWayId) as RowNumber,*
	from {0} {1}
) T
where RowNumber between {3} and {4}", BetAutoDropWater.TABLENAME, condition, BetAutoDropWater.COMPANYTYPEID, start, end);
            return base.ExecuteList<BetAutoDropWater>(sql, paramList.ToArray());
        }

        #region Count
        public int CountAllDrop()
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0}", BetAutoDropWater.TABLENAME);
            object count = base.ExecuteScalar(sql);
            return Convert.ToInt32(count);
        }
        public int CountByCondition(int companyType, int gameplayway, decimal amount, double dropValue)
        {
            string condition;
            var paramList = BuildCondition(companyType, gameplayway, amount, dropValue, out condition);
            if (!string.IsNullOrEmpty(condition))
                condition = " where " + condition;
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} {1}", BetAutoDropWater.TABLENAME, condition);
            object count = base.ExecuteScalar(sql, paramList.ToArray());
            return Convert.ToInt32(count);
        }
        #endregion

        private IList<SqlParameter> BuildCondition(int companyType, int gameplayway, decimal amount, double dropValue, out string condition, string prefix = "")
        {
            StringBuilder sb = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            Action<string, object> appendToCondition = (key, value) =>
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");
                sb.AppendFormat("{1}{0}=@{0}", key, prefix);
                paramList.Add(new SqlParameter(key, value));
            };
            if (companyType > 0)
                appendToCondition(BetAutoDropWater.COMPANYTYPEID, companyType);
            if (gameplayway > 0)
                appendToCondition(BetAutoDropWater.GAMEPLAYWAYID, gameplayway);
            if (amount > 0)
                appendToCondition(BetAutoDropWater.AMOUNT, amount);
            if (dropValue > 0)
                appendToCondition(BetAutoDropWater.DROPVALUE, dropValue);
            condition = sb.ToString();
            return paramList;
        }
    }
}
