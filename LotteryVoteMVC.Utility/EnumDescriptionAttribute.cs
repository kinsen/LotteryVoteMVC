using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Utility
{
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; set; }
        public int Value { get; private set; }
        public string Name { get; private set; }

        public EnumDescriptionAttribute(string description)
        {
            this.Description = description;
        }

        private static IDictionary<string, EnumDescriptionAttribute> cachedEnum = new Dictionary<string, EnumDescriptionAttribute>();

        public static EnumDescriptionAttribute GetEnumDesc(object enumValue)
        {
            Type enumType = enumValue.GetType();
            string key = enumType.FullName + "_" + enumValue.ToString();
            EnumDescriptionAttribute returnVal = null;
            if (cachedEnum.ContainsKey(key))
            {
                returnVal = cachedEnum[key];
            }
            else
            {
                var field = enumType.GetFields().Find(it => it.Name == enumValue.ToString());
                if (field != null)
                {
                    var eds = field.GetCustomAttributes(typeof(EnumDescriptionAttribute), false);
                    if (eds.Length == 1)
                    {
                        returnVal = (EnumDescriptionAttribute)eds[0];
                        returnVal.Value = Convert.ToInt32(enumValue);
                        returnVal.Name = enumValue.ToString();
                        cachedEnum.Add(key, returnVal);
                    }
                }
            }
            if (returnVal == null)
                throw new ApplicationException("枚举类型[" + enumType.Name + "]未定义属性EnumDescriptionAttribute");
            return returnVal;
        }
    }
}
