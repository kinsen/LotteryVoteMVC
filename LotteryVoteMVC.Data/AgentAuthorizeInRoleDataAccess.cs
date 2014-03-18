using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class AgentAuthorizeInRoleDataAccess : DataBase
    {
        public void Insert(AgentAuthorizeAction authAction, Role role)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2})", AgentAuthorizeInRole.TableName, AgentAuthorizeInRole.AUTHORIZEID, AgentAuthorizeInRole.ROLEID);
            base.ExecuteNonQuery(sql, new SqlParameter(AgentAuthorizeInRole.AUTHORIZEID, authAction.AuthorizeId),
                new SqlParameter(AgentAuthorizeInRole.ROLEID, (int)role));
        }
        public void Delete(int authId)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", AgentAuthorizeInRole.TableName, AgentAuthorizeInRole.AUTHORIZEID);
            base.ExecuteNonQuery(sql, new SqlParameter(AgentAuthorizeInRole.AUTHORIZEID, authId));
        }

        public IEnumerable<AgentAuthorizeInRole> GetAuthRoles(int authId)
        {
            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}=@{1}", AgentAuthorizeInRole.TableName, AgentAuthorizeInRole.AUTHORIZEID);
            return base.ExecuteList<AgentAuthorizeInRole>(sql, new SqlParameter(AgentAuthorizeInRole.AUTHORIZEID, authId));
        }
    }
}
