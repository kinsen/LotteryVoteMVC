using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class ShareRateGroupDataAccess : DataBase
    {
        public void AddGroup(ShareRateGroup group)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()",
                ShareRateGroup.TABLENAME, ShareRateGroup.NAME, ShareRateGroup.SHARERATE);
            base.ExecuteNonQuery(sql, new SqlParameter(ShareRateGroup.NAME, group.Name),
                new SqlParameter(ShareRateGroup.SHARERATE, group.ShareRate));
        }

        public ShareRateGroup GetGroup(int groupId)
        {
            string sql = string.Format(@"SELECT * FROM {0} where {1}=@{1}", ShareRateGroup.TABLENAME, ShareRateGroup.ID);
            var group = base.ExecuteModel<ShareRateGroup>(sql, new SqlParameter(ShareRateGroup.ID, groupId));
            return group;
        }

        public IEnumerable<ShareRateGroup> ListGroup()
        {
            string sql = string.Format(@"SELECT * FROM {0} ORDER BY {1} DESC", ShareRateGroup.TABLENAME, ShareRateGroup.SHARERATE);
            return base.ExecuteList<ShareRateGroup>(sql);
        }

        public IEnumerable<ShareRateGroup> ListChildGroup(double rate)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}<=@{1} OR {1}=0", ShareRateGroup.TABLENAME, ShareRateGroup.SHARERATE);
            return base.ExecuteList<ShareRateGroup>(sql, new SqlParameter(ShareRateGroup.SHARERATE, rate));
        }

        public void Remove(int groupId)
        {
            base.ExecuteWithTransaction(() =>
            {
                string sql_base = @"DELETE {0} WHERE {1}=@{1}";
                string sql = string.Format(sql_base, RateGroupBetLimit.TABLENAME, RateGroupBetLimit.GROUPID);
                base.ExecuteNonQuery(sql, new SqlParameter(RateGroupBetLimit.GROUPID, groupId));

                sql = string.Format(sql_base, RateGroupGameBetLimit.TABLENAME, RateGroupGameBetLimit.GROUPID);
                base.ExecuteNonQuery(sql, new SqlParameter(RateGroupGameBetLimit.GROUPID, groupId));

                sql = string.Format(sql_base, ShareRateGroup.TABLENAME, ShareRateGroup.ID);
                base.ExecuteNonQuery(sql, new SqlParameter(ShareRateGroup.ID, groupId));
            });

        }
    }
}
