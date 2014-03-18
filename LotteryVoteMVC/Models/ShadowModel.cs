using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LotteryVoteMVC.Utility.Validator;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class ShadowModel
    {
        public ShadowModel() { }
        public ShadowModel(User user)
        {
            this.UserName = user.UserName;
            this.Name = user.UserInfo.Name;
            this.State = user.UserInfo.State;
            this.Email = user.UserInfo.Email;
        }

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
        [Email]
        public string Email { get; set; }
        public int[] AuthActions { get; set; }
        public User ToUser()
        {
            return new User
            {
                UserName = this.UserName,
                Role = Role.Shadow,
                UserInfo = new UserInfo
                {
                    Name = this.Name,
                    Password = this.Password,
                    State = this.State,
                    Email = this.Email
                }
            };
        }
    }
}