using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core;
using System.Timers;
using System.Threading;
using System.Configuration;

namespace LotteryVoteMVC.Service
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer _timer;
        public Service1()
        {
            InitializeComponent();
        }



        protected override void OnStart(string[] args)
        {
            LogConsole.Info("Service Start");
            Thread thr = new Thread(Backup);
            thr.Start();
        }

        protected override void OnStop()
        {
            LogConsole.Info("Service End");
        }

        private void Backup()
        {
            DateTime now = DateTime.Now;
            string time = ConfigurationManager.AppSettings["ExecuteTime"] ?? now.AddMinutes(1).ToString("HH,mm,ss");
            var times = time.Split(',').Select(it => int.Parse(it)).ToList();

            DateTime executeTime = new DateTime(now.Year, now.Month, now.Day, times[0], times[1], times[2]);
            if (executeTime < DateTime.Now)
                executeTime = executeTime.AddDays(1);
            LogConsole.Info("下次执行备份时间:" + executeTime.ToString("yyyy-MM-dd HH:mm:ss"));


            var hangTime = executeTime - DateTime.Now;
            Thread.Sleep((int)hangTime.TotalMilliseconds);

            _timer = new System.Timers.Timer(86400000.0);
            _timer.Elapsed += new ElapsedEventHandler(_timer_Elapsed);
            _timer.Enabled = true;
            BackupData();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BackupData();
        }

        void BackupData()
        {
            LogConsole.Info("Pre back up.");
            BackupManager bkupManager = new BackupManager();
            bkupManager.BackUp();
            LogConsole.Info("End.");
            LogConsole.Info("下次执行索引时间:" + DateTime.Now.AddDays(1).ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
