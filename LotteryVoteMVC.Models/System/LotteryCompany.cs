using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace LotteryVoteMVC.Models
{
    /// <summary>
    /// 开奖公司
    /// </summary>
    [Serializable]
    public class LotteryCompany
    {
        public const string TABLENAME = "tb_LotteryCompany";
        public const string COMPANYID = "CompanyId";
        public const string NAME = "NAME";
        public const string ABBREVIATION = "Abbreviation";
        public const string TYPEID = "TypeId";
        public const string REGION = "Region";
        public const string OPENTIME = "OpenTime";
        public const string CLOSETIME = "CloseTime";
        public const string CREATETIME = "CreateTime";

        private TimeSpan m_openTimeSpan;
        private TimeSpan m_closeTimeSpan;
        /// <summary>
        /// 公司Id.
        /// </summary>
        /// <value>
        /// The id.
        /// </value>
        public int CompanyId { get; set; }
        /// <summary>
        /// 公司名称.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        public string Name { get; set; }
        /// <summary>
        /// 公司缩写
        /// </summary>
        /// <value>
        /// The abbreviation.
        /// </value>
        [Required]
        public string Abbreviation { get; set; }
        /// <summary>
        /// 公司所在地区.
        /// </summary>
        /// <value>
        /// The region.
        /// </value>
        [Required]
        public Region Region { get; set; }
        [XmlIgnore]
        public int TypeId { get; set; }
        /// <summary>
        /// 公司类型.
        /// </summary>
        /// <value>
        /// The type of the company.
        /// </value>
        [Required]
        public CompanyType CompanyType
        {
            get
            {
                return (CompanyType)TypeId;
            }
            set
            {
                TypeId = (int)value;
            }
        }
        /// <summary>
        /// 开盘时间.
        /// </summary>
        /// <value>
        /// The open time.
        /// </value>
        [XmlIgnore]
        [Required]
        public TimeSpan OpenTime
        {
            get
            {
                return m_openTimeSpan;
            }
            set
            {
                m_openTimeSpan = value;
            }
        }
        /// <summary>
        /// 关盘时间.
        /// </summary>
        /// <value>
        /// The close time.
        /// </value>
        [XmlIgnore]
        [Required]
        public TimeSpan CloseTime
        {
            get
            {
                return m_closeTimeSpan;
            }
            set
            {
                m_closeTimeSpan = value;
            }
        }
        /// <summary>
        /// 创建时间.
        /// </summary>
        /// <value>
        /// The create time.
        /// </value>
        public DateTime CreateTime { get; set; }
        [XmlElement("OpenTime")]
        public long OpenTimeTicks
        {
            get
            {
                return m_openTimeSpan.Ticks;
            }
            set
            {
                m_openTimeSpan = new TimeSpan(value);
            }
        }
        [XmlElement("CloseTime")]
        public long CloseTimeTicks
        {
            get
            {
                return m_closeTimeSpan.Ticks;
            }
            set
            {
                m_closeTimeSpan = new TimeSpan(value);
            }
        }
    }
}
