using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class RollbackAmountDataAccess : DataBase
    {
        public void InsertAmounts(IEnumerable<RollbackAmount> rbAmounts)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(RollbackAmount.ID, typeof(int));
            dt.Columns.Add(RollbackAmount.USERID, typeof(int));
            dt.Columns.Add(RollbackAmount.AMOUNT, typeof(decimal));
            dt.Columns.Add(RollbackAmount.TIMETOKEN, typeof(string));
            foreach (var rbamount in rbAmounts)
            {
                var dr = dt.NewRow();
                dr[RollbackAmount.ID] = rbamount.Id;
                dr[RollbackAmount.USERID] = rbamount.UserId;
                dr[RollbackAmount.AMOUNT] = rbamount.Amount;
                dr[RollbackAmount.TIMETOKEN] = rbamount.TimeToken;
                dt.Rows.Add(dr);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(RollbackAmount.TABLENAME, dt);
        }
        /// <summary>
        /// 回滚用户金额
        /// </summary>
        /// <param name="timeToken">The time token.</param>
        public void RollbackAmounts(string timeToken)
        {
            if (UseTransaction)
                base.ExecuteNonQuery(Transcation, CommandType.StoredProcedure, "RollbackUserAmount", new SqlParameter(RollbackAmount.TIMETOKEN, timeToken));
            else
                base.ExecuteNonQuery(CommandType.StoredProcedure, "RollbackUserAmount", new SqlParameter(RollbackAmount.TIMETOKEN, timeToken));
        }
    }
}
