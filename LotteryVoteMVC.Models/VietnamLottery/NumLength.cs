using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 公司类型所支持的号码长度
    /// </summary>
    public class NumLength
    {
        public const string TABLENAME = "tb_NumLength";
        public const string LENID = "LenId";
        public const string LENNAME = "LenName";
        public const string LENGTH = "Length";
        public const string DESCRIPTION = "Description";

        public int LenId { get; set; }
        /// <summary>
        /// 长度名称
        /// </summary>
        /// <value>
        /// The name of the len.
        /// </value>
        public string LenName { get; set; }
        /// <summary>
        /// 号码长度.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public int Length { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
    }
}
