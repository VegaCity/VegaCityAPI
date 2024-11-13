using Hangfire;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Service.Interface;
namespace VegaCityApp.API.Extensions
{
    public class BackgroundJobs
    {
        public static void RecurringJobs()
        {
            var cornDaily = Cron.Daily();
            var cornHour = Cron.HourInterval(1);
            var cornMinute = Cron.MinuteInterval(5);
            var timeZone = TimeUtils.GetSEATimeZone();
            RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.CheckExpireWallet(), cornHour, timeZone: timeZone);

            RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.EndDayCheckWalletCashier
            (Guid.Parse(EnvironmentVariableConstant.marketZoneId)), cornDaily, timeZone: timeZone);

            RecurringJob.AddOrUpdate<IPackageService>(x => x.CheckPackageItemExpire(), cornHour, timeZone: timeZone);

            RecurringJob.AddOrUpdate<IPackageService>(x => x.SolveWalletPackageItem(Guid.Parse(EnvironmentVariableConstant.marketZoneId)), 
                cornDaily, timeZone: timeZone);

            RecurringJob.AddOrUpdate<IPromotionService>(x => x.CheckExpiredPromotion(), cornMinute, timeZone: timeZone);

            RecurringJob.AddOrUpdate<IOrderService>(x => x.CheckOrderPending(), cornMinute, timeZone: timeZone);
        }
    }
}
