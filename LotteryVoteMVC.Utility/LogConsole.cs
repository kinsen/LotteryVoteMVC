using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.IO;

namespace LotteryVoteMVC.Utility
{
    public class LogConsole
    {
        private static ILog _log;
        public static ILog Log
        {
            get { return _log; }
            set { _log = value; }
        }

        static LogConsole()
        {
            log4net.Config.XmlConfigurator.Configure(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            _log = LogManager.GetLogger(typeof(LogConsole));
        }

        public static void Info(object message)
        {
            Log.Info(message);
        }
        public static void Info(object message, Exception ex)
        {
            Log.Info(message, ex);
        }
        public static void Debug(object message)
        {
            Log.Debug(message);
        }
        public static void Debug(object message, Exception ex)
        {
            Log.Debug(message, ex);
        }
        public static void Error(object message)
        {
            Log.Error(message);
        }
        public static void Error(object message, Exception ex)
        {
            Log.Error(message, ex);
        }
        public static void Warn(object message)
        {
            Log.Warn(message);
        }
        public static void Warn(object message, Exception ex)
        {
            Log.Warn(message, ex);
        }
    }
}
