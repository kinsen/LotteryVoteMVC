using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class FreezeFundsDataAccess : DataBase
    {
        public void Insert(FreezeFunds freezeFund)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3}) VALUES (@{1},@{2},@{3}) SELECT SCOPE_IDENTITY()",
                FreezeFunds.TABLENAME, FreezeFunds.USERID, FreezeFunds.AMOUNT, FreezeFunds.STATUS);
            object id = base.ExecuteScalar(sql, new SqlParameter(FreezeFunds.USERID, freezeFund.UserId),
            new SqlParameter(FreezeFunds.AMOUNT, freezeFund.Amount),
            new SqlParameter(FreezeFunds.STATUS, (int)freezeFund.Status));
            freezeFund.FreezeId = Convert.ToInt32(id);
        }
        public void Update(FreezeFunds freezeFund)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1},{2}=@{2} WHERE {3}=@{3}",
                FreezeFunds.TABLENAME, FreezeFunds.AMOUNT, FreezeFunds.STATUS, FreezeFunds.FREEZEID);
            base.ExecuteNonQuery(sql, new SqlParameter(FreezeFunds.AMOUNT, freezeFund.Amount),
                new SqlParameter(FreezeFunds.STATUS, freezeFund.Status),
                new SqlParameter(FreezeFunds.FREEZEID, freezeFund.FreezeId));
        }


        public FreezeFunds GetFreezeFundsByUser(int userId, BetStatus status)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", FreezeFunds.TABLENAME, FreezeFunds.USERID, FreezeFunds.STATUS);
            return base.ExecuteModel<FreezeFunds>(sql, new SqlParameter(FreezeFunds.USERID, userId),
                new SqlParameter(FreezeFunds.STATUS, (int)status));
        }
    }
}
