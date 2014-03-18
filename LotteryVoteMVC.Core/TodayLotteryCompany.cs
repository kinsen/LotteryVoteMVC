using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Timers;
using System.Xml.Serialization;
using LotteryVoteMVC.Models;
using System.Web;

namespace LotteryVoteMVC.Core
{
    public class TodayLotteryCompany
    {
        private static object _lockHelper = new object();
        private static TodayLotteryCompany _todayCompany;

        private readonly string _configName = "TodayLotteryCompany.config";
        private const string M_COMPANYSTR = "PersonalCompany";
        private DateTime _lastUpdateTime;
        public DateTime LastUpdateTime
        {
            get
            {
                if (_lastUpdateTime == default(DateTime))
                {
                    FileInfo config = new FileInfo(ConfigPath);
                    _lastUpdateTime = config.LastWriteTime;
                }
                return _lastUpdateTime;
            }
            private set
            {
                _lastUpdateTime = value;
            }
        }
        private string _configPath;
        public string ConfigPath
        {
            get
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    string folderPath = AppDomain.CurrentDomain.BaseDirectory + "\\" + "Config\\";
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);
                    _configPath = folderPath + _configName;
                }
                return _configPath;
            }
        }
        private CompanyManager _comManager;
        public CompanyManager ComManager
        {
            get
            {
                if (_comManager == null)
                    _comManager = new CompanyManager();
                return _comManager;
            }
        }

        Timer todayCompanyTime = new Timer(600000);        //十分钟检查更新

        private TodayLotteryCompany()
        {
            todayCompanyTime.AutoReset = true;
            todayCompanyTime.Enabled = true;
            todayCompanyTime.Elapsed += new ElapsedEventHandler(todayCompanyTime_Elapsed);
            todayCompanyTime.Start();
            CheckUpdate();
        }

        private void todayCompanyTime_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckUpdate();
        }
        private void CheckUpdate()
        {
            if (LastUpdateTime.Date != DateTime.Today)
                UpdateTodayLotteryCompany();
        }

        public static TodayLotteryCompany Instance
        {
            get
            {
                if (_todayCompany == null)
                {
                    lock (_lockHelper)
                    {
                        if (_todayCompany == null)
                            _todayCompany = new TodayLotteryCompany();
                    }
                }
                return _todayCompany;
            }
        }

        public IList<LotteryCompany> GetTodayCompany()
        {
            IList<LotteryCompany> companys = null;
            if (HttpContext.Current != null)
                companys = HttpContext.Current.Items[M_COMPANYSTR] as IList<LotteryCompany>;

            if (companys == null)
            {
                if (!File.Exists(ConfigPath))
                    throw new FileNotFoundException(ConfigPath);
                companys = Deserialize();
                if (HttpContext.Current != null)
                    HttpContext.Current.Items[M_COMPANYSTR] = companys;
            }
            return companys;
        }
        /// <summary>
        /// 获取还在接受投注的公司.
        /// </summary>
        /// <returns></returns>
        public IList<LotteryCompany> GetOpenCompany()
        {
            List<LotteryCompany> returnValue = new List<LotteryCompany>();
            var companyList = GetTodayCompany();
            foreach (var company in companyList)
            {
                var openTime = DateTime.Today.Add(company.OpenTime);
                var closeTime = DateTime.Today.Add(company.CloseTime);
                var now = DateTime.Now;
                if (now >= openTime && now <= closeTime)
                    returnValue.Add(company);
            }
            return returnValue;
        }

        /// <summary>
        /// 更新今日开奖公司.
        /// </summary>
        public void UpdateTodayLotteryCompany()
        {
            var companys = ComManager.GetTodayLotteryCompany().ToList(); ;
            Serialiaze(companys);
        }

        private void Serialiaze(IList<LotteryCompany> source)
        {
            XmlSerializer xs = new XmlSerializer(source.GetType());
            Stream stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xs.Serialize(stream, source);
            stream.Close();
        }
        private IList<LotteryCompany> Deserialize()
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<LotteryCompany>));
            Stream stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            List<LotteryCompany> companys = (List<LotteryCompany>)xs.Deserialize(stream);
            return companys;
        }
    }
}
