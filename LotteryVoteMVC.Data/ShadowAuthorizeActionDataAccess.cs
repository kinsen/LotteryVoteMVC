using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using LotteryVoteMVC.Models;
using System.Data.SqlClient;

namespace LotteryVoteMVC.Data
{
    public class ShadowAuthorizeActionDataAccess : DataBase
    {
        public void Insert(IEnumerable<ShadowAuthorizeAction> shadowAuths)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(ShadowAuthorizeAction.USERID, typeof(int));
            dt.Columns.Add(ShadowAuthorizeAction.AUTHORIZEID, typeof(int));
            foreach (var auth in shadowAuths)
            {
                var row = dt.NewRow();
                row[ShadowAuthorizeAction.USERID] = auth.UserId;
                row[ShadowAuthorizeAction.AUTHORIZEID] = auth.AuthorizeId;
                dt.Rows.Add(row);
            }
            base.ExecuteSqlTranWithSqlBulkCopy(ShadowAuthorizeAction.TableName, dt);
        }
        public void Insert(ShadowAuthorizeAction shadowAuth)
        {
            string sql = string.Format(@"INSERT INTO {0} ({1},{2}) VALUES (@{1},@{2}) SELECT SCOPE_IDENTITY()",
                ShadowAuthorizeAction.TableName, ShadowAuthorizeAction.USERID, ShadowAuthorizeAction.AUTHORIZEID);
            object id = base.ExecuteScalar(sql, new SqlParameter(ShadowAuthorizeAction.USERID, shadowAuth.UserId),
                new SqlParameter(ShadowAuthorizeAction.AUTHORIZEID, shadowAuth.AuthorizeId));
            shadowAuth.LinkId = Convert.ToInt32(id);
        }
        public void Delete(User user)
        {
            string sql = string.Format(@"DELETE {0} WHERE {1}=@{1}", ShadowAuthorizeAction.TableName, ShadowAuthorizeAction.USERID);
            base.ExecuteNonQuery(sql, new SqlParameter(ShadowAuthorizeAction.USERID, user.UserId));
        }
    }
}
