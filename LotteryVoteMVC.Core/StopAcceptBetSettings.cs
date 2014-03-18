using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LotteryVoteMVC.Models;
using System.Xml.Serialization;
using System.Web;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    public class StopAcceptBetSettings
    {
        private static object _lockHelper = new object();
        private static StopAcceptBetSettings _settings = new StopAcceptBetSettings();
        private const string M_SETTINGS = "StopAcceptBetSettings";

        private readonly string _configName = "StopAcceptBet.config";
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

        private DateTime? _lastUpdateTime;
        public DateTime LastUpdateTime
        {
            get
            {
                if (_lastUpdateTime == null || _lastUpdateTime.Value == default(DateTime))
                {
                    if (File.Exists(ConfigPath))
                    {
                        FileInfo config = new FileInfo(ConfigPath);
                        _lastUpdateTime = config.LastWriteTime;
                    }
                    else
                        _lastUpdateTime = default(DateTime);
                }
                return _lastUpdateTime.Value;
            }
            private set
            {
                _lastUpdateTime = value;
            }
        }

        private IList<Pair<int, int>> _settingDic;

        public static StopAcceptBetSettings Instance
        {
            get
            {
                if (_settings == null)
                {
                    lock (_lockHelper)
                    {
                        if (_settings == null)
                            _settings = new StopAcceptBetSettings();
                    }
                }
                return _settings;
            }
        }

        public IList<Pair<int, int>> GetStopAcceptBetSettings()
        {
            CheckUpdate();

            if (_settingDic == null)
            {
                if (!File.Exists(ConfigPath))
                    throw new FileNotFoundException(ConfigPath);
                _settingDic = Deserialize();
            }
            return _settingDic;
        }
        public void StopAcceptBet(int companyId, int gpwId)
        {
            var setting = GetStopAcceptBetSettings().Find(it => it.Key == companyId && it.Value == gpwId);
            if (setting == null)
            {
                setting = new Pair<int, int>(companyId, gpwId);
                _settingDic.Add(setting);
            }
            else
                _settingDic.Remove(setting);
            lock (_lockHelper)
            {
                Serialiaze(_settingDic);
                _settingDic = Deserialize();
            }
        }
        public void StartAcceptBet(int companyId, int gpwId)
        {
            var setting = GetStopAcceptBetSettings().Find(it => it.Key == companyId && it.Value == gpwId);
            if (setting != null)
            {
                _settingDic.Remove(setting);
            }

            lock (_lockHelper)
            {
                Serialiaze(_settingDic);
                _settingDic = Deserialize();
            }
        }

        private void CheckUpdate()
        {
            if (LastUpdateTime.Date != DateTime.Today)
            {
                _settingDic = Enumerable.Empty<Pair<int, int>>().ToList();
                Serialiaze(_settingDic);
            }
        }
        private void Serialiaze(IEnumerable<Pair<int, int>> settings)
        {
            XmlSerializer xs = new XmlSerializer(settings.GetType());
            using (Stream stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
            {
                xs.Serialize(stream, settings);
                stream.Close();
            }
        }
        private IList<Pair<int, int>> Deserialize()
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<Pair<int, int>>));
            using (Stream stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                List<Pair<int, int>> settings = (List<Pair<int, int>>)xs.Deserialize(stream);
                return settings;
            }
        }
    }
}
