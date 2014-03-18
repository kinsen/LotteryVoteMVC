using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class UserCommissionDataAccess : DataBase
    {
        public void InsertUserCommission(UserCommission userComms)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2})  SELECT SCOPE_IDENTITY()",
                UserCommission.TABLENAME, UserCommission.USERID, UserCommission.SPECIEID);
            object id = base.ExecuteScalar(sql, new SqlParameter(UserCommission.USERID, userComms.UserId),
                new SqlParameter(UserCommission.SPECIEID, userComms.SpecieId));
            userComms.CommissionId = Convert.ToInt32(id);
        }

        /// <summary>
        /// 获取用户的佣金信息
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public UserCommission GetUserCommission(User user, LotterySpecies specie)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", UserCommission.TABLENAME, UserCommission.USERID, UserCommission.SPECIEID);
            return base.ExecuteModel<UserCommission>(sql, new SqlParameter(UserCommission.USERID, user.UserId), new SqlParameter(UserCommission.SPECIEID, (int)specie));
        }
    }
}
