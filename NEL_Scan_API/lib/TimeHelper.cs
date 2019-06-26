using System;

namespace NEL_Scan_API.lib
{
    public class TimeHelper
    {

        private static DateTime ZERO_SECONDS_Date = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        public static long GetTimeStamp()
        {
            TimeSpan st = DateTime.UtcNow - ZERO_SECONDS_Date;
            return Convert.ToInt64(st.TotalSeconds);
        }

        public static long GetTimeStampZero()
        {
            TimeSpan st = DateTime.UtcNow.Date - ZERO_SECONDS_Date;
            return Convert.ToInt64(st.TotalSeconds);
        }
    }
}
