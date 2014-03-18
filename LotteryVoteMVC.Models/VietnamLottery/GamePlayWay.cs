using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 游戏玩法
    /// </summary>
    public class GamePlayWay
    {
        public const string TABLENAME = "tb_GamePlayWay";
        public const string ID = "Id";
        public const string GAMEID = "GameId";
        public const string WAYID = "WayId";

        public int Id { get; set; }
        public int GameId { get; set; }
        public int WayId { get; set; }
        public GameType GameType
        {
            get
            {
                return (GameType)GameId;
            }
            set
            {
                GameId = (int)value;
            }
        }
        /// <summary>
        /// 若玩法为空，则代表游戏类型本身就是一种玩法.
        /// </summary>
        /// <value>
        /// The play way.
        /// </value>
        public PlayWay PlayWay
        {
            get
            {
                return (PlayWay)WayId;
            }
            set
            {
                WayId = (int)value;
            }
        }
    }
}
