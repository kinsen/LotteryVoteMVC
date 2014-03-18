using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Data;

namespace LotteryVoteMVC.Core.Limit
{
    /// <summary>
    /// 限制检查器
    /// </summary>
    public class LimitChecker : IDisposable
    {

        private BetOrderDataAccess _daOrder;
        public BetOrderDataAccess DaOrder
        {
            get
            {
                if (_daOrder == null)
                    _daOrder = new BetOrderDataAccess();
                return _daOrder;
            }
        }

        private ILimitCheck _upperLimiter;
        internal ILimitCheck UpperLimiter
        {
            get
            {
                if (_upperLimiter == null)
                    _upperLimiter = new UpperLimiter();
                return _upperLimiter;
            }
            set
            {
                _upperLimiter = value;
            }
        }

        private ILimitCheck _userLimiter;
        internal ILimitCheck UserLimiter
        {
            get
            {
                if (_userLimiter == null)
                {
                    _userLimiter = new UserLimiter();
                    _userLimiter.BaseChecker = this;
                }
                return _userLimiter;
            }
            set
            {
                _userLimiter = value;
            }
        }

        private ILimitCheck _autoDropLimiter;
        internal ILimitCheck AutoDropLimiter
        {
            get
            {
                if (_autoDropLimiter == null)
                {
                    _autoDropLimiter = new AutoDropLimiter();
                    _autoDropLimiter.BaseChecker = this;
                }
                return _autoDropLimiter;
            }
            set
            {
                _autoDropLimiter = value;
            }
        }

        private IList<BetOrder> _beInsertOrderList;
        /// <summary>
        /// 待插入注单(一次下注可能有多张注单，例如十二生肖，后面的注单检查不会将前面检查过的注单金额也加进去，所以将通过检查的注单放到这里，与数据库中的下注额累加，得到正确的下注额)
        /// </summary>
        public IList<BetOrder> BeInsertOrderList
        {
            get
            {
                if (_beInsertOrderList == null)
                    _beInsertOrderList = new List<BetOrder>();
                return _beInsertOrderList;
            }
        }
        /// <summary>
        ///用户有效注单（DB数据，一次读取出来,内存操作）
        /// </summary>
        /// <value>
        /// The user valid order list.
        /// </value>
        public IList<BetOrder> UserValidOrderList { get; set; }
        public IDictionary<string, BetOrder> UserValidOrderDic { get; set; }

        public bool IsDrop { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 检查上限.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public bool Check(BetOrder order, User user)
        {
            PreLoadUserValidOrder(user);
            bool checkResult;
            checkResult = UserLimiter.Inject(order) && AutoDropLimiter.Inject(order) && UpperLimiter.Inject(order);
            IsDrop = checkResult && (AutoDropLimiter.IsDrop || UpperLimiter.IsDrop);
            return checkResult;
        }
        public void RollLimit(BetOrder order)
        {
            if (order.Status != BetStatus.Valid) return;
            UpperLimiter.RollOrderLimit(order);
        }
        public void UpdateLimit(DataBase tandemDB = null)
        {
        }
        public void Monitor(Action action, Action errorAction)
        {
            try
            {
                action();
            }
            catch
            {
                //将DB中的限制覆盖Cache中的
                //LimitContext.Instance.UpdateDBUplimitToCache();
                if (errorAction != null)
                    errorAction();
                throw;
            }
        }
        /// <summary>
        ///预加载用户的所有有效注单
        /// </summary>
        /// <param name="user">The user.</param>
        private void PreLoadUserValidOrder(User user)
        {
            if (UserValidOrderDic == null)
                UserValidOrderDic = DaOrder.GetOrdersTotals(user, BetStatus.Valid, DateTime.Today).ToDictionary(it => GetBetAmountDicKey(it), it => it);
            //UserValidOrderList = DaOrder.GetOrders(user.UserId, BetStatus.Valid, DateTime.Today).ToList();
        }
        internal string GetBetAmountDicKey(BetOrder order)
        {
            string key = string.Format("{0}_{1}_{2}", order.CompanyId, order.GamePlayWayId, order.Num);
            return key;
        }
        public void Dispose()
        {
            UserValidOrderList.Clear();
            _beInsertOrderList.Clear();
            _beInsertOrderList = null;
        }
    }
}
