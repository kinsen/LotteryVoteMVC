using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace LotteryVoteMVC.Utility
{
    delegate bool ParseFunc<T, S>(T T1, out S S1);

    /// <summary>
    /// Cookie解析器
    /// </summary>
    public class CookieParser
    {
        public static HttpCookieCollection RequestCookies
        {
            get
            {
                return HttpContext.Current.Request.Cookies;
            }
        }
        public static HttpCookieCollection ResponseCookies
        {
            get
            {
                return HttpContext.Current.Response.Cookies;
            }
        }

        public HttpCookie Cookie { get; set; }

        public CookieParser(string cookieName)
        {
            if (string.IsNullOrEmpty(cookieName))
                throw new ArgumentNullException("CookieName can't be null or empty!");
            this.Cookie = RequestCookies[cookieName];
        }
        public CookieParser(HttpCookie cookie)
        {
            this.Cookie = cookie;
        }

        #region Cookie Values
        #region Int32
        /// <summary>
        /// 获取指定Cookie中指定索引键的Int类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <returns></returns>
        public int GetInt32(string key)
        {
            return getInt32(key, null);
        }
        /// <summary>
        ///获取指定Cookie中指定索引键的Int类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public int GetInt32(string key, int defaultValue)
        {
            return getInt32(key, defaultValue);
        }
        /// <summary>
        /// 获取指定Cookie中指定索引键的Int类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private int getInt32(string key, int? defaultValue)
        {
            return GetFormCookie<int>(key, (string k, out int v) => int.TryParse(k, out v), defaultValue);
        }
        #endregion
        #region Boolean
        /// <summary>
        /// 获取指定Cookie中指定索引键的Boolean类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <returns></returns>
        public bool GetBoolean(string key)
        {
            return getBoolean(key, null);
        }
        /// <summary>
        /// 获取指定Cookie中指定索引键的Boolean类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public bool GetBoolean(string key, bool defaultValue)
        {
            return getBoolean(key, defaultValue);
        }
        /// <summary>
        /// 获取指定Cookie中指定索引键的Boolean类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private bool getBoolean(string key, bool? defaultValue)
        {
            return GetFormCookie<bool>(key, (string k, out bool v) => bool.TryParse(k, out v), defaultValue);
        }
        #endregion
        #region String
        /// <summary>
        /// 获取指定Cookie中指定索引键的String类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <returns></returns>
        public string GetString(string key)
        {
            return GetFormCookie(key, false, string.Empty);
        }
        /// <summary>
        /// 获取指定Cookie中指定索引键的String类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public string GetString(string key, string defaultValue)
        {
            return GetFormCookie(key, false, defaultValue);
        }
        #endregion

        /// <summary>
        /// 获取指定Cookie中指定索引的值.
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="key">索引键.</param>
        /// <param name="action">将指定索引键的值转换为返回类型的值的委托方法.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private T GetFormCookie<T>(string key, ParseFunc<string, T> action, T? defaultValue) where T : struct
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("查询键值不能为空!");
            string value = Cookie[key];
            T returnValue = default(T);
            if (!string.IsNullOrEmpty(value))
            {
                if (action(value, out returnValue))
                    return returnValue;
                else
                    throw new InvalidCastException(string.Format("Name为{0}的Cookie值无法转换为{1}类型。", key, typeof(T).FullName));
            }
            else if (string.IsNullOrEmpty(value) && defaultValue.HasValue)
                return defaultValue.Value;
            else
                throw new ApplicationException(string.Format("Cookie中找不到key为{0}的值,且没有指定默认值", key));
        }
        /// <summary>
        /// 获取指定Cookie中指定索引的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="mustExsit">指定Web请求中是否必须需要存在有该名称的元素,传入false则方法返回默认值,反之抛出异常.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private string GetFormCookie(string key, bool mustExsit, string defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("查询键值不能为空!");
            string value = Cookie == null ? string.Empty : Cookie[key];
            if (string.IsNullOrEmpty(value) && mustExsit)
                throw new ApplicationException(string.Format("Cookie中必须存在键为{0}的项！", key));
            return value ?? defaultValue;
        }
        #endregion

        #region Cookie Value
        #region Int32
        /// <summary>
        /// 获取指定Cookie的Int类型的值.
        /// </summary>
        /// <returns></returns>
        public int GetValueToInt32()
        {
            return getValueToInt32(null);
        }
        /// <summary>
        ///获取指定Cookie的Int类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public int GetValueToInt32(int defaultValue)
        {
            return getValueToInt32(defaultValue);
        }
        /// <summary>
        /// 获取指定Cookie的Int类型的值.
        /// </summary>
        /// <param name="key">索引键.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private int getValueToInt32(int? defaultValue)
        {
            return GetCookieValue<int>((string k, out int v) => int.TryParse(k, out v), defaultValue);
        }
        #endregion
        #region Boolean
        /// <summary>
        /// 获取指定Cookie的Boolean类型的值.
        /// </summary>
        /// <returns></returns>
        public bool GetValueToBoolean()
        {
            return getValueToBoolean(null);
        }
        /// <summary>
        /// 获取指定Cookie的Boolean类型的值.
        /// </summary>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public bool GetValueToBoolean(bool defaultValue)
        {
            return getValueToBoolean(defaultValue);
        }
        /// <summary>
        /// 获取指定Cookie的Boolean类型的值.
        /// </summary>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private bool getValueToBoolean(bool? defaultValue)
        {
            return GetCookieValue<bool>((string k, out bool v) => bool.TryParse(k, out v), defaultValue);
        }
        #endregion
        #region String
        /// <summary>
        /// 获取指定Cookie的值.
        /// </summary>
        /// <returns></returns>
        public string GetValueToString()
        {
            return Cookie == null ? string.Empty : Cookie.Value;
        }
        /// <summary>
        /// 获取指定Cookie的值.
        /// </summary>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        public string GetValueToString(string defaultValue)
        {
            return Cookie == null ? null : Cookie.Value ?? defaultValue;
        }
        #endregion

        /// <summary>
        /// 获取指定Cookie的值.
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="action">将指定索引键的值转换为返回类型的值的委托方法.</param>
        /// <param name="defaultValue">默认值.</param>
        /// <returns></returns>
        private T GetCookieValue<T>(ParseFunc<string, T> action, T? defaultValue) where T : struct
        {
            string value = Cookie.Value;
            T returnValue = default(T);
            if (!string.IsNullOrEmpty(value))
            {
                if (action(value, out returnValue))
                    return returnValue;
                else
                    throw new InvalidCastException(string.Format("Cookie的值{0}无法转换为{1}类型。", value, typeof(T).FullName));
            }
            else if (string.IsNullOrEmpty(value) && defaultValue.HasValue)
                return defaultValue.Value;
            else
                throw new ApplicationException("Cookie值为空,且没有指定默认值");
        }
        #endregion
    }
}
