using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    /// <summary>
    /// 用户佣金差DB类
    /// </summary>
    public class CommissionValueDataAccess : DataBase
    {
        public void InsertCommValues(IEnumerable<CommissionValue> commValues)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(CommissionValue.COMMISSIONID, typeof(int));
            dt.Columns.Add(CommissionValue.GAMEID, typeof(int));
            dt.Columns.Add(CommissionValue.COMPANYTYPEID, typeof(int));
            dt.Columns.Add(CommissionValue.COMM, typeof(double));
            dt.Columns.Add(CommissionValue.ODDS, typeof(double));
            foreach (var commValue in commValues)
            {
                var row = dt.NewRow();
                row[CommissionValue.COMMISSIONID] = commValue.CommissionId;
                row[CommissionValue.GAMEID] = commValue.GameId;
                row[CommissionValue.COMPANYTYPEID] = commValue.CompanyTypeId;
                row[CommissionValue.COMM] = commValue.Comm;
                row[CommissionValue.ODDS] = commValue.Odds;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(CommissionValue.TABLENAME, dt);
        }
        /// <summary>
        /// 根据用户和市场删除特定的佣金差
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        public void DeleteCommValues(User user, LotterySpecies specie)
        {
            string sql = string.Format(@"delete {0} where {1}=(select {3} from {2} where {4}=@{4} and {5}=@{5})",
                CommissionValue.TABLENAME, CommissionValue.COMMISSIONID, UserCommission.TABLENAME, UserCommission.COMMISSIONID,
                UserCommission.USERID, UserCommission.SPECIEID);
            base.ExecuteNonQuery(sql, new SqlParameter(UserCommission.USERID, user.UserId),
                new SqlParameter(UserCommission.SPECIEID, (int)specie));
        }

        /// <summary>
        /// 获取用户特定市场的佣金差
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<CommissionValue> GetCommissionValue(User user, LotterySpecies specie)
        {
            string sql = string.Format(@"SELECT CV.* FROM {0} CV
JOIN {1} UC on UC.{3}=cv.{2}
where UC.{4}=@{4} and UC.{5}=@{5}", CommissionValue.TABLENAME, UserCommission.TABLENAME, CommissionValue.COMMISSIONID,
             UserCommission.COMMISSIONID, UserCommission.USERID, UserCommission.SPECIEID);
            return base.ExecuteList<CommissionValue>(sql, new SqlParameter(UserCommission.USERID, user.UserId),
                new SqlParameter(UserCommission.SPECIEID, (int)specie));
        }
        /// <summary>
        /// 获取指定用户各父级佣金差（包括自身）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<CommissionValue> GetFamilyUserCommissionValue(User user, LotterySpecies specie)
        {
            string sql = string.Format(@";WITH FAMILY AS
(
SELECT * FROM tb_User  WHERE {4}=@{4}
UNION ALL
SELECT B.* FROM tb_User AS B,FAMILY AS C WHERE B.UserId=C.ParentId and c.UserId>b.UserId
)
SELECT CV.*,u.UserId FROM {0} CV
JOIN {1} UC on UC.{3}=cv.{2}
JOIN FAMILY u on u.UserId=uc.UserId
where  UC.{5}=@{5}", CommissionValue.TABLENAME, UserCommission.TABLENAME, CommissionValue.COMMISSIONID, UserCommission.COMMISSIONID,
                     User.USERID, UserCommission.SPECIEID);
            return base.ExecuteList<CommissionValue>(sql, new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(UserCommission.SPECIEID, (int)specie));
        }
    }
}
