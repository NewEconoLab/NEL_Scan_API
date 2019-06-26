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

        public static long GetTimeStampZeroBj()
        {
            var t1 = DateTime.UtcNow;
            var t2 = new DateTime(t1.Year, t1.Month, t1.Day - 1, 16, 0, 0, 0);
            return Convert.ToInt64((t2 - ZERO_SECONDS_Date).TotalSeconds);
        }
    }
}
