using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 代理可授权的操作所属的角色
    /// </summary>
    public class AgentAuthorizeInRole
    {
        public const string TableName = "tb_AgentAuthorizeInRole";
        public const string AUTHORIZEID = "AuthorizeId";
        public const string ROLEID = "RoleId";

        public int AuthorizeId { get; set; }
        public int RoleId { get; set; }
    }
}
