using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 影子用户所授权的操作
    /// </summary>
    public class ShadowAuthorizeAction
    {
        public const string TableName = "tb_ShadowAuthorizeAction";
        public const string LINKID = "LinkId";
        public const string USERID = "UserId";
        public const string AUTHORIZEID = "AuthorizeId";

        public int LinkId { get; set; }
        public int UserId { get; set; }
        public int AuthorizeId { get; set; }
    }
}
