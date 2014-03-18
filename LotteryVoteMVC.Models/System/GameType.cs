using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Utility;

namespace LotteryVoteMVC.Models
{
    public enum GameType
    {
        /// <summary>
        /// 2D
        /// </summary>
        [EnumDescription("2D")]
        TwoDigital = 1,
        /// <summary>
        /// 3D
        /// </summary>
        [EnumDescription("3D")]
        ThreeDigital = 2,
        /// <summary>
        /// 4D
        /// </summary>
        [EnumDescription("4D")]
        FourDigital = 3,
        [EnumDescription("5D")]
        FiveDigital = 4,
        [EnumDescription("PL2")]
        PL2 = 5,
        [EnumDescription("PL3")]
        PL3 = 6,
        [EnumDescription("A&B PL2")]
        A_B_PL2 = 7,
        [EnumDescription("B&C PL2")]
        B_C_PL2 = 8,
        [EnumDescription("C&A PL2")]
        C_A_PL2 = 9

    }
}
