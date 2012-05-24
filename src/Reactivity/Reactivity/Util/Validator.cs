using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reactivity.Util
{
    public static class Validator
    {
        public static bool IsInteger(string integer)
        {
            return integer != null && integer != "" &&
                System.Text.RegularExpressions.Regex.Replace(integer, @"[\-]{0,1}[0-9]+", "") == "";
        }
        public static bool IsUsername(string username)
        {
            return username != null && username != "" &&
                System.Text.RegularExpressions.Regex.Replace(username, @"[0-9a-zA-Z\.\-_]{3,50}", "") == "";
        }
        public static bool IsEmail(string email)
        {
            return email != null && email != "" &&
                System.Text.RegularExpressions.Regex.Replace(email, @"([a-zA-Z0-9_\-\.])+@(([0-2]?[0-5]?[0-5]\.[0-2]?[0-5]?[0-5]\.[0-2]?[0-5]?[0-5]\.[0-2]?[0-5]?[0-5])|((([a-zA-Z0-9\-])+\.)+([a-zA-Z\-])+))", "") == "";
        }
    }
}
