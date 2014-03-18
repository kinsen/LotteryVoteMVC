using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core.Limit
{
    internal interface ILimitCheck
    {
        bool IsDrop { get; }
        LimitChecker BaseChecker { get; set; }
        bool Inject(BetOrder order);
        void RollOrderLimit(BetOrder order);
    }
}
