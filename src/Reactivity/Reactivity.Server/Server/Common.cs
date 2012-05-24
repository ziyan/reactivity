using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Reactivity.Objects;

namespace Reactivity.Server
{
    static class Common
    {
        internal static Random Rand = new Random();

        #region Password
        internal static string EncryptPassword(string prefix, string hashpassword)
        {
            if (hashpassword.Length != Util.Hash.StringLength) throw new ArgumentOutOfRangeException();
            if (prefix.Length != PasswordPrefixLength) throw new ArgumentOutOfRangeException();
            return prefix.ToLower() + Util.Hash.ToString(hashpassword.ToLower() + prefix.ToLower());
        }
        internal static string GeneratePasswordPrefix()
        {
            string characters = "0123456789abcdef";
            string prefix = "";
            for (int i = 0; i < PasswordPrefixLength; i++)
                prefix += characters[Rand.Next(characters.Length)];
            return prefix;
        }
        internal static string GetPasswordPrefix(string password)
        {
            if (password.Length != Util.Hash.StringLength + PasswordPrefixLength) throw new ArgumentOutOfRangeException();
            return password.Substring(0, PasswordPrefixLength).ToLower();
        }
        private static int PasswordPrefixLength = 10;
        #endregion
    }
}
