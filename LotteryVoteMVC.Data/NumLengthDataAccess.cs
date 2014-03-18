using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class NumLengthDataAccess : DataBase
    {
        /// <summary>
        /// 根据公司Id获取该公司所支持的号码长度
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <returns></returns>
        public IEnumerable<CompanyTypeSupportNumLen> GetSupportNumLengthByType(CompanyType companyType)
        {
            string sql = string.Format(@"SELECT * FROM {0} NL
JOIN {1} CSN ON CSN.{2}=NL.{3}
join {4} ct on ct.Id=CSN.CompanyTypeId
WHERE CSN.{5}=@{5}", NumLength.TABLENAME, CompanyTypeSupportNumLen.TABLENAME, NumLength.LENID, CompanyTypeSupportNumLen.LENID,
 CompanyTypeModel.TABLENAME, CompanyTypeSupportNumLen.COMPANYTYPEID);
            return base.ExecuteList<CompanyTypeSupportNumLen>(sql, new SqlParameter(CompanyTypeSupportNumLen.COMPANYTYPEID, (int)companyType));
        }
    }
}
