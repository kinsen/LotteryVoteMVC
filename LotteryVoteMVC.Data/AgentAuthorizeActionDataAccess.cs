using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class AgentAuthorizeActionDataAccess : DataBase
    {
        public void Insert(AgentAuthorizeAction auth)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2},{3},{4},{5}) VALUES(@{1},@{2},@{3},@{4},@{5}) SELECT SCOPE_IDENTITY()",
                AgentAuthorizeAction.TableName, AgentAuthorizeAction.NAME, AgentAuthorizeAction.CONTROLLER, AgentAuthorizeAction.ACTION,
                AgentAuthorizeAction.METHODSIGN, AgentAuthorizeAction.DEFAULTSTATE);
            object id = base.ExecuteScalar(sql, new SqlParameter(AgentAuthorizeAction.NAME, auth.Name),
                new SqlParameter(AgentAuthorizeAction.CONTROLLER, auth.Controller),
                new SqlParameter(AgentAuthorizeAction.ACTION, auth.Action),
                new SqlParameter(AgentAuthorizeAction.METHODSIGN, auth.MethodSign),
                new SqlParameter(AgentAuthorizeAction.DEFAULTSTATE, auth.DefaultState));
            auth.AuthorizeId = Convert.ToInt32(id);
        }
        public void Update(AgentAuthorizeAction auth)
        {
            string sql = string.Format(@"UPDATE {0} SET {1}=@{1},{2}=@{2},{3}=@{3},{4}=@{4},{5}=@{5} WHERE {6}=@{6}",
                AgentAuthorizeAction.TableName, AgentAuthorizeAction.NAME, AgentAuthorizeAction.CONTROLLER, AgentAuthorizeAction.ACTION,
                AgentAuthorizeAction.METHODSIGN, AgentAuthorizeAction.DEFAULTSTATE, AgentAuthorizeAction.AUTHORIZEID);
            base.ExecuteNonQuery(sql, new SqlParameter(AgentAuthorizeAction.NAME, auth.Name),
                new SqlParameter(AgentAuthorizeAction.CONTROLLER, auth.Controller),
                new SqlParameter(AgentAuthorizeAction.ACTION, auth.Action),
                new SqlParameter(AgentAuthorizeAction.METHODSIGN, auth.MethodSign),
                new SqlParameter(AgentAuthorizeAction.DEFAULTSTATE, auth.DefaultState),
                new SqlParameter(AgentAuthorizeAction.AUTHORIZEID, auth.AuthorizeId));
        }
        public void Delete(int authId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", AgentAuthorizeAction.TableName, AgentAuthorizeAction.AUTHORIZEID);
            base.ExecuteNonQuery(sql, new SqlParameter(AgentAuthorizeAction.AUTHORIZEID, authId));
        }

        public AgentAuthorizeAction GetAgetnAuth(int authId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", AgentAuthorizeAction.TableName, AgentAuthorizeAction.AUTHORIZEID);
            return base.ExecuteModel<AgentAuthorizeAction>(sql, new SqlParameter(AgentAuthorizeAction.AUTHORIZEID, authId));
        }
        public AgentAuthorizeAction GetAgentAuth(string name)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", AgentAuthorizeAction.TableName, AgentAuthorizeAction.NAME);
            return base.ExecuteModel<AgentAuthorizeAction>(sql, new SqlParameter(AgentAuthorizeAction.NAME, name));
        }
        public IEnumerable<AgentAuthorizeAction> GetAuthByRole(Role role)
        {
            string sql = string.Format(@"select aaa.* from tb_AgentAuthorizeAction aaa
join tb_AgentAuthorizeInRole aar on aar.AuthorizeId=aaa.AuthorizeId
where aar.{0}=@{0}", AgentAuthorizeInRole.ROLEID);
            return base.ExecuteList<AgentAuthorizeAction>(sql, new SqlParameter(AgentAuthorizeInRole.ROLEID, (int)role));
        }
        /// <summary>
        /// 根据用户获取他授权的操作
        /// </summary>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        public IEnumerable<AgentAuthorizeAction> GetAuthByUser(User user)
        {
            string sql = string.Format(@"select aaa.* from tb_ShadowAuthorizeAction saa
join tb_AgentAuthorizeAction aaa on aaa.AuthorizeId=saa.AuthorizeId
where saa.{0}=@{0}", ShadowAuthorizeAction.USERID);
            return base.ExecuteList<AgentAuthorizeAction>(sql, new SqlParameter(ShadowAuthorizeAction.USERID, user.UserId));
        }
        public IEnumerable<AgentAuthorizeAction> GetAll()
        {
            string sql = string.Format(@"SELECT * FROM {0}", AgentAuthorizeAction.TableName);
            return base.ExecuteList<AgentAuthorizeAction>(sql);
        }
    }
}
