using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility.Validator;

namespace LotteryVoteMVC.Models
{
    public class RateGroupBetLimit
    {
        public const string TABLENAME = "tb_RateGroupBetLimit";
        public const string GROUPID = "GroupId";
        public const string GAMEID = "GameId";
        public const string LEASTLIMIT = "LeastLimit";
        public const string LARGESTLIMIT = "LargestLimit";

        public int GroupId { get; set; }
        public int GameId { get; set; }
        public GameType GameType
        {
            set
            {
                GameId = (int)value;
            }
            get
            {
                return (GameType)GameId;
            }
        }
        [Numeric]
        public decimal LeastLimit { get; set; }
        [Numeric]
        public decimal LargestLimit { get; set; }
    }
}
