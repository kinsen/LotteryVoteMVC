using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class MemberPackageDataAccess : DataBase
    {
        public void InsertMemberPackage(User user, CommissionGroup group)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3}) VALUES (@{1},@{2},@{3})",
                MemberPackage.TABLENAME, MemberPackage.USERID, MemberPackage.GROUPID, MemberPackage.SPECIEID);
            base.ExecuteNonQuery(sql, new SqlParameter(MemberPackage.USERID, user.UserId),
                new SqlParameter(MemberPackage.GROUPID, group.GroupId),
                new SqlParameter(MemberPackage.SPECIEID, group.SpecieId));
        }
        /// <summary>
        /// 删除会员分组
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        public void DeleteMemeberPackage(User user, LotterySpecies specie)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1} AND {2}=@{2}", MemberPackage.TABLENAME, MemberPackage.USERID, MemberPackage.SPECIEID);
            base.ExecuteNonQuery(sql, new SqlParameter(MemberPackage.USERID, user.UserId),
                new SqlParameter(MemberPackage.SPECIEID, (int)specie));
        }
        public MemberPackage GetMemberPackageBySpecie(User user, LotterySpecies specie)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2}", MemberPackage.TABLENAME, MemberPackage.USERID, MemberPackage.SPECIEID);
            return base.ExecuteModel<MemberPackage>(sql,
                new SqlParameter(MemberPackage.USERID, user.UserId),
                new SqlParameter(MemberPackage.SPECIEID, (int)specie));
        }
    }
}
