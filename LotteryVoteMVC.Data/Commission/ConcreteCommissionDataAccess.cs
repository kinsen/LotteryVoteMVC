using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data.Commission
{
    public class ConcreteCommissionDataAccess : DataBase
    {
        public void InsertComms(IEnumerable<ConcreteCommission> comms)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ConcreteCommission.GROUPID, typeof(int));
            dt.Columns.Add(ConcreteCommission.GAMEID, typeof(int));
            dt.Columns.Add(ConcreteCommission.COMPANYTYPEID, typeof(int));
            dt.Columns.Add(ConcreteCommission.COMMISSION, typeof(double));
            dt.Columns.Add(ConcreteCommission.ODDS, typeof(double));
            foreach (var comm in comms)
            {
                var row = dt.NewRow();
                row[ConcreteCommission.GROUPID] = comm.GroupId;
                row[ConcreteCommission.GAMEID] = comm.GameId;
                row[ConcreteCommission.COMPANYTYPEID] = comm.CompanyTypeId;
                row[ConcreteCommission.COMMISSION] = comm.Commission;
                row[ConcreteCommission.ODDS] = comm.Odds;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(ConcreteCommission.TABLENAME, dt);
        }
        public void DeleteComms(int groupId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", ConcreteCommission.TABLENAME, ConcreteCommission.GROUPID);
            base.ExecuteNonQuery(sql, new SqlParameter(ConcreteCommission.GROUPID, groupId));
        }

        /// <summary>
        /// 获取佣金分组的佣金信息
        /// </summary>
        /// <param name="group">The group.</param>
        /// <returns></returns>
        public IEnumerable<ConcreteCommission> GetConcreteCommission(CommissionGroup group)
        {
            string sql = string.Format(@"select * from tb_ConcreteCommission where {0}=@{0}", ConcreteCommission.GROUPID);
            return base.ExecuteList<ConcreteCommission>(sql, new SqlParameter(ConcreteCommission.GROUPID, group.GroupId));
        }

        /// <summary>
        /// 根据市场获取所有佣金组的实际项
        /// </summary>
        /// <param name="specie">The specie.</param>
        /// <returns></returns>
        public IEnumerable<ConcreteCommission> GetConcreteCommission(LotterySpecies specie)
        {
            string sql = string.Format(@"select cc.* from tb_CommissionGroup cg
join tb_ConcreteCommission cc on cc.GroupId=cg.GroupId
where cg.{0}=@{0}", CommissionGroup.SPECIEID);
            return base.ExecuteList<ConcreteCommission>(sql, new SqlParameter(CommissionGroup.SPECIEID, (int)specie));
        }
    }
}
