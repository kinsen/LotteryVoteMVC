using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 代理可授权的操作
    /// </summary>
    public class AgentAuthorizeAction
    {
        public const string TableName = "tb_AgentAuthorizeAction";
        public const string AUTHORIZEID = "AuthorizeId";
        public const string NAME = "Name";
        public const string CONTROLLER = "Controller";
        public const string ACTION = "Action";
        public const string METHODSIGN = "MethodSign";
        public const string DEFAULTSTATE = "DefaultState";
        public const string CREATETIME = "CreateTime";

        public int AuthorizeId { get; set; }
        /// <summary>
        /// 操作名称
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        ///操作所在控制器
        /// </summary>
        /// <value>
        /// The controller.
        /// </value>
        public string Controller { get; set; }
        /// <summary>
        /// 操作所在Action
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        public string Action { get; set; }
        /// <summary>
        /// 操作的方法签名
        /// </summary>
        /// <value>
        /// The method sign.
        /// </value>
        public string MethodSign { get; set; }
        /// <summary>
        /// 默认状态，是否选中
        /// </summary>
        /// <value>
        ///   <c>true</c> if [default state]; otherwise, <c>false</c>.
        /// </value>
        public bool DefaultState { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
