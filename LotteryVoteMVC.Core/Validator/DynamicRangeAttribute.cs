using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Resources;

namespace LotteryVoteMVC.Core.Validator
{
    public class DynamicRangeAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _minPropertyName;
        private readonly string _maxPropertyName;
        private string m_errorMessage;
        public DynamicRangeAttribute(string minPropertyName, string maxPropertyName)
        {
            _minPropertyName = minPropertyName;
            _maxPropertyName = maxPropertyName;
        }
        public new string ErrorMessage
        {
            get
            {
                if (string.IsNullOrEmpty(m_errorMessage))
                    m_errorMessage = GetErrorMessage();
                return m_errorMessage;
            }
            set
            {
                m_errorMessage = value;
            }
        }
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var minProperty = validationContext.ObjectType.GetProperty(_minPropertyName);
            var maxProperty = validationContext.ObjectType.GetProperty(_maxPropertyName);
            if (minProperty == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", _minPropertyName));
            }
            if (maxProperty == null)
            {
                return new ValidationResult(string.Format("Unknown property {0}", _maxPropertyName));
            }
            var minValue = minProperty.GetValue(validationContext.ObjectInstance, null);
            var maxValue = maxProperty.GetValue(validationContext.ObjectInstance, null);
            var currentValue = value;
            if (LessThan(currentValue, minValue) || GreatThan(currentValue, maxValue))
            {
                return new ValidationResult(
                    string.Format(
                        GetErrorMessage(),
                        minValue,
                        maxValue
                    )
                );
            }

            return null;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "dynamicrange",
                ErrorMessage = this.ErrorMessage,
            };
            rule.ValidationParameters["minvalueproperty"] = _minPropertyName;
            rule.ValidationParameters["maxvalueproperty"] = _maxPropertyName;
            yield return rule;
        }

        private string GetErrorMessage()
        {
            if (ErrorMessageResourceType == null || string.IsNullOrEmpty(ErrorMessageResourceName))
                return ErrorMessage;
            ResourceManager resource = new ResourceManager(ErrorMessageResourceType);
            return resource.GetString(ErrorMessageResourceName);
        }
        private bool GreatThan(object v1, object v2)
        {
            var type = v1.GetType();
            if (type == typeof(decimal))
            {
                return (decimal)v1 > (decimal)v2;
            }
            else if (type == typeof(double))
            {
                return (double)v1 > (double)v2;
            }
            else
                return (int)v1 > (int)v2;
        }
        private bool GreatEquals(object v1, object v2)
        {
            var type = v1.GetType();
            if (type == typeof(decimal))
            {
                return (decimal)v1 >= (decimal)v2;
            }
            else if (type == typeof(double))
            {
                return (double)v1 >= (double)v2;
            }
            else
                return (int)v1 >= (int)v2;
        }
        private bool LessThan(object v1, object v2)
        {
            var type = v1.GetType();
            if (type == typeof(decimal))
            {
                return (decimal)v1 < (decimal)v2;
            }
            else if (type == typeof(double))
            {
                return (double)v1 < (double)v2;
            }
            else
                return (int)v1 < (int)v2;
        }
        private bool LessEquals(object v1, object v2)
        {
            var type = v1.GetType();
            if (type == typeof(decimal))
            {
                return (decimal)v1 <= (decimal)v2;
            }
            else if (type == typeof(double))
            {
                return (double)v1 <= (double)v2;
            }
            else
                return (int)v1 <= (int)v2;
        }
    }
}
