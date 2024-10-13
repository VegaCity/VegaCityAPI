namespace VegaCityApp.API.Utils
{
    public static class TimeUtils
    {
        public static string GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssff");
        }

        public static string GetHoursTime(DateTime value)
        {
            return value.ToString("H:mm");
        }

        // config on server ubuntu, docker
        public static DateTime GetCurrentSEATime()
        {
            TimeZoneInfo tz = GetSEATimeZone();
            DateTime localTime = DateTime.Now;
            DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
            return utcTime;
        }
        public static TimeZoneInfo GetSEATimeZone()
        {
            TimeZoneInfo tz;
            try
            {
                // Try using Windows time zone
                tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                // Fallback to IANA time zone for Linux/Docker
                tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            }
            return tz;
        }
        // config on server ubuntu, docker
        public static DateTime ConvertToSEATime(DateTime value)
        {
            TimeZoneInfo tz = GetSEATimeZone();
            DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);
            return convertedTime;
        }
        //config on local or windows
        public static DateTime GetCurrenSEATimeOnWindowOrLocal()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime localTime = DateTime.Now;
            DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
            return utcTime;
        }
        public static DateTime ConvertToSEATimeOnWindowOrLocal(DateTime value)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);
            return convertedTime;
        }
    }
}
