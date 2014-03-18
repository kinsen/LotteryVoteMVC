using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace LotteryVoteMVC.Data
{
    internal class DBManager
    {
        internal static readonly string ConnectionString = ConfigurationManager.ConnectionStrings["LotteryVoteDataBase"].ConnectionString;
        internal static readonly string DBProviderName =
            string.IsNullOrEmpty(ConfigurationManager.AppSettings["dbProviderName"]) ?
            "System.Data.SqlClient" : ConfigurationManager.AppSettings["dbProviderName"];
    }
}
