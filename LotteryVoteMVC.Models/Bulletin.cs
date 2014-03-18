using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 公告
    /// </summary>
    [Serializable]
    public class Bulletin
    {
        public string BulletinId { get; set; }
        public string Content { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
