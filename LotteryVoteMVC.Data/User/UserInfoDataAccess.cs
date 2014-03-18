using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class UserInfoDataAccess : DataBase
    {
        public void Insert(UserInfo userInfo)
        {
            string sql = string.Format(@"Insert into tb_UserInfo ({0},{1},{2},{3},{4},{5},{6},{7}) values(@{0},@{1},@{2},@{3},@{4},@{5},@{6},@{7})",
                UserInfo.USERID, UserInfo.PASSWORD, UserInfo.NAME, UserInfo.STATE, UserInfo.GIVENCREDIT, UserInfo.AVAILABLEGIVENCREDIT, UserInfo.SHARERATE, UserInfo.EMAIL);
            base.ExecuteNonQuery(sql,
            new SqlParameter(UserInfo.USERID, userInfo.UserId),
            new SqlParameter(UserInfo.PASSWORD, userInfo.Password),
            new SqlParameter(UserInfo.NAME, userInfo.Name),
            new SqlParameter(UserInfo.STATE, (int)userInfo.State),
            new SqlParameter(UserInfo.GIVENCREDIT, userInfo.GivenCredit),
            new SqlParameter(UserInfo.AVAILABLEGIVENCREDIT, userInfo.AvailableGivenCredit),
            new SqlParameter(UserInfo.SHARERATE, userInfo.ShareRate),
            new SqlParameter(UserInfo.EMAIL, userInfo.Email));
        }
        public void Update(UserInfo userInfo)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1},{2}=@{2},{3}=@{3},{4}=@{4},{5}=@{5},{6}=@{6},{7}=@{7},{8}=@{8} WHERE {9}=@{9}",
                UserInfo.TABLENAME, UserInfo.PASSWORD, UserInfo.NAME, UserInfo.STATE,
                UserInfo.GIVENCREDIT, UserInfo.AVAILABLEGIVENCREDIT, UserInfo.SHARERATE, UserInfo.EMAIL, UserInfo.LASTCHANGEPWD, UserInfo.USERID);
            base.ExecuteNonQuery(sql,
                new SqlParameter(UserInfo.USERID, userInfo.UserId),
                new SqlParameter(UserInfo.PASSWORD, userInfo.Password),
                new SqlParameter(UserInfo.NAME, userInfo.Name),
                new SqlParameter(UserInfo.STATE, (int)userInfo.State),
                new SqlParameter(UserInfo.GIVENCREDIT, userInfo.GivenCredit),
                new SqlParameter(UserInfo.AVAILABLEGIVENCREDIT, userInfo.AvailableGivenCredit),
                new SqlParameter(UserInfo.SHARERATE, userInfo.ShareRate),
                new SqlParameter(UserInfo.EMAIL, userInfo.Email),
                new SqlParameter(UserInfo.LASTCHANGEPWD, userInfo.LastChangePwd));
        }
        public void UpdateState(User user, UserState state)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1} WHERE {2}=@{2}", UserInfo.TABLENAME, UserInfo.STATE, UserInfo.USERID);
            base.ExecuteNonQuery(sql, new SqlParameter(UserInfo.STATE, (int)state),
                new SqlParameter(UserInfo.USERID, user.UserId));
        }
        /// <summary>
        /// 修改家族状态（以传入用户为根节点）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="state">The state.</param>
        public void UpdateFamilyState(User user, UserState state)
        {
            string sql = string.Format(@";WITH CTE AS(
SELECT * FROM tb_User  WHERE {0}=@{0}
UNION ALL
SELECT B.* FROM tb_User AS B,CTE AS C 
WHERE B.parentId=C.UserId and B.UserId>C.UserId)
update tb_UserInfo set {1}=@{1} where UserId in (select UserId from CTE)", User.USERID, UserInfo.STATE);
            base.ExecuteNonQuery(sql, new SqlParameter(User.USERID, user.UserId), new SqlParameter(UserInfo.STATE, (int)state));
        }
        /// <summary>
        /// 更新家族树的分成（以传入User为根）
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="shareRate">The share rate.</param>
        public void UpdateFamilyShareRate(User user, double shareRate)
        {
            string sql = string.Format(@";WITH CTE AS
(
SELECT * FROM tb_User  WHERE {0}=@{0}
UNION ALL
SELECT B.* FROM tb_User AS B,CTE AS C WHERE B.parentId=C.UserId and B.UserId>C.UserId
)
update tb_UserInfo set {1}=@{1} where UserId in ( SELECT UserId FROM CTE)", User.USERID, UserInfo.SHARERATE);
            base.ExecuteNonQuery(sql, new SqlParameter(User.USERID, user.UserId),
                new SqlParameter(UserInfo.SHARERATE, shareRate));
        }

        public UserInfo GetUserInfo(int userId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", UserInfo.TABLENAME, UserInfo.USERID);
            return base.ExecuteModel<UserInfo>(sql, new SqlParameter(UserInfo.USERID, userId));
        }
    }
}
