using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Models;
using LotteryVoteMVC.Core.Bet;
using LotteryVoteMVC.Core.Limit;

namespace LotteryVoteMVC.Core.Bet
{
    public static class OrderBuilder
    {
        internal static BuildResult Build(User user, LotterySpecies specie,
            IEnumerable<BetItem> betList, bool isCon = false, LimitChecker checker = null)
        {
            var builder = new CommOrderBuilder();
            if (checker != null) builder.Checker = checker;
            return new BuildResult
            {
                Sheet = builder.BuildOrder(user, specie, betList,
                    new Dictionary<string, object> { { "ISCON", isCon } }),
                Result = builder.BetResult
            };
        }

        internal static BuildResult Build(User user, LotterySpecies specie,
            FastBetItem fastBetItem, GameType gameType, LimitChecker checker = null)
        {
            var builder = new FastBetOrderBuilder();
            if (checker != null) builder.Checker = checker;
            return new BuildResult
            {
                Sheet = builder.BuildOrder(user, specie, new[] { fastBetItem },
                    new Dictionary<string, object> { { "GameType", gameType } }),
                Result = builder.BetResult
            };
        }

        internal static BuildResult Build(User user, LotterySpecies specie,
            IEnumerable<AutoBetItem> betList, LimitChecker checker = null)
        {
            var builder = new AutoBetOrderBuilder();
            if (checker != null) builder.Checker = checker;
            return new BuildResult
            {
                Sheet = builder.BuildOrder(user, specie, betList, null),
                Result = builder.BetResult
            };
        }

        internal static BuildResult BuildUnionPL2(User user, LotterySpecies specie,
            IEnumerable<BetItem> betList, LimitChecker checker = null)
        {
            var builder = new UnionPL2OrderBuilder();
            if (checker != null) builder.Checker = checker;
            return new BuildResult
            {
                Sheet = builder.BuildOrder(user, specie, betList, null),
                Result = builder.BetResult
            };
        }
    }

    internal class BuildResult
    {
        public IDictionary<BetSheet, IList<BetOrder>> Sheet { get; set; }
        public BetResult Result { get; set; }
    }
}
