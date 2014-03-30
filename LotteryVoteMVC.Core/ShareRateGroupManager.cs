using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 分成组管理器
    /// </summary>
    public class ShareRateGroupManager : ManagerBase
    {
        private ShareRateGroupDataAccess _daShareRateGroup;

        public ShareRateGroupDataAccess DaShareRateGroup
        {
            get
            {
                if (_daShareRateGroup == null)
                    _daShareRateGroup = new ShareRateGroupDataAccess();
                return _daShareRateGroup;
            }
        }

        public void AddGroup(string name, double shareRate)
        {
            var group = new ShareRateGroup
            {
                Name = name,
                ShareRate = shareRate
            };
            DaShareRateGroup.AddGroup(group);
        }

        public ShareRateGroup GetGroup(int groupId)
        {
            return DaShareRateGroup.GetGroup(groupId);
        }

        public IEnumerable<ShareRateGroup> ListChildGroup(double rate)
        {
            return DaShareRateGroup.ListChildGroup(rate);
        }

        public IEnumerable<ShareRateGroup> ListGroup()
        {
            return DaShareRateGroup.ListGroup();
        }
    }
}
