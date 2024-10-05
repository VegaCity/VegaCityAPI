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

        public static DateTime GetCurrentSEATime()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            DateTime localTime = DateTime.Now;
            DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
            return utcTime;
        }

        public static DateTime ConvertToSEATime(DateTime value)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);
            return convertedTime;
        }
    }
}
