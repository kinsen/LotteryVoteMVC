using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LotteryVoteMVC.Core.Validator;
using LotteryVoteMVC.Resources.Models;
using System.Xml.Serialization;

namespace LotteryVoteMVC.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        [Remote("CheckUserName", "Validate", ErrorMessage = "已存在该用户")]
        public string UserName { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string YourPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Password { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"), Compare("Password")]
        public string ConfirmPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public UserState State { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"),
        DynamicRange("MinGivenCredit", "MaxGivenCredit", ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "DynamicRange")]
        public decimal GivenCredit { get; set; }
        public decimal MinGivenCredit { get; set; }
        public decimal MaxGivenCredit { get; set; }
        public string Email { get; set; }
        public LotterySpecies[] Species { get; set; }
        public int CommGroup { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public int RateGroupId { get; set; }
        public double MinShareRate { get; set; }
        public double MaxShareRate { get; set; }

        public User ToUser()
        {
            User user = new User
            {
                UserName = this.UserName,
                UserInfo = new UserInfo
                {
                    Password = this.Password,
                    Name = this.Name,
                    State = this.State,
                    GivenCredit = this.GivenCredit,
                    AvailableGivenCredit = this.GivenCredit,
                    RateGroupId = this.RateGroupId
                }
            };
            return user;
        }
    }
}