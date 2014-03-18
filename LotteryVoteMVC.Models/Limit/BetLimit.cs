using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility.Validator;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 注单限制
    /// </summary>
    public class BetLimit
    {
        public const string TABLENAME = "tb_BetLimit";
        public const string USERID = "UserId";
        public const string GAMEID = "GameId";
        public const string LEASTLIMIT = "LeastLimit";
        public const string LARGESTLIMIT = "LargestLimit";

        public int UserId { get; set; }
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
