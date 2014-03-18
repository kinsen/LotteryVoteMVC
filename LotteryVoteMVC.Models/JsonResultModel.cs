using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LotteryVoteMVC.Models
{
    public class JsonResultModel
    {
        /// <summary>
        /// 是否调用成功
        /// </summary>
        /// <value>
        ///   <c>true</c> if [invoke success]; otherwise, <c>false</c>.
        /// </value>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// 返回的消息
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }
        /// <summary>
        /// 要传送的Model实体
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public object Model { get; set; }
    }
}