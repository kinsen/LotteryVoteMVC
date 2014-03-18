using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;
using LotteryVoteMVC.Utility;
using System.Text.RegularExpressions;
using System.Data;

namespace LotteryVoteMVC.Data
{
    public class DropWaterDataAccess : DataBase
    {
        public void Insert(DropWater dropWater)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6}) VALUES (@{1},@{2},@{3},@{4},@{5},@{6}) SELECT SCOPE_IDENTITY()",
                DropWater.TABLENAME, DropWater.NUM, DropWater.GAMEPLAYWAYID, DropWater.DROPVALUE, DropWater.AMOUNT, DropWater.DROPTYPE, DropWater.COMPANYTYPE);
            object id = base.ExecuteScalar(sql, new SqlParameter(DropWater.NUM, dropWater.Num),
                new SqlParameter(DropWater.GAMEPLAYWAYID, dropWater.GamePlayWayId),
                new SqlParameter(DropWater.DROPVALUE, dropWater.DropValue),
                new SqlParameter(DropWater.AMOUNT, dropWater.Amount),
                new SqlParameter(DropWater.DROPTYPE, (int)dropWater.DropType),
                new SqlParameter(DropWater.COMPANYTYPE, dropWater.CompanyType ?? 0));
            dropWater.DropId = Convert.ToInt32(id);
        }
        public void Insert(DropWater dropWater, DateTime date)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5},{6},{7}) VALUES (@{1},@{2},@{3},@{4},@{5},@{6},@{7}) SELECT SCOPE_IDENTITY()",
                DropWater.TABLENAME, DropWater.NUM, DropWater.GAMEPLAYWAYID, DropWater.DROPVALUE, DropWater.AMOUNT, DropWater.DROPTYPE, DropWater.COMPANYTYPE, DropWater.CREATETIME);
            object id = base.ExecuteScalar(sql, new SqlParameter(DropWater.NUM, dropWater.Num),
                new SqlParameter(DropWater.GAMEPLAYWAYID, dropWater.GamePlayWayId),
                new SqlParameter(DropWater.DROPVALUE, dropWater.DropValue),
                new SqlParameter(DropWater.AMOUNT, dropWater.Amount),
                new SqlParameter(DropWater.DROPTYPE, (int)dropWater.DropType),
                new SqlParameter(DropWater.COMPANYTYPE, dropWater.CompanyType ?? 0),
                new SqlParameter(DropWater.CREATETIME, date));
            dropWater.DropId = Convert.ToInt32(id);
        }
        public void Insert(IEnumerable<DropWater> dropWaters)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(DropWater.NUM, typeof(string));
            dt.Columns.Add(DropWater.GAMEPLAYWAYID, typeof(int));
            dt.Columns.Add(DropWater.DROPVALUE, typeof(double));
            dt.Columns.Add(DropWater.DROPTYPE, typeof(int));
            dt.Columns.Add(DropWater.COMPANYTYPE, typeof(int));
            dt.Columns.Add(DropWater.AMOUNT, typeof(decimal));
            foreach (var drop in dropWaters)
            {
                var dr = dt.NewRow();
                dr[DropWater.NUM] = drop.Num;
                dr[DropWater.GAMEPLAYWAYID] = drop.GamePlayWayId;
                dr[DropWater.DROPVALUE] = drop.DropValue;
                dr[DropWater.DROPTYPE] = (int)drop.DropType;
                dr[DropWater.COMPANYTYPE] = (int)drop.CompanyType;
                dr[DropWater.AMOUNT] = drop.Amount;
                dt.Rows.Add(dr);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(DropWater.TABLENAME, dt);
        }
        public void Insert(DropWater dropWater, int companyId)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2})", DropInCompany.TABLENAME, DropInCompany.DROPID, DropInCompany.COMPANYID);
            base.ExecuteNonQuery(sql, new SqlParameter(DropInCompany.DROPID, dropWater.DropId),
                new SqlParameter(DropInCompany.COMPANYID, companyId));
        }
        public void Delete(int dropId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", DropWater.TABLENAME, DropWater.DROPID);
            base.ExecuteNonQuery(sql, new SqlParameter(DropWater.DROPID, dropId));
        }
        public void Delete(string num, int gameplaywayId, double dropValue, decimal amount, CompanyType? companyType, DropType dropType, DateTime? date)
        {
            StringBuilder condition = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            #region 获取号码条件
            if (num.IsNum())
            {
                condition.AppendFormat("{0}=@{0}", DropWater.NUM);
                paramList.Add(new SqlParameter(DropWater.NUM, num));
            }
            else if (num.IsNumArray())
            {
                var nums = num.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                condition.AppendFormat("('|'+@{0}+'|' like '%|'+{0}+'|%')", DropWater.NUM);
                paramList.Add(new SqlParameter(DropWater.NUM, string.Join("|", nums)));
            }
            else if (num.IsRangeNum())
            {
                var nums = num.Split(new[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                condition.AppendFormat("{0} between @{1} and @{2} and len({0})={3}", DropWater.NUM, "FromNum", "ToNum", nums[0].Length);
                paramList.Add(new SqlParameter("FromNum", nums[0]));
                paramList.Add(new SqlParameter("ToNum", nums[1]));
            }
            else if (num.IsBatterNum())
            {
                var numIndex = num.LastIndexOf("-");
                var lastNum = num.Substring(numIndex + 1);
                var preNumLen = num.Length - lastNum.Length;
                string fromNum = lastNum, toNum = lastNum;
                for (int i = 0; i < preNumLen; i++)
                {
                    fromNum = "0" + fromNum;
                    toNum = "9" + toNum;
                }
                condition.AppendFormat("{0} between @{1} and @{2} and len({0})={3}", DropWater.NUM, "FromNum", "ToNum", fromNum.Length);
                paramList.Add(new SqlParameter("FromNum", fromNum));
                paramList.Add(new SqlParameter("ToNum", toNum));
            }
            #endregion
            Action<string, object> appendToCondition = (key, value) =>
            {
                if (condition.Length > 0)
                    condition.Append(" AND ");
                condition.AppendFormat("{0}=@{0}", key);
                paramList.Add(new SqlParameter(key, value));
            };
            if (gameplaywayId != 0)
                appendToCondition(DropWater.GAMEPLAYWAYID, gameplaywayId);
            if (dropValue > 0)
                appendToCondition(DropWater.DROPVALUE, dropValue);
            if (amount > 0)
                appendToCondition(DropWater.AMOUNT, amount);
            if (companyType.HasValue)
                appendToCondition(DropWater.COMPANYTYPE, (int)companyType.Value);
            appendToCondition(DropWater.DROPTYPE, (int)dropType);
            if (date.HasValue)
            {
                if (condition.Length > 0) condition.Append(" AND ");
                condition.AppendFormat(@"CONVERT(char(10),{0},120)=CONVERT(char(10),@{0},120)", DropWater.CREATETIME);
                paramList.Add(new SqlParameter(DropWater.CREATETIME, date.Value));
            }
            string sql = string.Format(@"DELETE {0} WHERE {1}", DropWater.TABLENAME, condition.ToString());
            base.ExecuteNonQuery(sql, paramList.ToArray());
        }

        public DropWater GetDropWater(int dropwaterId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", DropWater.TABLENAME, DropWater.DROPID);
            return base.ExecuteModel<DropWater>(sql, new SqlParameter(DropWater.DROPID, dropwaterId));
        }
        public DropWater GetDropWater(string num, int gameplaywayId, decimal amount, DropType dropType, int? companyType)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3} AND {4}=@{4} AND {5}=@{5}", DropWater.TABLENAME, DropWater.NUM,
                DropWater.GAMEPLAYWAYID, DropWater.AMOUNT, DropWater.DROPTYPE, DropWater.COMPANYTYPE);
            return base.ExecuteModel<DropWater>(sql, new SqlParameter(DropWater.NUM, num),
                new SqlParameter(DropWater.GAMEPLAYWAYID, gameplaywayId),
                new SqlParameter(DropWater.AMOUNT, amount),
                new SqlParameter(DropWater.DROPTYPE, (int)dropType),
                new SqlParameter(DropWater.COMPANYTYPE, companyType));
        }
        public DropWater GetDropWater(int gameplaywayId, decimal amount, DropType dropType, CompanyType companyType)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3} AND {4}=@{4}", DropWater.TABLENAME, DropWater.GAMEPLAYWAYID,
                DropWater.AMOUNT, DropWater.DROPTYPE, DropWater.COMPANYTYPE);
            return base.ExecuteModel<DropWater>(sql, new SqlParameter(DropWater.GAMEPLAYWAYID, gameplaywayId),
                new SqlParameter(DropWater.AMOUNT, amount),
                new SqlParameter(DropWater.DROPTYPE, (int)dropType),
                new SqlParameter(DropWater.COMPANYTYPE, (int)companyType));
        }
        /// <summary>
        /// 列出指定号码的跌水设置.
        /// </summary>
        /// <param name="num">号码.</param>
        /// <param name="gameplaywayId">玩法.</param>
        /// <param name="company">公司.</param>
        /// <param name="date">日期.</param>
        /// <returns></returns>
        public IEnumerable<DropWater> ListNumsDropWater(string num, int gameplaywayId, LotteryCompany company, DateTime date)
        {
            string sql = string.Format(@"select * from {0} dw
left join {1} dc on dc.{2}=dw.{3}
where ((dc.{4}=@{4} and dw.{7}=@{7} and CONVERT(char(10),dw.{5},120)=CONVERT(char(10),@{5},120)) 
or (dw.{6}=@{6} and dw.{9}=@{9})) and dw.{8}=@{8}", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID,
                   DropInCompany.COMPANYID, DropWater.CREATETIME, DropWater.DROPTYPE, DropWater.NUM, DropWater.GAMEPLAYWAYID, DropWater.COMPANYTYPE);
            return base.ExecuteList<DropWater>(sql, new SqlParameter(DropInCompany.COMPANYID, company.CompanyId),
                new SqlParameter(DropWater.CREATETIME, date),
                new SqlParameter(DropWater.DROPTYPE, DropType.Auto),
                new SqlParameter(DropWater.NUM, num),
                new SqlParameter(DropWater.GAMEPLAYWAYID, gameplaywayId),
                new SqlParameter(DropWater.COMPANYTYPE, (int)company.CompanyType));
        }
        public IEnumerable<DropWater> GetMininumDropWater(string num, int gameplaywayId, LotteryCompany company, DateTime date)
        {
            string sql = string.Format(@";WITH CTE AS(select dw.*,dc.CompanyId from {0} dw
left join {1} dc on dc.{2}=dw.{3}
where ((dc.{4}=@{4} and dw.{7}=@{7} and CONVERT(char(10),dw.{5},120)=CONVERT(char(10),@{5},120)) 
or (dw.{6}=@{6} and dw.{9}=@{9})) and dw.{8}=@{8}) select * from CTE where Amount=(select top 1 Amount from CTE order by Amount asc)", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID,
                       DropInCompany.COMPANYID, DropWater.CREATETIME, DropWater.DROPTYPE, DropWater.NUM, DropWater.GAMEPLAYWAYID, DropWater.COMPANYTYPE);
            return base.ExecuteList<DropWater>(sql, new SqlParameter(DropInCompany.COMPANYID, company.CompanyId),
                new SqlParameter(DropWater.CREATETIME, date),
                new SqlParameter(DropWater.DROPTYPE, DropType.Auto),
                new SqlParameter(DropWater.NUM, num),
                new SqlParameter(DropWater.GAMEPLAYWAYID, gameplaywayId),
                new SqlParameter(DropWater.COMPANYTYPE, (int)company.CompanyType));
        }
        public IEnumerable<DropWater> ListDropWater(string num, DateTime date)
        {
            string sql = string.Format(@"SELECT DW.*,DIC.{6} FROM {0} DW 
LEFT JOIN {1} DIC ON DIC.{2}=DW.{3} WHERE DW.{4}=@{4} AND CONVERT(char(10),DW.{5},120)=CONVERT(char(10),@{5},120)
", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID, DropWater.NUM, DropWater.CREATETIME, DropInCompany.COMPANYID);
            return base.ExecuteList<DropWater>(sql, new SqlParameter(DropWater.NUM, num),
                new SqlParameter(DropWater.CREATETIME, date));
        }
        public IEnumerable<DropWater> ListDropWater(DropType dropType, int start, int end, DateTime? date = null)
        {
            string dateCondition = date.HasValue ? string.Format(@" AND CONVERT(char(10),{0},120)=CONVERT(char(10),@{0},120)", DropWater.CREATETIME) : string.Empty;

            string sql = string.Format(@";WITH CTE AS(select  dw.*,dic.CompanyId from {0} dw
left join {1} dic on dic.{2}=dw.{3}
where dw.{4}=@{4} {9})
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY CompanyType,GamePlayWayId, {6},Amount ) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {7} AND {8}", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID,
                           DropWater.DROPTYPE, DropWater.CREATETIME, DropWater.NUM, start, end, dateCondition);

            var paramArr = new SqlParameter[date.HasValue ? 2 : 1];
            paramArr[0] = new SqlParameter(DropWater.DROPTYPE, (int)dropType);
            if (date.HasValue)
                paramArr[1] = new SqlParameter(DropWater.CREATETIME, date);

            return base.ExecuteList<DropWater>(sql, paramArr);
        }
        public IEnumerable<DropWater> ListDropWaterByCondition(DropType dropType, DateTime? date, string num, int companyId,
            int gameplaywayId, double dropvalue, decimal amount, int start, int end)
        {
            string condition;
            var parameterList = BuildCondition(dropType, date, num, companyId, gameplaywayId, dropvalue, amount, out condition);

            string sql = string.Format(@";WITH CTE AS(select  dw.*,dic.CompanyId from {0} dw
left join {1} dic on dic.{2}=dw.{3}
where {4})
SELECT * FROM 
(
     SELECT ROW_NUMBER() OVER(ORDER BY {5} desc) AS RowNumber,* 
     FROM CTE
) T
WHERE RowNumber BETWEEN {6} AND {7}", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID,
                           condition.ToString(), DropWater.NUM, start, end);
            return base.ExecuteList<DropWater>(sql, parameterList.ToArray());
        }
        public IEnumerable<DropWater> GetDropWaterByCondition(string num, LotteryCompany company, int gameplayway, decimal amount, DateTime date)
        {
            string sql = string.Format(@";WITH CTE AS(select  dw.*,dic.CompanyId from tb_DropWater dw
left join tb_DropInCompany dic on dic.DropId=dw.DropId
where {1}=@{1} AND {2}=@{2}
and ((DropType={3} and {5}=@{5}) or (DropType={4} and {0}=@{0} AND {6}=@{6} and CAST({7} as Date)=CAST(@{7} as DATE)))
)
select * from CTE", DropWater.NUM, DropWater.GAMEPLAYWAYID, DropWater.AMOUNT, (int)DropType.Auto, (int)DropType.Manual, DropWater.COMPANYTYPE, DropInCompany.COMPANYID, DropWater.CREATETIME);
            return base.ExecuteList<DropWater>(sql, new SqlParameter(DropWater.NUM, num),
                new SqlParameter(DropWater.AMOUNT, amount),
                new SqlParameter(DropWater.GAMEPLAYWAYID, gameplayway),
                new SqlParameter(DropWater.COMPANYTYPE, (int)company.CompanyType),
                new SqlParameter(DropInCompany.COMPANYID, company.CompanyId),
                new SqlParameter(DropWater.CREATETIME, date));
        }

        #region Count
        public int CountDropWater(int dropId, int companyId)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND {2}=@{2}", DropInCompany.TABLENAME, DropInCompany.DROPID, DropInCompany.COMPANYID);
            object count = base.ExecuteScalar(sql, new SqlParameter(DropInCompany.DROPID, dropId),
                new SqlParameter(DropInCompany.COMPANYID, companyId));
            return Convert.ToInt32(count);
        }
        public int CountAutoDropWater(CompanyType companyType, int gameplaywayId, decimal amount)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND {2}=@{2} AND {3}=@{3}", DropWater.TABLENAME, DropWater.COMPANYTYPE,
                DropWater.GAMEPLAYWAYID, DropWater.AMOUNT);
            object count = base.ExecuteScalar(sql, new SqlParameter(DropWater.COMPANYTYPE, (int)companyType),
                new SqlParameter(DropWater.GAMEPLAYWAYID, gameplaywayId),
                new SqlParameter(DropWater.AMOUNT, amount));
            return Convert.ToInt32(count);
        }
        public int CountDropWater(DropType dropType, DateTime? date = null)
        {
            string dateCondition = date.HasValue ? string.Format(@" AND CONVERT(char(10),dw.{0},120)=CONVERT(char(10),@{0},120)", DropWater.CREATETIME) : string.Empty;
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} dw
LEFT JOIN {1} dic ON dic.{2}=dw.{3}
WHERE dw.{4}=@{4} {5}", DropWater.TABLENAME, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID, DropWater.DROPTYPE, dateCondition);

            var paramArr = new SqlParameter[date.HasValue ? 2 : 1];
            paramArr[0] = new SqlParameter(DropWater.DROPTYPE, (int)dropType);
            if (date.HasValue)
                paramArr[1] = new SqlParameter(DropWater.CREATETIME, date);
            object count = base.ExecuteScalar(sql, paramArr);
            return Convert.ToInt32(count);
        }
        public int CountDropWaterByCondition(DropType dropType, DateTime? date, string num, int companyId,
            int gameplaywayId, double dropvalue, decimal amount)
        {
            string condition;
            var parameterList = BuildCondition(dropType, date, num, companyId, gameplaywayId, dropvalue, amount, out condition);
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} dw
LEFT JOIN {2} dic ON dic.{3}=dw.{4}
WHERE {1}", DropWater.TABLENAME, condition, DropInCompany.TABLENAME, DropInCompany.DROPID, DropWater.DROPID);
            object count = base.ExecuteScalar(sql, parameterList.ToArray());
            return Convert.ToInt32(count);
        }
        #endregion

        private IList<SqlParameter> BuildCondition(DropType dropType, DateTime? date, string num, int companyId,
           int gameplaywayId, double dropvalue, decimal amount, out string condit)
        {
            List<SqlParameter> parameterList = new List<SqlParameter>();
            StringBuilder condition = new StringBuilder();
            Action<string, object> appendToCondtion = (it, arg) =>
            {
                if (condition.Length > 0)
                    condition.Append(" AND ");
                condition.AppendFormat("{0}=@{0}", it);
                parameterList.Add(new SqlParameter(it, arg));
            };
            if (!string.IsNullOrEmpty(num))
            {
                if (num.IsNum())
                    appendToCondtion(DropWater.NUM, num);
                else if (new Regex(@"^-+\d+$").IsMatch(num))
                {
                    if (condition.Length > 0)
                        condition.Append(" AND ");
                    int numIndex = num.LastIndexOf("-") + 1;
                    string n = num.Substring(numIndex);
                    string placeHolder = string.Empty;
                    for (int i = 0; i < num.Length - n.Length; i++)
                        placeHolder += "_";
                    condition.AppendFormat("{0} like @{0}", DropWater.NUM);
                    parameterList.Add(new SqlParameter(DropWater.NUM, placeHolder + n));
                }
            }
            if (date.HasValue)
            {
                if (condition.Length > 0)
                    condition.Append(" AND ");
                condition.AppendFormat("CONVERT(char(10),{0},120)=CONVERT(char(10),@{0},120)", DropWater.CREATETIME);
                parameterList.Add(new SqlParameter(DropWater.CREATETIME, date));
            }
            if (companyId > 0)
                appendToCondtion(DropInCompany.COMPANYID, companyId);
            if (gameplaywayId > 0)
                appendToCondtion(DropWater.GAMEPLAYWAYID, gameplaywayId);
            if (dropvalue > 0)
                appendToCondtion(DropWater.DROPVALUE, dropvalue);
            if (amount > 0)
                appendToCondtion(DropWater.AMOUNT, amount);
            appendToCondtion(DropWater.DROPTYPE, (int)dropType);
            condit = condition.ToString();
            return parameterList;
        }
    }
}
