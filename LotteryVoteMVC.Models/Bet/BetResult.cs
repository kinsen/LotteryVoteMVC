using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 投注返回结果
    /// </summary>
    public class BetResult
    {
        private IList<BetOrder> _notAcceptOrderList, _dropOrderList;

        /// <summary>
        /// 未接受投注的注单
        /// </summary>
        public IList<BetOrder> NotAcceptOrderList
        {
            get
            {
                if (_notAcceptOrderList == null)
                    _notAcceptOrderList = new List<BetOrder>();
                return _notAcceptOrderList;
            }
        }
        /// <summary>
        /// 被跌水的注单
        /// </summary>
        public IList<BetOrder> DropOrderList
        {
            get
            {
                if (_dropOrderList == null)
                    _dropOrderList = new List<BetOrder>();
                return _dropOrderList;
            }
        }
        public decimal ActualTurnover { get; set; }
        public decimal ExceptTurnover { get; set; }
        public void Append(BetResult result)
        {
            this.ActualTurnover += result.ActualTurnover;
            this.ExceptTurnover += result.ExceptTurnover;
            if (result.NotAcceptOrderList != null && result.NotAcceptOrderList.Count > 0)
            {
                var notAccept = this.NotAcceptOrderList as List<BetOrder>;
                notAccept.AddRange(result.NotAcceptOrderList);
            }

            if (result.DropOrderList != null && result.DropOrderList.Count > 0)
            {
                var dropList = this.DropOrderList as List<BetOrder>;
                dropList.AddRange(result.DropOrderList);
            }
        }
    }
}
