using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models.VietnamLottery
{
    public class CompanyNumLen
    {
        public int CompanyId { get; set; }
        public IList<NumLenItem> NumLenList { get; set; }
        public class NumLenItem
        {
            public int Length { get; set; }
            public int Count { get; set; }
        }
    }
}
