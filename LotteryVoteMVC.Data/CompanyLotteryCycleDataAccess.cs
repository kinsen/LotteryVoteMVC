using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class CompanyLotteryCycleDataAccess : DataBase
    {
        public void Insert(int companyId, DayOfWeek day)
        {
            string sql = string.Format(@"INSERT INTO {0}  ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()",
                CompanyLotteryCycle.TABLENAME, CompanyLotteryCycle.COMPANYID, CompanyLotteryCycle.DAYOFWEEK);
            base.ExecuteScalar(sql, new SqlParameter(CompanyLotteryCycle.COMPANYID, companyId),
               new SqlParameter(CompanyLotteryCycle.DAYOFWEEK, (int)day));
        }
        public void Delete(int companyId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", CompanyLotteryCycle.TABLENAME, CompanyLotteryCycle.COMPANYID);
            base.ExecuteNonQuery(sql, new SqlParameter(CompanyLotteryCycle.COMPANYID, companyId));
        }
        public IEnumerable<CompanyLotteryCycle> GetCycles(LotteryCompany company)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", CompanyLotteryCycle.TABLENAME, CompanyLotteryCycle.COMPANYID);
            return base.ExecuteList<CompanyLotteryCycle>(sql, new SqlParameter(CompanyLotteryCycle.COMPANYID, company.CompanyId));
        }
    }
}
