using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class BetUpperLimitDataAccess : DataBase
    {
        public void Insert(BetUpperLimit upperLimit)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6},{7}) VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7}) 
SELECT SCOPE_IDENTITY()", BetUpperLimit.TABLENAME, BetUpperLimit.NUM, BetUpperLimit.COMPANYID, BetUpperLimit.GAMEPLAYWAYID, BetUpperLimit.DROPVALUE,
                            BetUpperLimit.NEXTLIMIT, BetUpperLimit.UPPERLLIMIT, BetUpperLimit.TOTALBETAMOUNT);
            object id = base.ExecuteScalar(sql, new SqlParameter(BetUpperLimit.NUM, upperLimit.Num),
                new SqlParameter(BetUpperLimit.COMPANYID, upperLimit.CompanyId),
                new SqlParameter(BetUpperLimit.GAMEPLAYWAYID, upperLimit.GamePlayWayId),
                new SqlParameter(BetUpperLimit.DROPVALUE, upperLimit.DropValue),
                new SqlParameter(BetUpperLimit.NEXTLIMIT, upperLimit.NextLimit),
                new SqlParameter(BetUpperLimit.UPPERLLIMIT, upperLimit.UpperLlimit),
                new SqlParameter(BetUpperLimit.TOTALBETAMOUNT, upperLimit.TotalBetAmount));
            upperLimit.LimitId = Convert.ToInt32(id);
        }
        public void Update(BetUpperLimit upperLimit)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1},{2}=@{2},{3}=@{3},{4}=@{4},{5}=@{5} WHERE {6}=@{6}", BetUpperLimit.TABLENAME,
                BetUpperLimit.DROPVALUE, BetUpperLimit.NEXTLIMIT, BetUpperLimit.UPPERLLIMIT, BetUpperLimit.TOTALBETAMOUNT, BetUpperLimit.STOPBET, BetUpperLimit.LIMITID);
            base.ExecuteNonQuery(sql, new SqlParameter(BetUpperLimit.DROPVALUE, upperLimit.DropValue),
                new SqlParameter(BetUpperLimit.NEXTLIMIT, upperLimit.NextLimit),
                new SqlParameter(BetUpperLimit.UPPERLLIMIT, upperLimit.UpperLlimit),
                new SqlParameter(BetUpperLimit.TOTALBETAMOUNT, upperLimit.TotalBetAmount),
                new SqlParameter(BetUpperLimit.STOPBET, upperLimit.StopBet),
                new SqlParameter(BetUpperLimit.LIMITID, upperLimit.LimitId));
        }

        /// <summary>
        /// 根据公司，玩法获取特定日期的上限
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="gameplayway">The gameplayway.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public DataTable GetUpperLimit(int companyId, int gameplayway, DateTime date)
        {
            string sql = string.Format(@"select * from tb_BetUpperLimit where {0}=@{0} and {1}=@{1} and DATEDIFF(DD,{2},@{2})=0",
                BetUpperLimit.COMPANYID, BetUpperLimit.GAMEPLAYWAYID, BetUpperLimit.CREATETIME);
            return base.ExecuteDataTable(sql, new SqlParameter(BetUpperLimit.COMPANYID, companyId),
                new SqlParameter(BetUpperLimit.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(BetUpperLimit.CREATETIME, date));
        }
        public IEnumerable<BetUpperLimit> ListUpperLimitByCondition(DateTime date, string num, int companyId, int gameplayway, int startRow, int endRow)
        {
            string condition;
            var parameterList = BuildCondition(date, num, companyId, gameplayway, out condition);

            string sql = string.Format(@"SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY CompanyId,GamePlayWayId,Num) AS RowNumber,* 
     FROM {0} WHERE {1}
) T
WHERE RowNumber BETWEEN {2} AND {3}", BetUpperLimit.TABLENAME, condition, startRow, endRow);
            return base.ExecuteList<BetUpperLimit>(sql, parameterList.ToArray());
        }
        public int CountUpperLimitByCondition(DateTime date, string num, int companyId, int gameplayway)
        {
            string condition;
            var parameterList = BuildCondition(date, num, companyId, gameplayway, out condition);
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}", BetUpperLimit.TABLENAME, condition);
            object count = base.ExecuteScalar(sql, parameterList.ToArray());
            return Convert.ToInt32(count);
        }

        private IList<SqlParameter> BuildCondition(DateTime date, string num, int companyId, int gameplayway, out string condition, bool hasDrop = true)
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            StringBuilder sb = new StringBuilder();
            Action<string, object> appendToCondition = (key, value) =>
            {
                if (sb.Length > 0)
                    sb.Append(" AND ");
                sb.AppendFormat("{0}=@{0}", key);
                parameterList.Add(new SqlParameter(key, value));
            };
            sb.AppendFormat("CAST({0} as DATE)=CAST(@{0} as DATE)", BetUpperLimit.CREATETIME);
            parameterList.Add(new SqlParameter(BetUpperLimit.CREATETIME, date));
            if (!string.IsNullOrEmpty(num))
                appendToCondition(BetUpperLimit.NUM, num);
            if (companyId > 0)
                appendToCondition(BetUpperLimit.COMPANYID, companyId);
            if (gameplayway > 0)
                appendToCondition(BetUpperLimit.GAMEPLAYWAYID, gameplayway);
            if (hasDrop)
                sb.AppendFormat(" AND {0}>0", BetUpperLimit.DROPVALUE);
            condition = sb.ToString();

            return parameterList;
        }
    }
}
