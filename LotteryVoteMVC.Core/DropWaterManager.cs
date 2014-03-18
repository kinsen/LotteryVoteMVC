using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Core.Exceptions;
using LotteryVoteMVC.Resources;

namespace LotteryVoteMVC.Core
{
    public class DropWaterManager : ManagerBase
    {
        private DropWaterDataAccess _daDropWater;
        private BetAutoDropWaterDataAccess _daAutoDrop;
        public DropWaterDataAccess DaDropWater
        {
            get
            {
                if (_daDropWater == null)
                    _daDropWater = new DropWaterDataAccess();
                return _daDropWater;
            }
        }
        public BetAutoDropWaterDataAccess DaAutoDrop
        {
            get
            {
                if (_daAutoDrop == null)
                    _daAutoDrop = new BetAutoDropWaterDataAccess();
                return _daAutoDrop;
            }
        }


        #region DropWater
        public void AddTodayDrop(string num, int gameplaywayId, double dropValue, decimal amount, params int[] companys)
        {
            if (num.IsRangeNum() || num.IsBatterNum() || num.IsNumArray() || num.IsStartBatterNum())
            {
                foreach (var n in Extended.GetRangeNum(num))
                    AddTodayDrop(n, gameplaywayId, dropValue, amount, companys);

                return;
            }
            var dws = DaDropWater.ListDropWater(num, DateTime.Today);
            var dw = dws.Find(it => it.Num == num && it.GamePlayWayId == gameplaywayId && it.Amount == amount && it.DropValue == dropValue && it.DropType == DropType.Manual);

            foreach (var company in companys)
            {
                if (dw != null)
                {
                    if (DaDropWater.CountDropWater(dw.DropId, company) > 0)        //公司已添加该跌水的
                        throw new BusinessException(Resource.AlreadyExist);
                    DaDropWater.Insert(dw, company);
                }
                else
                {
                    //同种玩法，同个跌水金额，不同跌水率
                    dw = dws.Find(it => it.Num == num && it.GamePlayWayId == gameplaywayId && it.Amount == amount && it.CompanyId == company);
                    if (dw != null)
                    {
                        throw new BusinessException(Resource.AlreadyExist);
                    }
                    dw = new DropWater
                    {
                        Num = num,
                        DropType = DropType.Manual,
                        DropValue = dropValue,
                        Amount = amount,
                        GamePlayWayId = gameplaywayId
                    };
                    DaDropWater.ExecuteWithTransaction(() =>
                    {
                        DaDropWater.Insert(dw);
                        DaDropWater.Insert(dw, company);
                    });
                }
            }
            UpperLimitManager.GetManager().RefreshLimitNextLimit(num, gameplaywayId);
        }
        public void AddManualDrop(DateTime date, string num, int gameplaywayId, double dropValue, decimal amount, params int[] companys)
        {
            if (num.IsRangeNum() || num.IsBatterNum() || num.IsNumArray() || num.IsStartBatterNum())
            {
                foreach (var n in Extended.GetRangeNum(num))
                    AddManualDrop(date, n, gameplaywayId, dropValue, amount, companys);

                return;
            }
            var dws = DaDropWater.ListDropWater(num, date);
            var dw = dws.Find(it => it.Num == num && it.GamePlayWayId == gameplaywayId && it.Amount == amount && it.DropValue == dropValue && it.DropType == DropType.Manual);

            foreach (var company in companys)
            {
                if (dw != null)
                {
                    if (DaDropWater.CountDropWater(dw.DropId, company) > 0)        //公司已添加该跌水的
                        throw new BusinessException(Resource.AlreadyExist);
                    DaDropWater.Insert(dw, company);
                }
                else
                {
                    //同种玩法，同个跌水金额，不同跌水率
                    dw = dws.Find(it => it.Num == num && it.GamePlayWayId == gameplaywayId && it.Amount == amount && it.CompanyId == company);
                    if (dw != null)
                    {
                        throw new BusinessException(Resource.AlreadyExist);
                    }
                    dw = new DropWater
                    {
                        Num = num,
                        DropType = DropType.Manual,
                        DropValue = dropValue,
                        Amount = amount,
                        GamePlayWayId = gameplaywayId,
                        CreateTime = date
                    };
                    DaDropWater.ExecuteWithTransaction(() =>
                    {
                        DaDropWater.Insert(dw, date);
                        DaDropWater.Insert(dw, company);
                    });
                }
            }
        }
        /// <summary>
        /// 获取手动跌水
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<DropWater> GetManualDrops(DateTime date, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);

            return new PagedList<DropWater>(DaDropWater.ListDropWater(DropType.Manual, start, end, date), pageIndex, pageSize, DaDropWater.CountDropWater(DropType.Manual, date));
        }
        public PagedList<DropWater> GetManualDropByCondition(DateTime date, string num, int companyId, int gameplaywayId, double dropvalue,
            decimal amount, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<DropWater>(DaDropWater.ListDropWaterByCondition(DropType.Manual, date, num, companyId, gameplaywayId, dropvalue, amount, start, end), pageIndex, pageSize,
                DaDropWater.CountDropWaterByCondition(DropType.Manual, date, num, companyId, gameplaywayId, dropvalue, amount));
        }
        /// <summary>
        /// 获取今日指定号码，玩法，公司的跌水
        /// </summary>
        /// <param name="num">The num.</param>
        /// <param name="gameplaywayId">The gameplayway id.</param>
        /// <param name="company">The company.</param>
        /// <returns></returns>
        public IEnumerable<DropWater> GetTodayNumsDropWater(string num, int gameplaywayId, LotteryCompany company)
        {
            return DaDropWater.ListNumsDropWater(num, gameplaywayId, company, DateTime.Today);
        }
        public IEnumerable<DropWater> GetTodayNumMininumDropWaters(string num, int gameplaywayId, LotteryCompany company)
        {
            return DaDropWater.GetMininumDropWater(num, gameplaywayId, company, DateTime.Today);
        }
        #endregion

        #region AutoDrop
        public void AddAutoDrop(int gameplayway, double dropValue, decimal amount, CompanyType companyType)
        {
            //if (num.IsBatterNum() || num.IsRangeNum() || num.IsNumArray())
            //{
            //    AddAutoDrop(GetRangeNum(num), gameplayway, dropValue, amount, companyType);
            //    return;
            //}
            var gpw = LotterySystem.Current.FindGamePlayWay(gameplayway);
            //var drop = DaDropWater.GetDropWater(num, gameplayway, amount, DropType.Auto, (int)companyType);
            var drop = DaDropWater.GetDropWater(gameplayway, amount, DropType.Auto, companyType);
            if (drop != null)
                throw new BusinessException(Resource.AlreadyExist);
            drop = new DropWater
            {
                Num = GetNumArrange(gpw.GameType),
                GamePlayWayId = gameplayway,
                DropValue = dropValue,
                Amount = amount,
                DropType = DropType.Auto,
                CompanyType = (int)companyType
            };
            DaDropWater.Insert(drop);
        }
        public void AddAutoDrop(IEnumerable<string> nums, int gameplayway, double dropValue, decimal amount, CompanyType companyType)
        {
            if (DaDropWater.CountAutoDropWater(companyType, gameplayway, amount) > 0)
                throw new BusinessException(Resource.AlreadyExist);
            List<DropWater> drops = new List<DropWater>();
            foreach (var num in nums)
                drops.Add(new DropWater
                {
                    Num = num,
                    GamePlayWayId = gameplayway,
                    DropValue = dropValue,
                    Amount = amount,
                    DropType = DropType.Auto,
                    CompanyType = (int)companyType
                });
            DaDropWater.Insert(drops);
        }
        public void RemoveAutoDrop(int gameplaywayId, double dropValue, decimal amount, CompanyType companyType)
        {
            DaDropWater.Delete(string.Empty, gameplaywayId, dropValue, amount, companyType, DropType.Auto, null);
        }
        /// <summary>
        /// 获取自动跌水
        /// </summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <returns></returns>
        public PagedList<DropWater> GetAutoDrops(int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);

            return new PagedList<DropWater>(DaDropWater.ListDropWater(DropType.Auto, start, end), pageIndex, pageSize, DaDropWater.CountDropWater(DropType.Auto));
        }
        public PagedList<DropWater> GetAutoDropByCondition(string num, int companyId, int gameplaywayId, double dropvalue,
            decimal amount, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<DropWater>(DaDropWater.ListDropWaterByCondition(DropType.Auto, null, num, companyId, gameplaywayId, dropvalue, amount, start, end), pageIndex, pageSize,
                DaDropWater.CountDropWaterByCondition(DropType.Auto, null, num, companyId, gameplaywayId, dropvalue, amount));
        }
        public IEnumerable<BetAutoDropWater> GetAutoDrops(CompanyType companyType, int gameplaywayId)
        {
            return DaAutoDrop.GetDrop(companyType, gameplaywayId);
        }
        private string GetNumArrange(GameType gameType)
        {
            if (gameType <= GameType.FiveDigital)
            {
                int len = ((int)gameType) + 1;
                string start = 0.ToString("D" + len);
                string end = string.Empty;
                for (int i = 0; i < len; i++)
                    end += "9";
                return string.Format("{0}-{1}", start, end);
            }
            else if (gameType == GameType.PL2)
                return "**#**";
            else
                return "**#**#**";
        }
        #endregion

        #region BetAutoDrop
        public void AddBetAutoDrop(CompanyType companyType, int gameplaywayId, decimal amount, double dropValue)
        {
            if (DaAutoDrop.GetDrops(companyType, gameplaywayId, amount).Count() > 0)
                throw new BusinessException(Resource.AlreadyExist);
            BetAutoDropWater drop = new BetAutoDropWater
            {
                CompanyType = companyType,
                GamePlayWayId = gameplaywayId,
                Amount = amount,
                DropValue = dropValue
            };
            DaAutoDrop.Insert(drop);
        }
        public void RemoveBetAutoDrop(int dropId)
        {
            DaAutoDrop.Delete(dropId);
        }
        public PagedList<BetAutoDropWater> GetAutoBetDrop(int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);
            return new PagedList<BetAutoDropWater>(DaAutoDrop.ListDrop(start, end), pageIndex, pageSize, DaAutoDrop.CountAllDrop());
        }
        public PagedList<BetAutoDropWater> SearchAutoBetDrop(int companyType, int gameplayway, decimal amount, double dropValue, int pageIndex)
        {
            int start = GetStart(pageIndex);
            int end = GetEnd(pageIndex);

            return new PagedList<BetAutoDropWater>(DaAutoDrop.ListByCondition(companyType, gameplayway, amount, dropValue, start, end), pageIndex, pageSize
                , DaAutoDrop.CountByCondition(companyType, gameplayway, amount, dropValue));
        }
        #endregion

        public DropWater SearchTodayDropWater(string num, int companyId, int gameplayway, decimal amount)
        {
            var company = LotterySystem.Current.FindCompany(companyId);
            var dws = DaDropWater.GetDropWaterByCondition(num, company, gameplayway, amount, DateTime.Today);
            DropWater dropwater = null;
            foreach (var dw in dws)
            {
                if (dropwater == null)
                    dropwater = dw;
                else
                {
                    if (dw.DropType == DropType.Manual && dw.DropType == DropType.Auto)
                        dropwater = dw;
                }
            }
            return dropwater;
        }

        public void Remove(int dropId, User user)
        {
            if (user.Role != Role.Company) throw new NoPermissionException("删除跌水", user.ToString());
            var drop = DaDropWater.GetDropWater(dropId);
            if (drop != null)
            {
                DaDropWater.Delete(dropId);
                UpperLimitManager.GetManager().RefreshLimitNextLimit(drop.Num, drop.GamePlayWayId);
            }
        }
    }
}
