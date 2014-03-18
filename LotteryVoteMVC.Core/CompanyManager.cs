using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    public class CompanyManager : ManagerBase
    {
        private CompanyDataAccess _daCompany;
        private NumLengthDataAccess _daNumLen;
        private CompanyLotteryCycleDataAccess _daCompanyCycle;
        private LotteryResultDataAccess _daLotteryResult;
        public CompanyDataAccess DaCompany
        {
            get
            {
                if (_daCompany == null)
                    _daCompany = new CompanyDataAccess();
                return _daCompany;
            }
        }
        public NumLengthDataAccess DaNumLen
        {
            get
            {
                if (_daNumLen == null)
                    _daNumLen = new NumLengthDataAccess();
                return _daNumLen;
            }
        }
        public CompanyLotteryCycleDataAccess DaCompanyCycle
        {
            get
            {
                if (_daCompanyCycle == null)
                    _daCompanyCycle = new CompanyLotteryCycleDataAccess();
                return _daCompanyCycle;
            }
        }
        public LotteryResultDataAccess DaLotteryResult
        {
            get
            {
                if (_daLotteryResult == null)
                    _daLotteryResult = new LotteryResultDataAccess();
                return _daLotteryResult;
            }
        }


        #region Action
        public void Add(LotteryCompany company, User user)
        {
            if (user.Role != Role.Company) throw new NoPermissionException("Add Company", user.ToString());
            DaCompany.Insert(company);
            LotterySystem.ClearAllCompany();
        }
        /// <summary>
        /// 更新开奖公司
        /// </summary>
        /// <param name="company">The company.</param>
        /// <param name="user">The user.</param>
        public void Update(LotteryCompany company, User user)
        {
            if (user.Role != Role.Company) throw new NoPermissionException("Update Company", user.ToString());
            DaCompany.Update(company);
            LotterySystem.ClearAllCompany();
        }
        /// <summary>
        /// 删除开奖公司
        /// </summary>
        /// <param name="companyId">The company id.</param>
        /// <param name="user">The user.</param>
        public void Remove(int companyId, User user)
        {
            if (user.Role != Role.Company)
                throw new NoPermissionException("Delete company", user.ToString());
            DaCompany.Delete(companyId);
            LotterySystem.ClearAllCompany();        //清空公司列表缓存
        }
        #endregion

        #region Select
        public LotteryCompany GetCompany(int companyId)
        {
            return DaCompany.GetCompany(companyId);
        }
        /// <summary>
        /// 获取今日开奖公司
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LotteryCompany> GetTodayLotteryCompany()
        {
            return GetLotteryCompanyByDate(DateTime.Today);
        }
        public IEnumerable<LotteryCompany> GetLotteryCompanyByDate(DateTime date)
        {
            return DaCompany.GetCompanyByDay(date.DayOfWeek);
        }
        /// <summary>
        /// 获取指定公司支持的号码长度.
        /// </summary>
        /// <param name="company">The company.</param>
        /// <returns></returns>
        public IEnumerable<CompanyTypeSupportNumLen> GetNumLengthByCompany(LotteryCompany company)
        {
            return GetNumLenthByCompanyType(company.CompanyType);
        }
        /// <summary>
        /// 获取指定公司类型支持的号码长度
        /// </summary>
        /// <param name="companyType">Type of the company.</param>
        /// <returns></returns>
        public IEnumerable<CompanyTypeSupportNumLen> GetNumLenthByCompanyType(CompanyType companyType)
        {
            return DaNumLen.GetSupportNumLengthByType(companyType);
        }
        public IEnumerable<LotteryCompany> GetAllCompany()
        {
            return DaCompany.GetAll();
        }
        #endregion

        #region Cycles
        public void UpdateCycles(int companyId, IEnumerable<DayOfWeek> days)
        {
            DaCompanyCycle.ExecuteWithTransaction(() =>
            {
                DaCompanyCycle.Delete(companyId);
                foreach (var day in days)
                    DaCompanyCycle.Insert(companyId, day);
            });
        }
        public IEnumerable<CompanyLotteryCycle> GetCycles(LotteryCompany company)
        {
            return DaCompanyCycle.GetCycles(company);
        }
        #endregion

        #region LotteryResult
        public void AddLotteryResult(int companyId, IEnumerable<LotteryRecord> records)
        {
            var company = TodayLotteryCompany.Instance.GetTodayCompany().Find(it => it.CompanyId == companyId);
            var lenSupport = DaNumLen.GetSupportNumLengthByType(company.CompanyType);
            foreach (var support in lenSupport)
            {
                var numLenCount = records.Where(it => it.Value.Length == support.NumLen.Length).Count();
                if (numLenCount != support.Count)
                    throw new BusinessException("Please confirm the accuracy of the numbers!");
            }
            bool isNewData = true;
            var period = DaLotteryResult.GetPeriod(company, DateTime.Today);
            if (isNewData = (period == null))
            {
                period = new LotteryPeriod
                {
                    Company = company
                };
                DaLotteryResult.InsertPeriod(period);
            }
            records = records.ForEach(it => { it.PeriodId = period.PeriodId; return it; });
            DaLotteryResult.ExecuteWithTransaction(() =>
            {
                if (!isNewData)
                    DaLotteryResult.DeleteRecord(period);
                DaLotteryResult.InsertRecord(records);
            });
        }
        /// <summary>
        /// 获取指定日期的开奖结果
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="patchNull">是否填补空的开奖结果（例如今日HL还没开奖，为了显示，则patchNull设置为true，则可看到27个空白的项）.</param>
        /// <returns></returns>
        public IEnumerable<LotteryResult> GetLotteryResultByDate(DateTime date, bool patchNull = true)
        {
            var data = DaLotteryResult.GetResultByDate(date);
            var result = from x in data.AsEnumerable()
                         group x by x.Field<int>(LotteryPeriod.COMPANYID) into gp
                         select new LotteryResult(ModelParser<LotteryCompany>.ParseModel(gp.FirstOrDefault()), ModelParser<LotteryRecord>.ParseModels(gp), date);
            if (date == DateTime.Today && patchNull)
            {
                List<LotteryResult> resultList = new List<LotteryResult>(result);
                foreach (var company in TodayLotteryCompany.Instance.GetTodayCompany())
                {
                    if (!resultList.Contains(it => it.Company.CompanyId == company.CompanyId))
                    {
                        var companyNumLen = DaNumLen.GetSupportNumLengthByType(company.CompanyType).Sum(it => it.Count);
                        resultList.Add(new LotteryResult(company, new LotteryRecord[companyNumLen], date));
                    }
                }
                result = resultList;
            }
            return result.OrderBy(it => it.Company.CompanyId);
        }
        #endregion
    }
}
