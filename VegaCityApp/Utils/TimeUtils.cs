namespace VegaCityApp.API.Utils
{
    public static class TimeUtils
    {
        public static string GetTimestamp(DateTime value)
        {
            DateTime seaTime = value.AddHours(7);
            return seaTime.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ");
        }

        //public static string GetHoursTime(DateTime value)
        //{
        //    return value.ToString("H:mm");
        //}

        public static DateTime GetCurrentSEATime()
        {
            //TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //DateTime localTime = DateTime.UtcNow;
            ////DateTime utcTime = TimeZoneInfo.ConvertTime(localTime, TimeZoneInfo.Local, tz);
            ////DateTime utcTime = localTime.AddDays(7);
            //return localTime;
            DateTime utcTime = DateTime.UtcNow; // Get the current UTC time
            DateTime convertedTime = utcTime.AddHours(7); // Add 7 hours for Ho Chi Minh City's time (UTC+7)
            return convertedTime;
        }

        public static DateTime ConvertToSEATime(DateTime value)
        {
            //TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            //DateTime convertedTime = TimeZoneInfo.ConvertTime(value, tz);

            DateTime convertedTime = value.AddHours(7);
            return value;
        }
    }
}
