using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LotteryVoteMVC.Utility
{
    public static class EnumHelper
    {
        public static IEnumerable<SelectListItem> ToSelectList<T>(Predicate<T> isSelected, Func<T, string> text, Func<T, string> value) where T : struct
        {
            return GetAll<T>().Select(it => new SelectListItem
            {
                Selected = isSelected(it),
                Text = text(it),
                Value = value(it)
            });
        }
        public static MultiSelectList ToMultiSelectList<T>(Predicate<T> isSelected, Func<T, string> text, Func<T, string> value) where T : struct
        {
            var data = ToSelectList<T>(isSelected, text, value);
            return new MultiSelectList(data, "Value", "Text", data.Where(it => it.Selected).Select(it => it.Value));
        }

        public static IEnumerable<T> GetAll<T>() where T : struct
        {
            var values = Enum.GetValues(typeof(T));
            foreach (var v in values)
                yield return (T)v;
        }

        public static T GetEnum<T>(string value) where T : struct
        {
            T result;
            if (!Enum.TryParse<T>(value, out result))
                result = default(T);
            return result;
        }
        public static IList<EnumDescriptionAttribute> GetDescription<T>() where T : struct
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new ArgumentException(enumType.FullName + " 不是枚举类型!");
            List<EnumDescriptionAttribute> descList = new List<EnumDescriptionAttribute>();
            foreach (var name in Enum.GetNames(enumType))
            {
                descList.Add(EnumDescriptionAttribute.GetEnumDesc(GetEnum<T>(name)));
            }
            return descList;
        }
        public static EnumDescriptionAttribute GetEnumDescript(Enum enumValue)
        {
            return EnumDescriptionAttribute.GetEnumDesc(enumValue);
        }
    }
}
