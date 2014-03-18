using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using Microsoft;
using System.Data;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    public class BackupManager : ManagerBase
    {
        private string _databaseName;
        private string _backupPath;
        public string DataBaseName { get { return _databaseName; } set { _databaseName = value; } }
        public string BackupPath { get { return _backupPath; } set { _backupPath = value; } }

        private int _keepDataMonth;
        /// <summary>
        /// 数据保存时间,默认3个月
        /// </summary>
        protected int KeepDataMonth
        {
            get
            {
                if (_keepDataMonth == default(int))
                {
                    _keepDataMonth = int.Parse(ConfigurationManager.AppSettings["KeepDataMonth"] ?? "3");
                }
                return _keepDataMonth;
            }
        }

        public BackupManager()
        {
            _databaseName = ConfigurationManager.AppSettings["DataBaseName"] ?? "LotteryVote";
            _backupPath = ConfigurationManager.AppSettings["BackupPath"] ?? @"D:\DataBase\Backup";
        }

        public void BackUp()
        {
            BackupOrders();
            BackupDataBase();
        }

        private void BackupDataBase()
        {
            if (DateTime.Today.DayOfWeek != DayOfWeek.Sunday) return;

            try
            {
                LogConsole.Info("Begin back up data base.");
                if (!Directory.Exists(BackupPath))
                    Directory.CreateDirectory(BackupPath);
                string bakName = string.Format("LotteryVote_{0}.bak", DateTime.Today.ToString("yyyy_MM_dd"));
                string bakPath = string.Concat(BackupPath, "\\", bakName);

                string sqlbak = string.Format("BACKUP DATABASE {1} TO DISK = '{0}' WITH INIT", bakPath, DataBaseName);

                var result = SqlHelper.ExecuteNonQuery(ConfigurationManager.ConnectionStrings["LotteryVoteDataBase"].ConnectionString, CommandType.Text, sqlbak) != 0;
                LogConsole.Info(result ? "Backup database success!" : "Backup database faile!");
            }
            catch (Exception ex)
            {
                LogConsole.Error("Backup DataBase Error", ex);
            }

        }

        private void BackupOrders()
        {
            //if (DateTime.Today.Day != 1) return;
            DateTime targetDate = DateTime.Today.AddMonths(KeepDataMonth * -1);

            try
            {
                LogConsole.Info(string.Format("begin back up order.{0}", targetDate.ToShortDateString()));
                OrderManager orderManager = new OrderManager();
                orderManager.Backup(targetDate);
                LogConsole.Info("back up order success.");
            }
            catch (Exception ex)
            {
                LogConsole.Error("Back up order error", ex);
            }
        }
    }
}
