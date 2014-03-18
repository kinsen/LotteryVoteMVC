using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using LotteryVoteMVC.Resources.Models;

namespace LotteryVoteMVC.Models
{
    public class ShadowAuthModel
    {
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Name { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Controller { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string Action { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public string MethodSign { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public Role[] AuthRole { get; set; }
        [Required(ErrorMessageResourceType = typeof(ModelResource), ErrorMessageResourceName = "Required")]
        public bool DefaultState { get; set; }

        public AgentAuthorizeAction ToAuthAction()
        {
            return new AgentAuthorizeAction
            {
                Name = this.Name,
                Controller = this.Controller,
                Action = this.Action,
                MethodSign = this.MethodSign,
                DefaultState = this.DefaultState
            };
        }
    }
}