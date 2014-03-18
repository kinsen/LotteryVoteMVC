using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.ComponentModel.DataAnnotations;

namespace LotteryVoteMVC.Models
{
    public class User
    {
        public const string TABLENAME = "tb_User";
        public const string USERID = "UserId";
        public const string ROLEID = "RoleId";
        public const string USERNAME = "UserName";
        public const string PARENTID = "ParentId";

        public DataRow DataRow { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        [Required]
        public string UserName { get; set; }
        public int ParentId { get; set; }
        public DateTime? LastLoginTime { get; set; }
        private UserInfo _userInfo;
        [Required]
        public UserInfo UserInfo
        {
            get
            {
                if (_userInfo == null && this.DataRow != null)
                    _userInfo = ModelParser<UserInfo>.ParseModel(DataRow);
                return _userInfo;
            }
            set
            {
                _userInfo = value;
            }
        }
        public Role Role
        {
            get
            {
                return (Models.Role)RoleId;
            }
            set
            {
                RoleId = (int)value;
            }
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}:{1} ", USERID, UserId);
            sb.AppendFormat("Role:{0} ", Role);
            sb.AppendFormat("{0}:{1} ", USERNAME, UserName);
            sb.AppendFormat("{0}:{1} ", PARENTID, ParentId);
            return sb.ToString();
        }
    }
}
