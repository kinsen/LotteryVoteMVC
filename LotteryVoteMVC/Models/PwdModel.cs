using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LotteryVoteMVC.Resources;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class PwdModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"), Remote("CheckPassword", "Validate", ErrorMessage = "密码错误")]
        public string YourPassword { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"), MinLength(6)]
        public string Password { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"), Compare("Password")]
        public string ConfirmPassword { get; set; }
    }
}