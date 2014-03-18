using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;

namespace LotteryVoteMVC.Utility
{
    public class JSONHelper
    {
        public static string ToJson(object obj)
        {
            StringBuilder sb = new StringBuilder();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(obj.GetType());
            using (MemoryStream ms = new MemoryStream())
            {
                ds.WriteObject(ms, obj);
                sb.Append(Encoding.UTF8.GetString(ms.ToArray()));
            }
            return sb.ToString();
        }

        public static T ToObject<T>(string json) where T : class
        {
            try
            {
                DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    T obj = ds.ReadObject(ms) as T;
                    return obj;
                }
            }
            catch (Exception ex)
            {
                LogConsole.Error(json, ex);
                throw;
            }
        }
    }
}
