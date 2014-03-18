using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Models
{
    public class CompanyTypeModel
    {
        public const string TABLENAME = "tb_CompanyType";
        public const string ID = "Id";
        public const string TYPENAME = "TypeName";
        public const string CREATETIME = "CreateTime";


        public int Id { get; set; }
        /// <summary>
        /// 类型名称.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string TypeName { get; set; }
        public DateTime CreateTime { get; set; }
        public override string ToString()
        {
            return TypeName;
        }
    }
}
