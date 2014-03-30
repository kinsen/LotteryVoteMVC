using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class ShareGroupModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Name { get; set; }

        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public double ShareRate { get; set; }

    }
}