using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 用于同一管理Manager,以便Manager的重用
    /// </summary>
    public class ManagerHelper
    {
        private const string M_Key = "ManagerHelper";
        private static object _lockHelper = new object();
        private static ManagerHelper _managerHelper;

        internal IDictionary<int, ManagerBase> ManagerDic
        {
            get
            {
                var managerDic = HttpContext.Current.Items[M_Key] as IDictionary<int, ManagerBase>;
                if (managerDic == null)
                {
                    managerDic = new Dictionary<int, ManagerBase>();
                    HttpContext.Current.Items.Add(M_Key, managerDic);
                }
                return managerDic;
            }
        }
        private ManagerHelper() { }

        public static ManagerHelper Instance
        {
            get
            {
                if (_managerHelper == null)
                {
                    lock (_lockHelper)
                    {
                        if (_managerHelper == null)
                            _managerHelper = new ManagerHelper();
                    }
                }
                return _managerHelper;
            }
        }

        public T GetManager<T>() where T : ManagerBase
        {
            var key = typeof(T).GetHashCode();
            ManagerBase manager;
            if (!ManagerDic.TryGetValue(key, out manager))
            {
                manager = Activator.CreateInstance<T>();
                ManagerDic.Add(key, manager);
            }
            return (T)manager;
        }
    }
}
