using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Core.Application;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Utility;
using LotteryVoteMVC.Core.Exceptions;

namespace LotteryVoteMVC.Core
{
    public class GPWManager : ManagerBase
    {
        public static string ToString(int gpwId)
        {
            var gpw = LotterySystem.Current.GamePlayWays.Find(it => it.Id == gpwId);
            if (gpw == null)
                throw new InvalidDataException("gpwId", "找不到游戏玩法,GPWID:" + gpwId);
            var gameDesc = EnumHelper.GetEnumDescript(gpw.GameType);
            return string.Format("{0}{1}", gameDesc.Description, Resource.ResourceManager.GetString(gpw.PlayWay.ToString()));
        }
    }
}
