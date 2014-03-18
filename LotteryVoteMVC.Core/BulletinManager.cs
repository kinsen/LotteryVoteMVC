using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using System.Xml.Serialization;
using System.IO;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Core
{
    public class BulletinManager : ManagerBase
    {
        private static object _lockHelper = new object();
        private static BulletinManager _manager;
        private BulletinManager() { }
        public static BulletinManager GetManager()
        {
            if (_manager == null)
            {
                lock (_lockHelper)
                {
                    if (_manager == null)
                        _manager = new BulletinManager();
                }
            }
            return _manager;
        }

        private readonly string _storeFileName = "bulletin.config";
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
                    _configPath = folderPath + _storeFileName;
                }
                return _configPath;
            }
        }
        private IEnumerable<Bulletin> _bulletins;
        public IEnumerable<Bulletin> Bulletins
        {
            get
            {
                if (_bulletins == null)
                    _bulletins = Deserialize();
                return _bulletins;
            }
        }

        public string GetTopBulletin()
        {
            var bulletin = Bulletins.OrderByDescending(it => it.CreateTime).FirstOrDefault();
            return bulletin == null ? "6D9VN.com Welcome!" : bulletin.Content;
        }
        public void Add(string content)
        {
            Bulletin bulletin = new Bulletin
            {
                BulletinId = Guid.NewGuid().ToString(),
                Content = content,
                CreateTime = DateTime.Now
            };
            var list = new List<Bulletin>(Bulletins);
            list.Add(bulletin);
            Serialiaze(list);
            _bulletins = null;  //清空旧数据
        }
        public void Remove(string bulletinId)
        {
            var bulletin = Bulletins.Find(it => it.BulletinId == bulletinId);
            if (bulletin != null)
            {
                var bulletinList = Bulletins as List<Bulletin>;
                bulletinList.Remove(bulletin);
                Serialiaze(bulletinList);
                this._bulletins = null;
            }
        }

        public PagedList<Bulletin> GetBulletins(int pageIndex)
        {
            int skip = (pageIndex - 1) * pageSize;
            int count = Bulletins.Count();
            return new PagedList<Bulletin>(Bulletins.OrderByDescending(it => it.CreateTime).Skip(skip).Take(pageSize), pageIndex, pageSize, count);
        }
        public PagedList<Bulletin> GetBulletins(string content)
        {
            var bulletins = Bulletins.Where(it => it.Content.Contains(content));
            return new PagedList<Bulletin>(bulletins, 0, pageSize, bulletins.Count());
        }

        private void Serialiaze(IEnumerable<Bulletin> bulletins)
        {
            XmlSerializer xs = new XmlSerializer(bulletins.GetType());
            Stream stream = new FileStream(ConfigPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            xs.Serialize(stream, bulletins);
            stream.Close();
        }
        private IEnumerable<Bulletin> Deserialize()
        {
            try
            {
                XmlSerializer xs = new XmlSerializer(typeof(List<Bulletin>));
                Stream stream = new FileStream(ConfigPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                IEnumerable<Bulletin> bulletins = (List<Bulletin>)xs.Deserialize(stream) ?? Enumerable.Empty<Bulletin>();
                return bulletins;
            }
            catch (FileNotFoundException ex)
            {
                LogConsole.Error("反序列化公告", ex);
                return Enumerable.Empty<Bulletin>();
            }
        }
    }
}
