using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class DefaultUpperLimitDataAccess : DataBase
    {
        public void Insert(DefaultUpperLimit limit)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3}) VALUES (@{1},@{2},@{3}) SELECT SCOPE_IDENTITY()",
                DefaultUpperLimit.TABLENAME, DefaultUpperLimit.COMPANYTYPE, DefaultUpperLimit.GAMEPLAYWAYID, DefaultUpperLimit.LIMITAMOUNT);
            object id = base.ExecuteScalar(sql, new SqlParameter(DefaultUpperLimit.COMPANYTYPE, (int)limit.CompanyType),
                new SqlParameter(DefaultUpperLimit.GAMEPLAYWAYID, limit.GamePlayWayId),
                new SqlParameter(DefaultUpperLimit.LIMITAMOUNT, limit.LimitAmount));
            limit.LimitId = Convert.ToInt32(id);
        }
        public void Delete(int limitId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", DefaultUpperLimit.TABLENAME, DefaultUpperLimit.LIMITID);
            base.ExecuteNonQuery(sql, new SqlParameter(DefaultUpperLimit.LIMITID, limitId));
        }

        /// <summary>
        /// 根据公司类型和玩法获取默认上限限制
        /// </summary>
        /// <param name="companyType">Type of the company.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <returns></returns>
        public DefaultUpperLimit GetDefaultUpperLimit(CompanyType companyType, int gameplaywayId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", DefaultUpperLimit.TABLENAME, DefaultUpperLimit.COMPANYTYPE, DefaultUpperLimit.GAMEPLAYWAYID);
            return base.ExecuteModel<DefaultUpperLimit>(sql, new SqlParameter(DefaultUpperLimit.COMPANYTYPE, (int)companyType),
                new SqlParameter(DefaultUpperLimit.GAMEPLAYWAYID, gameplaywayId));
        }

        public IEnumerable<DefaultUpperLimit> GetAll()
        {
            string sql = string.Format(@"SELECT * FROM {0} order by {1},{2}", DefaultUpperLimit.TABLENAME, DefaultUpperLimit.COMPANYTYPE, DefaultUpperLimit.GAMEPLAYWAYID);
            return base.ExecuteList<DefaultUpperLimit>(sql);
        }
    }
}
