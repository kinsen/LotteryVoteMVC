using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Data;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    /// <summary>
    /// 分成组管理器
    /// </summary>
    public class ShareRateGroupManager : ManagerBase
    {
        private UserDataAccess _daUser;
        public UserDataAccess DaUser
        {
            get
            {
                if (_daUser == null)
                    _daUser = new UserDataAccess();
                return _daUser;
            }
        }

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

        public void Remove(int groupId)
        {
            int count = 0;
            if ((count = DaUser.CountRateGroup(groupId)) > 0)
                throw new BusinessException(string.Format("该分成组被分配{0}个用户上，请先更改这些用户的分成组再尝试删除", count));
            DaShareRateGroup.Remove(groupId);
        }
    }
}
