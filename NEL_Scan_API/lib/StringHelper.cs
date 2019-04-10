using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NEL_Scan_API.lib
{
    public class StringHelper
    {
        private static Regex mailRegex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
        public static bool validateEmail(string mail)
        {
            return mailRegex.IsMatch(mail);
        }
    }
}
