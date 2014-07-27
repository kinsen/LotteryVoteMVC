using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Mvc;

namespace LotteryVoteMVC.Utility
{
    public static class EnumableEx
    {
        public static void ForEach<T>(this IList<T> source, Action<T> action)
        {
            for (int i = 0; i < source.Count; i++)
                action(source[i]);
        }
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Func<T, T> action)
        {
            foreach (var item in source)
                yield return action(item);
        }
        public static T Find<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            foreach (var item in source)
                if (match(item)) return item;

            var emptyField = typeof(T).GetField("Empty", BindingFlags.Public | BindingFlags.Static);
            if (emptyField != null)
                return (T)emptyField.GetValue(null);
            return default(T);
        }
        public static T Find<T>(this IList<T> source, Predicate<T> match)
        {
            for (int i = 0; i < source.Count; i++)
                if (match(source[i]))
                    return source[i];
            var emptyField = typeof(T).GetField("Empty", BindingFlags.Public | BindingFlags.Static);
            if (emptyField != null)
                return (T)emptyField.GetValue(null);
            return default(T);
        }
        public static IList<T> List<T>(this IList<T> source, Predicate<T> match)
        {
            List<T> result = new List<T>();
            foreach (var item in source)
                if (match(item))
                    result.Add(item);
            return result;
        }
        public static IEnumerable<T> FindAll<T>(this IList<T> source, Predicate<T> match)
        {
            foreach (var item in source)
                if (match(item))
                    yield return item;
        }
        public static MultiSelectList ToMultiSelectList<T>(this IEnumerable<T> source, Func<T, string> getText, Func<T, string> getValue, Func<T, bool> isSelected = null)
        {
            if (isSelected == null)
                isSelected = it => false;
            var selectList = source.Select(it => new SelectListItem
            {
                Selected = isSelected(it),
                Text = getText(it),
                Value = getValue(it)
            });
            return new MultiSelectList(selectList, "Value", "Text", selectList.Where(it => it.Selected).Select(it => it.Value));
        }
        public static bool Contains<T>(this IEnumerable<T> source, Predicate<T> match)
        {
            foreach (var item in source)
                if (match(item)) return true;
            return false;
        }
        public static void AddRange<K, V>(this IDictionary<K, V> source, IDictionary<K, V> target)
        {
            foreach (var item in target)
                source[item.Key] = item.Value;
        }
    }
}
