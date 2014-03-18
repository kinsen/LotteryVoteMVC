using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class CompanyDataAccess : DataBase
    {
        public void Insert(LotteryCompany company)
        {
            string sql = string.Format(@"Insert into {0} ({1},{2},{3},{4},{5},{6})values(@{1},@{2},@{3},@{4},@{5},@{6})  SELECT SCOPE_IDENTITY()",
                LotteryCompany.TABLENAME, LotteryCompany.NAME, LotteryCompany.ABBREVIATION, LotteryCompany.TYPEID,
                LotteryCompany.REGION, LotteryCompany.OPENTIME, LotteryCompany.CLOSETIME);
            object id = base.ExecuteScalar(sql, new SqlParameter(LotteryCompany.NAME, company.Name),
                new SqlParameter(LotteryCompany.ABBREVIATION, company.Abbreviation),
                new SqlParameter(LotteryCompany.TYPEID, (int)company.CompanyType),
                new SqlParameter(LotteryCompany.REGION, (int)company.Region),
                new SqlParameter(LotteryCompany.OPENTIME, company.OpenTime),
                new SqlParameter(LotteryCompany.CLOSETIME, company.CloseTime));
            company.CompanyId = Convert.ToInt32(id);
        }
        public void Update(LotteryCompany company)
        {
            string sql = string.Format(@"Update {0} SET {1}=@{1},{2}=@{2},{3}=@{3},{4}=@{4},{5}=@{5},{6}=@{6} WHERE {7}=@{7}",
                LotteryCompany.TABLENAME, LotteryCompany.NAME, LotteryCompany.ABBREVIATION, LotteryCompany.TYPEID,
                LotteryCompany.REGION, LotteryCompany.OPENTIME, LotteryCompany.CLOSETIME, LotteryCompany.COMPANYID);
            base.ExecuteNonQuery(sql, new SqlParameter(LotteryCompany.NAME, company.Name),
            new SqlParameter(LotteryCompany.ABBREVIATION, company.Abbreviation),
            new SqlParameter(LotteryCompany.TYPEID, (int)company.CompanyType),
            new SqlParameter(LotteryCompany.REGION, (int)company.Region),
            new SqlParameter(LotteryCompany.OPENTIME, company.OpenTime),
            new SqlParameter(LotteryCompany.CLOSETIME, company.CloseTime),
            new SqlParameter(LotteryCompany.COMPANYID, company.CompanyId));
        }
        public void Delete(int companyId)
        {
            string sql = string.Format(@"delete {0} WHERE {1}=@{1}", LotteryCompany.TABLENAME, LotteryCompany.COMPANYID);
            base.ExecuteNonQuery(sql, new SqlParameter(LotteryCompany.COMPANYID, companyId));
        }


        public LotteryCompany GetCompany(int companyId)
        {
            string sql = string.Format(@"SELECT lc.*,ct.Id,ct.TypeName,ct.CreateTime as CompanyType_CreateTime FROM {0} as lc JOIN {1} as ct on ct.{2}=lc.{3}
where lc.{4}=@{4}", LotteryCompany.TABLENAME, CompanyTypeModel.TABLENAME,
                CompanyTypeModel.ID, LotteryCompany.TYPEID, LotteryCompany.COMPANYID);
            return base.ExecuteModel<LotteryCompany>(sql, new SqlParameter(LotteryCompany.COMPANYID, companyId));
        }
        /// <summary>
        /// 获取一周某天开奖的公司
        /// </summary>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public IEnumerable<LotteryCompany> GetCompanyByDay(DayOfWeek day)
        {
            string sql = string.Format(@"select lc.*,ct.* from {0} clc
join {1} lc on lc.{2}=clc.{3}
join {4} ct on ct.{5}=lc.{6}
where clc.{7}=@{7} order by lc.{8}", CompanyLotteryCycle.TABLENAME, LotteryCompany.TABLENAME,
                   LotteryCompany.COMPANYID, CompanyLotteryCycle.COMPANYID, CompanyTypeModel.TABLENAME,
                   CompanyTypeModel.ID, LotteryCompany.TYPEID, CompanyLotteryCycle.DAYOFWEEK, LotteryCompany.TYPEID);
            return base.ExecuteList<LotteryCompany>(sql, new SqlParameter(CompanyLotteryCycle.DAYOFWEEK, (int)day));
        }
        public IEnumerable<LotteryCompany> GetAll()
        {
            string sql = string.Format(@"SELECT * FROM {0}", LotteryCompany.TABLENAME);
            return base.ExecuteList<LotteryCompany>(sql);
        }
    }
}
