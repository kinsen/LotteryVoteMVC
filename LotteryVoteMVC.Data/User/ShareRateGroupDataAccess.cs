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
            string sql = string.Format(@"SELECT * FROM {0}", ShareRateGroup.TABLENAME);
            return base.ExecuteList<ShareRateGroup>(sql);
        }

        public IEnumerable<ShareRateGroup> ListChildGroup(double rate)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}<=@{1}", ShareRateGroup.TABLENAME, ShareRateGroup.SHARERATE);
            return base.ExecuteList<ShareRateGroup>(sql, new SqlParameter(ShareRateGroup.SHARERATE, rate));
        }
    }
}
