using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class UserBetAutoDropWaterDataAccess : DataBase
    {
        public void Insert(UserBetAutoDropWater drop)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5}) VALUES (@{1},@{2},@{3},@{4},@{5}) SELECT SCOPE_IDENTITY()",
                UserBetAutoDropWater.TABLENAME, UserBetAutoDropWater.USERID, UserBetAutoDropWater.COMPANYTYPEID, UserBetAutoDropWater.GAMEPLAYWAYID,
                UserBetAutoDropWater.AMOUNT, UserBetAutoDropWater.DROPVALUE);
            object id = base.ExecuteScalar(sql, new SqlParameter(UserBetAutoDropWater.USERID, drop.UserId),
                new SqlParameter(UserBetAutoDropWater.COMPANYTYPEID, drop.CompanyTypeId),
                new SqlParameter(UserBetAutoDropWater.GAMEPLAYWAYID, drop.GamePlayWayId),
                new SqlParameter(UserBetAutoDropWater.AMOUNT, drop.Amount),
                new SqlParameter(UserBetAutoDropWater.DROPVALUE, drop.DropValue));
            drop.BetDropId = Convert.ToInt32(id);
        }
        public void Delete(int dropId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", UserBetAutoDropWater.TABLENAME, UserBetAutoDropWater.BETDROPID);
            base.ExecuteNonQuery(sql, new SqlParameter(UserBetAutoDropWater.BETDROPID, dropId));
        }

        public IEnumerable<UserBetAutoDropWater> GetDrop(CompanyType companyType, int gameplayway, int userId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3}", UserBetAutoDropWater.TABLENAME,
                UserBetAutoDropWater.COMPANYTYPEID, UserBetAutoDropWater.GAMEPLAYWAYID, UserBetAutoDropWater.USERID);
            return base.ExecuteList<UserBetAutoDropWater>(sql, new SqlParameter(UserBetAutoDropWater.COMPANYTYPEID, (int)companyType),
                new SqlParameter(UserBetAutoDropWater.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(UserBetAutoDropWater.USERID, userId));
        }
        public IEnumerable<UserBetAutoDropWater> GetDrops(CompanyType companyType, int gameplayway, int userId, decimal amount)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3} AND {4}=@{4}", UserBetAutoDropWater.TABLENAME,
                UserBetAutoDropWater.COMPANYTYPEID, UserBetAutoDropWater.GAMEPLAYWAYID, UserBetAutoDropWater.USERID, UserBetAutoDropWater.AMOUNT);
            return base.ExecuteList<UserBetAutoDropWater>(sql, new SqlParameter(UserBetAutoDropWater.COMPANYTYPEID, (int)companyType),
                new SqlParameter(UserBetAutoDropWater.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(UserBetAutoDropWater.USERID, userId),
                new SqlParameter(UserBetAutoDropWater.AMOUNT, amount));
        }
        /// <summary>
        /// 获取所有祖先与自己的跌水.
        /// </summary>
        /// <param name="companyType">Type of the company.</param>
        /// <param name="gameplayway">The gameplayway.</param>
        /// <param name="userId">The user id.</param>
        /// <param name="amount">The amount.</param>
        /// <returns></returns>
        public IEnumerable<UserBetAutoDropWater> GetFamilyDrops(CompanyType companyType, int gameplayway, int userId)
        {
            string sql = string.Format(@";WITH tUser AS
(
SELECT * FROM tb_User  WHERE {3}=@{3}
UNION ALL
SELECT B.* FROM tb_User AS B,tUser AS C WHERE B.UserId=C.ParentId and c.UserId>b.UserId
)
SELECT * FROM {0} d
JOIN tUser u on u.UserId=d.UserId
WHERE d.{1}=@{1} AND d.{2}=@{2} ORDER BY d.UserId ASC", UserBetAutoDropWater.TABLENAME,
                UserBetAutoDropWater.COMPANYTYPEID, UserBetAutoDropWater.GAMEPLAYWAYID, UserBetAutoDropWater.USERID);
            return base.ExecuteList<UserBetAutoDropWater>(sql, new SqlParameter(UserBetAutoDropWater.COMPANYTYPEID, (int)companyType),
                new SqlParameter(UserBetAutoDropWater.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(UserBetAutoDropWater.USERID, userId));
        }
        public IEnumerable<UserBetAutoDropWater> ListDrop(int userId, int start, int end)
        {
            string sql = string.Format(@"select * from 
(
	select ROW_NUMBER() over (order by {1},GamePlayWayId,Amount) as RowNumber,*
	from {0} where UserId={4}
) T
where RowNumber between {2} and {3}", UserBetAutoDropWater.TABLENAME, UserBetAutoDropWater.COMPANYTYPEID, start, end, userId);
            return base.ExecuteList<UserBetAutoDropWater>(sql);
        }
        public IEnumerable<UserBetAutoDropWater> ListByCondition(int userId, int companyType, int gameplayway, decimal amount, double dropValue, int start, int end)
        {
            string condition;
            var paramList = BuildCondition(userId, companyType, gameplayway, amount, dropValue, out condition);
            if (!string.IsNullOrEmpty(condition))
                condition = " where " + condition;
            string sql = string.Format(@"select * from 
(
	select ROW_NUMBER() over (order by {2} ,GamePlayWayId) as RowNumber,*
	from {0} {1}
) T
where RowNumber between {3} and {4}", UserBetAutoDropWater.TABLENAME, condition, UserBetAutoDropWater.COMPANYTYPEID, start, end);
            return base.ExecuteList<UserBetAutoDropWater>(sql, paramList.ToArray());
        }

        #region Count
        public int CountAllDrop()
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0}", UserBetAutoDropWater.TABLENAME);
            object count = base.ExecuteScalar(sql);
            return Convert.ToInt32(count);
        }
        public int CountByCondition(int userId, int companyType, int gameplayway, decimal amount, double dropValue)
        {
            string condition;
            var paramList = BuildCondition(userId, companyType, gameplayway, amount, dropValue, out condition);
            if (!string.IsNullOrEmpty(condition))
                condition = " where " + condition;
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} {1}", UserBetAutoDropWater.TABLENAME, condition);
            object count = base.ExecuteScalar(sql, paramList.ToArray());
            return Convert.ToInt32(count);
        }
        #endregion

        private IList<SqlParameter> BuildCondition(int userId, int companyType, int gameplayway, decimal amount, double dropValue, out string condition, string prefix = "")
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
            if (userId > 0)
                appendToCondition(UserBetAutoDropWater.USERID, userId);
            if (companyType > 0)
                appendToCondition(UserBetAutoDropWater.COMPANYTYPEID, companyType);
            if (gameplayway > 0)
                appendToCondition(UserBetAutoDropWater.GAMEPLAYWAYID, gameplayway);
            if (amount > 0)
                appendToCondition(UserBetAutoDropWater.AMOUNT, amount);
            if (dropValue > 0)
                appendToCondition(UserBetAutoDropWater.DROPVALUE, dropValue);
            condition = sb.ToString();
            return paramList;
        }
    }
}
