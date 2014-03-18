using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Core.Application;
using System.Web;

namespace LotteryVoteMVC.Core
{
    public abstract class ManagerBase
    {
        protected int pageSize = LotterySystem.Current.PageSize;
        protected DataBase TandemDB { get; private set; }
        protected int GetStart(int pageIndex)
        {
            return (pageIndex - 1) * pageSize + 1;
        }
        protected int GetEnd(int pageIndex)
        {
            return pageIndex * pageSize;
        }
        public void Tandem(ManagerBase manager)
        {
            Tandem(manager.TandemDB);
        }
        public void Tandem(DataBase db)
        {
            var dbPropeties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(it => it.PropertyType.IsSubclassOf(typeof(DataBase)));
            foreach (var pro in dbPropeties)
            {
                var dbPro = (DataBase)pro.GetValue(this, null);
                dbPro.Tandem(db);
            }
            TandemDB = db;
        }
        public void ClearTandem()
        {
            var dbPropeties = this.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                    .Where(it => it.DeclaringType.IsSubclassOf(typeof(DataBase)));
            foreach (var pro in dbPropeties)
            {
                var dbPro = (DataBase)pro.GetValue(this, null);
                dbPro.Transcation = null;
            }
            TandemDB = null;
        }
        protected void PageNotFound()
        {
            throw new HttpException(404, "HTTP/1.1 404 Not Found");
        }
    }
}
