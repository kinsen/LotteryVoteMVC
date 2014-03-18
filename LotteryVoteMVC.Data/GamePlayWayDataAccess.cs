using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Data
{
    public class GamePlayWayDataAccess : DataBase
    {
        public IEnumerable<GamePlayWay> GetAll()
        {
            string sql = string.Format(@"SELECT * FROM {0}", GamePlayWay.TABLENAME);
            return base.ExecuteList<GamePlayWay>(sql);
        }
    }
}
