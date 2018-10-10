using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace NEL_Scan_API.lib
{
    public class NumberDecimalHelper
    {
        public static string formatDecimal(string numberDecimalStr)
        {
            string value = numberDecimalStr;
            if (numberDecimalStr.Contains("$numberDecimal"))
            {
                value = Convert.ToString(JObject.Parse(numberDecimalStr)["$numberDecimal"]);
            }
            if (value.Contains("E"))
            {
                value = decimal.Parse(value, NumberStyles.Float).ToString();
            }
            return value;
        }
    }
}
