
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 角色
    /// </summary>
    public class RoleModel
    {
        public const string TABLENAME = "tb_Role";
        public const string ROLEID = "RoleId";
        public const string ROLENAME = "RoleName";

        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
