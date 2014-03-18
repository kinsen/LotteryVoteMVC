using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Data
{
    public class LotteryResultDataAccess : DataBase
    {
        public void InsertPeriod(LotteryPeriod period)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1}) VALUES (@{1}) SELECT SCOPE_IDENTITY()", LotteryPeriod.TABLENAME, LotteryPeriod.COMPANYID);
            object id = base.ExecuteScalar(sql, new SqlParameter(LotteryPeriod.COMPANYID, period.Company.CompanyId));
            period.PeriodId = Convert.ToInt32(id);
        }
        public void DeleteRecord(LotteryPeriod period)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", LotteryRecord.TABLENAME, LotteryRecord.PERIODID);
            base.ExecuteNonQuery(sql, new SqlParameter(LotteryRecord.PERIODID, period.PeriodId));
        }

        public void InsertRecord(IEnumerable<LotteryRecord> records)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(LotteryRecord.PERIODID, typeof(int));
            dt.Columns.Add(LotteryRecord.VALUE, typeof(string));
            foreach (var record in records)
            {
                var row = dt.NewRow();
                row[LotteryRecord.PERIODID] = record.PeriodId;
                row[LotteryRecord.VALUE] = record.Value;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(LotteryRecord.TABLENAME, dt);
        }

        public DataTable GetResultByDate(DateTime date)
        {
            string sql = string.Format(@"select * from tb_LotteryCompany AS LC
JOIN tb_LotteryPeriod LP ON LP.CompanyId=LC.CompanyId
JOIN tb_LotteryRecord LR ON LR.PeriodId=LP.PeriodId
WHERE CAST(LP.CreateDate as Date)=CAST(@{0} as DATE)", LotteryPeriod.CREATEDATE);
            return base.ExecuteDataTable(sql, new SqlParameter(LotteryPeriod.CREATEDATE, date));
        }
        public LotteryPeriod GetPeriod(LotteryCompany company, DateTime date)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND CONVERT(char(10),{2},120)=CONVERT(char(10),@{2},120)", LotteryPeriod.TABLENAME, LotteryPeriod.COMPANYID, LotteryPeriod.CREATEDATE);
            return base.ExecuteModel<LotteryPeriod>(sql, new SqlParameter(LotteryPeriod.COMPANYID, company.CompanyId),
                new SqlParameter(LotteryPeriod.CREATEDATE, date));
        }
        /// <summary>
        /// 根据公司和日期获取开奖结果.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public IEnumerable<LotteryRecord> ListRecord(LotteryCompany company, DateTime date)
        {
            string sql = string.Format(@"SELECT LR.* FROM {0} LR
JOIN {1} LP ON LP.{2}=LR.{3}
WHERE CONVERT(char(10),LP.{4},120)=CONVERT(char(10),@{4},120) AND LP.{5}=@{5}", LotteryRecord.TABLENAME,
            LotteryPeriod.TABLENAME, LotteryPeriod.PERIODID, LotteryRecord.PERIODID, LotteryPeriod.CREATEDATE, LotteryPeriod.COMPANYID);
            return base.ExecuteList<LotteryRecord>(sql, new SqlParameter(LotteryPeriod.CREATEDATE, date),
                new SqlParameter(LotteryPeriod.COMPANYID, company.CompanyId));
        }
    }
}
