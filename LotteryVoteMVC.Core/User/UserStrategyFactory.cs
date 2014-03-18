using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;

namespace LotteryVoteMVC.Core
{
    internal class UserStrategyFactory
    {
        private static UserStrategyFactory _factory;
        private UserStrategyFactory() { }

        internal static UserStrategyFactory GetFactory()
        {
            if (_factory == null)
                _factory = new UserStrategyFactory();
            return _factory;
        }

        internal UserStrategy GetUserStrategy(Role role, UserManager userManager)
        {
            UserStrategy strategy = null;
            switch (role)
            {
                case Role.Super:
                case Role.Master:
                case Role.Agent:
                    strategy = new ProxyStrategy(); break;
                case Role.Guest:
                    strategy = new MemberStrategy(); break;
                case Role.Shadow:
                    strategy = new ShadowStrategy(); break;
                default: throw new ApplicationException("不能创建其他角色的用户!");
            }
            strategy.UserManager = userManager;
            return strategy;
        }
    }
}
