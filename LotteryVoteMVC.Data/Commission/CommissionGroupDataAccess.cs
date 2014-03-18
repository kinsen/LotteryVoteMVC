using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class CommissionGroupDataAccess : DataBase
    {
        public void Insert(CommissionGroup group)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()",
                CommissionGroup.TABLENAME, CommissionGroup.GROUPNAME, CommissionGroup.SPECIEID);
            object id = base.ExecuteScalar(sql, new SqlParameter(CommissionGroup.GROUPNAME, group.GroupName),
                new SqlParameter(CommissionGroup.SPECIEID, group.SpecieId));
            group.GroupId = Convert.ToInt32(id);
        }
        public void Delete(int groupId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", CommissionGroup.TABLENAME, CommissionGroup.GROUPID);
            base.ExecuteNonQuery(sql, new SqlParameter(CommissionGroup.GROUPID, groupId));
        }

        public CommissionGroup GetCommissionGroup(int groupId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", CommissionGroup.TABLENAME, CommissionGroup.GROUPID);
            return base.ExecuteModel<CommissionGroup>(sql, new SqlParameter(CommissionGroup.GROUPID, groupId));
        }
        public CommissionGroup GetMemebersCommGroup(User member, LotterySpecies specie)
        {
            string sql = string.Format(@"select cg.* from tb_CommissionGroup cg
join tb_MemberPackage mpa on mpa.GroupId=cg.GroupId
where mpa.{0}=@{0} and mpa.{1}=@{1}", MemberPackage.USERID, MemberPackage.SPECIEID);
            return base.ExecuteModel<CommissionGroup>(sql, new SqlParameter(MemberPackage.USERID, member.UserId),
                new SqlParameter(MemberPackage.SPECIEID, (int)specie));
        }
        /// <summary>
        /// 根据市场获取佣金组
        /// </summary>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<CommissionGroup> GetCommissionGroupBySpecie(LotterySpecies specie)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", CommissionGroup.TABLENAME, CommissionGroup.SPECIEID);
            return base.ExecuteList<CommissionGroup>(sql, new SqlParameter(CommissionGroup.SPECIEID, (int)specie));
        }

        public IEnumerable<CommissionGroup> GetCommissionGroupByGroupId(params int[] groupIdArr)
        {
            if (groupIdArr.Length == 0) return new List<CommissionGroup>();
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1} IN (@{1})", CommissionGroup.TABLENAME, CommissionGroup.GROUPID);
            StringBuilder sb = new StringBuilder();
            bool isFirst = true;
            foreach (int groupId in groupIdArr)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.Append(",");
                sb.Append(groupId);
            }
            return base.ExecuteList<CommissionGroup>(sql, new SqlParameter(CommissionGroup.GROUPID, sb.ToString()));
        }
    }
}
