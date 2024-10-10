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
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho Chi Minh");
            DateTime localTime = DateTime.UtcNow;
            DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
            //DateTime utcTime = localTime.AddDays(7);
            return utcTime;
        }

        public static DateTime ConvertToSEATime(DateTime value)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);

           // DateTime convertedTime = value.AddHours(7);
            return convertedTime;
        }
    }
}
