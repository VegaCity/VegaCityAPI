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
            var corn = Cron.HourInterval(1);
            //var corn = Cron.MinuteInterval(1);
            var timeZone = TimeUtils.GetSEATimeZone();
            RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.CheckExpireWallet(), corn, timeZone: timeZone);
            //RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.EndDayCheckWalletCashier
            //(Guid.Parse(EnvironmentVariableConstant.marketZoneId)), cornDaily, timeZone: timeZone);
            //RecurringJob.AddOrUpdate<IEtagService>(x => x.CheckEtagExpire(), corn, timeZone: timeZone);
        }
    }
    public class RoleConfig
    {
        public static void InitRole()
        {
            //when run system, init role
            RecurringJob.AddOrUpdate<IAccountService>(x => x.AddRole(), Cron.Minutely());
        }
    }
}
