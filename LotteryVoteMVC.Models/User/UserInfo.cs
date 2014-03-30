using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LotteryVoteMVC.Resources.Models;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace LotteryVoteMVC.Models
{
    public class UserInfo
    {
        public const string TABLENAME = "tb_UserInfo";
        public const string USERID = "UserId";
        public const string PASSWORD = "Password";
        public const string NAME = "Name";
        public const string STATE = "State";
        public const string EMAIL = "Email";
        public const string GIVENCREDIT = "GivenCredit";
        public const string AVAILABLEGIVENCREDIT = "AvailableGivenCredit";
        public const string RATEGROUPID = "RateGroupId";
        public const string CREATETIME = "CreateTime";
        public const string LASTCHANGEPWD = "LastChangePwd";

        public DataRow DataRow { get; set; }
        public int UserId { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"),
        StringLength(20, ErrorMessageResourceType = (typeof(ModelResource)), ErrorMessageResourceName = "StringLengthRange", MinimumLength = 6)]
        public string Password { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"),
        StringLength(30, MinimumLength = 1, ErrorMessageResourceType = (typeof(ModelResource)), ErrorMessageResourceName = "StringLengthRange")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public UserState State { get; set; }
        /// <summary>
        /// 信用额
        /// </summary>
        /// <value>
        /// The given credit.
        /// </value>
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^\d+(\.\d+)?$")]
        public decimal GivenCredit { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public decimal AvailableGivenCredit { get; set; }
        /// <summary>
        /// 分成.
        /// </summary>
        /// <value>
        /// The share rate.
        /// </value>
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^\d+$", ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "MustBeNum")]
        public int RateGroupId { get; set; }

        private ShareRateGroup _rateGroup;
        public ShareRateGroup RateGroup
        {
            get
            {
                if (_rateGroup == null && this.DataRow != null)
                    _rateGroup = ModelParser<ShareRateGroup>.ParseModel(DataRow);
                return _rateGroup;
            }
            set
            {
                _rateGroup = value;
            }
        }

        [RegularExpression(@"^\w+((-\w+)|(\.\w+))*\@[A-Za-z0-9]+((\.|-)[A-Za-z0-9]+)*\.[A-Za-z0-9]+$", ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "EmailError")]
        public string Email { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastChangePwd { get; set; }
    }
}
