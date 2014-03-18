using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class LoginModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string UserName { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required"), DataType(DataType.Password)]
        public string Password { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string VerifyCode { get; set; }
    }
}