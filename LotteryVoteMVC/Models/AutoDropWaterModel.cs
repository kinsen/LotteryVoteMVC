using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class AutoDropWaterModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public int GamePlayWay { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public double DropWater { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public decimal Amount { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public CompanyType CompanyType { get; set; }
    }
}