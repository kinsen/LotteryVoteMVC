using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 公司类型所支持的号码长度
    /// </summary>
    public class CompanyTypeSupportNumLen
    {
        public const string TABLENAME = "tb_CompanyTypeSupportNumLen";
        public const string COMPANYTYPEID = "CompanyTypeId";
        public const string LENID = "LenId";
        public const string COUNT = "Count";

        private NumLength _length;
        public DataRow DataRow { get; set; }
        public int CompanyTypeId { get; set; }
        /// <summary>
        /// 公司.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public CompanyType CompanyType
        {
            get
            {
                return (CompanyType)CompanyTypeId;
            }
            set
            {
                CompanyTypeId = (int)value;
            }
        }
        /// <summary>
        /// 号码长度
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public NumLength NumLen
        {
            get
            {
                if (_length == null && DataRow != null)
                    _length = ModelParser<NumLength>.ParseModel(DataRow);
                return _length;
            }
            set
            {
                _length = value;
            }
        }
        /// <summary>
        /// 号码数量.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        public int Count { get; set; }
    }
}
