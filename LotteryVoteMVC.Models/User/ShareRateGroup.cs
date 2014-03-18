using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class ShareRateGroup
    {
        public const string TABLENAME = "tb_ShareRateGroup";
        public const string ID = "Id";
        public const string NAME = "Name";
        public const string SHARERATE = "ShareRate";
        public const string CREATETIME = "CreateTime";

        public int Id { get; set; }
        public string Name { get; set; }
        public float ShareRate { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
