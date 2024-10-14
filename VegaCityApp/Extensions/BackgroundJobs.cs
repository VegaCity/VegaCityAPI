using Hangfire;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
namespace VegaCityApp.API.Extensions
{
    public class BackgroundJobs
    {
        public static void RecurringJobs()
        {
            //var corn = Cron.Daily();
            var corn = Cron.HourInterval(1);
            var timeZone = TimeUtils.GetSEATimeZone();
            RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.CheckExpireWallet(), corn, timeZone: timeZone);
            RecurringJob.AddOrUpdate<IEtagService>(x => x.CheckEtagExpire(), corn, timeZone: timeZone);
        }
    }
}
