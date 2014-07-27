using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;
using System.Data;

namespace LotteryVoteMVC.Data
{
    public class UserDataAccess : DataBase
    {
        private readonly string _JoinUserInfoSql = @"select u.*,ui.*,g.*,(select top 1 ll.LastLoginTime from tb_LoginLog ll where ll.UserId=u.UserId order by ll.LoginId desc) as LastLoginTime from tb_User as u 
join tb_UserInfo as ui on ui.UserId=u.UserId
join tb_ShareRateGroup g on g.Id=ui.RateGroupId";

        public void Insert(User user)
        {
            string sql = string.Format(@"Insert into {0} ({1},{2},{3}) values(@{1},@{2},@{3}) SELECT SCOPE_IDENTITY()",
                User.TABLENAME, User.USERNAME, User.ROLEID, User.PARENTID);
            object id = base.ExecuteScalar(sql, new SqlParameter(User.USERNAME, user.UserName),
                new SqlParameter(User.ROLEID, user.RoleId), new SqlParameter(User.PARENTID, user.ParentId));
            user.UserId = Convert.ToInt32(id);
        }
        public void Delete(User user)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", User.TABLENAME, User.USERID);
            base.ExecuteNonQuery(sql, new SqlParameter(User.USERID, user.UserId));
        }

        public User GetUserById(int userId)
        {
            string sql = string.Format(@"{0} where u.UserId=@{1}", _JoinUserInfoSql, User.USERID);
            var user = base.ExecuteModel<User>(sql, new SqlParameter(User.USERID, userId));
            return user;
        }
        public User GetUserByUserName(string userName)
        {
            string sql = string.Format(@"{0} where u.{1}=@{1}", _JoinUserInfoSql, User.USERNAME);
            return base.ExecuteModel<User>(sql, new SqlParameter(User.USERNAME, userName));
        }
        /// <summary>
        /// 获取所有有效用户
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetAllValidUser()
        {
            string sql = string.Format(@"SELECT * FROM tb_User u join tb_UserInfo ui on ui.UserId=u.UserId where ui.{0}=@{0}", UserInfo.STATE);
            return base.ExecuteList<User>(sql, new SqlParameter(UserInfo.STATE, (int)UserState.Active));
        }
        public IEnumerable<User> GetUserByRole(Role role)
        {
            string sql = string.Format(@"{0} WHERE {1}=@{1}", _JoinUserInfoSql, User.ROLEID);
            var returnVal = base.ExecuteList<User>(sql, new SqlParameter(User.ROLEID, (int)role));
            return returnVal;
        }
        /// <summary>
        /// 获取子用户
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IEnumerable<User> GetChild(User user)
        {
            string sql = string.Format(@"{0} WHERE u.{1}=@{1} ORDER BY u.USERID", _JoinUserInfoSql, User.PARENTID);
            return base.ExecuteList<User>(sql, new SqlParameter(User.PARENTID, user.UserId));
        }
        /// <summary>
        /// 获取子用户
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IEnumerable<User> GetChild(User user, Role role)
        {
            string sql = string.Format(@"{0} WHERE u.{1}=@{1} and u.{2}=@{2} ORDER BY u.USERID", _JoinUserInfoSql, User.PARENTID, User.ROLEID);
            return base.ExecuteList<User>(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter(User.ROLEID, (int)role));
        }
        /// <summary>
        /// 获取用户的祖先用户.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IEnumerable<User> GetParentUser(int userId)
        {
            string sql = string.Format(@";WITH CTE AS
(
SELECT * FROM {0}  WHERE {1}=@{1}
UNION ALL
SELECT B.* FROM {0} AS B,CTE AS C WHERE B.{1}=C.{2} and c.{1}>b.{1}
)
SELECT * FROM CTE u
JOIN tb_UserInfo ui on ui.UserId=u.{1}", User.TABLENAME, User.USERID, User.PARENTID);
            return base.ExecuteList<User>(sql, new SqlParameter(User.USERID, userId));
        }
        /// <summary>
        /// 获取用户的子树（包括用户自身）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IEnumerable<User> GetFamily(User user)
        {
            string sql = string.Format(@";WITH CTE AS
(
SELECT * FROM tb_User  WHERE {0}=@{0}
UNION ALL
SELECT B.* FROM tb_User AS B,CTE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
)
select u.*,ui.* from CTE as u join tb_UserInfo as ui on ui.UserId=u.UserId ORDER BY u.USERID", User.USERID);
            return base.ExecuteList<User>(sql, new SqlParameter(User.USERID, user.UserId));
        }
        /// <summary>
        /// 获取今日下注的用户.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<User> GetBetUsers()
        {
            return base.ExecuteList<User>(CommandType.StoredProcedure, "GetBetUser");
        }
        /// <summary>
        /// 根据条件查找用户
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="state">The state.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="sortField">The sort field.</param>
        /// <returns></returns>
        public IEnumerable<User> GetUserByCondition(string name, string userName, UserState state, Role role, User parent, string sortField)
        {
            StringBuilder sb = new StringBuilder();
            List<SqlParameter> paramList = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(name))
            {
                sb.AppendFormat(@"ui.{0}=@{0}", UserInfo.NAME);
                paramList.Add(new SqlParameter(UserInfo.NAME, name));
            }
            if (!string.IsNullOrEmpty(userName))
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.AppendFormat(@"u.{0}=@{0}", User.USERNAME);
                paramList.Add(new SqlParameter(User.USERNAME, userName));
            }

            if (sb.Length > 0)
                sb.Append(" and ");
            sb.AppendFormat(@"ui.{0}=@{0}", UserInfo.STATE);
            paramList.Add(new SqlParameter(UserInfo.STATE, (int)state));

            if (role != default(Role))
            {
                if (sb.Length > 0)
                    sb.Append(" and ");
                sb.AppendFormat(@"u.{0}=@{0}", User.ROLEID);
                paramList.Add(new SqlParameter(User.ROLEID, (int)role));
            }
            string condition = sb.ToString() ?? string.Empty; //sb.Length > 0 ? string.Format(" where {0}", sb.ToString()) : string.Empty;
            string orderStatement = string.IsNullOrEmpty(sortField) ? string.Empty : string.Format(" order by {0}", sortField);

            string sql = string.Format(@"
select u.*,ui.*,(select top 1 ll.LastLoginTime from tb_LoginLog ll where ll.UserId=u.UserId order by ll.LoginId desc) as LastLoginTime
 from tb_User u
join tb_UserInfo ui on ui.UserId=u.UserId
where u.{0}=@{0} and {1} {2}", User.PARENTID, condition, orderStatement);
            paramList.Add(new SqlParameter(User.PARENTID, parent.UserId));
            return base.ExecuteList<User>(sql, paramList.ToArray());
        }

        public int CountMember()
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} u JOIN {1} ui ON  ui.{2}=u.{3} WHERE u.{4}=@{4} AND ui.{5}=0", User.TABLENAME, UserInfo.TABLENAME,
                UserInfo.USERID, User.USERID, User.ROLEID, UserInfo.STATE);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.ROLEID, (int)Role.Guest));
            return Convert.ToInt32(count);
        }
        public int CountChild(User user)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND {2}<>@{2}", User.TABLENAME, User.PARENTID, User.ROLEID);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter(User.ROLEID, (int)Role.Shadow));
            return Convert.ToInt32(count);
        }
        public int CountShadow(User user)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1} AND {2}=@{2}", User.TABLENAME, User.PARENTID, User.ROLEID);
            object count = base.ExecuteScalar(sql, new SqlParameter(User.PARENTID, user.UserId),
                new SqlParameter(User.ROLEID, (int)Role.Shadow));
            return Convert.ToInt32(count);
        }

        public int CountRateGroup(int groupId)
        {
            string sql = string.Format(@"SELECT COUNT(0) FROM {0} WHERE {1}=@{1}", UserInfo.TABLENAME, UserInfo.RATEGROUPID);
            object count = base.ExecuteScalar(sql, new SqlParameter(UserInfo.RATEGROUPID, groupId));
            return Convert.ToInt32(count);
        }
    }
}
