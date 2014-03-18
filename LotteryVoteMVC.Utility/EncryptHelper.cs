using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace LotteryVoteMVC.Utility
{
    public class EncryptHelper
    {
        public static string Encrypt(string text)
        {
            return FormsAuthentication.HashPasswordForStoringInConfigFile(text, "MD5");
        }

        public static bool Equal(string source, string encryption)
        {
            var sourceEncrypt = Encrypt(source);
            return sourceEncrypt == encryption;
        }
    }
}
