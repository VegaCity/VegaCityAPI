using Hangfire;
using VegaCityApp.API.Services.Interface;
namespace VegaCityApp.API.Extensions
{
    public class BackgroundJobs
    {
        public static void RecurringJobs()
        {
            //var corn = Cron.Daily();
            var corn = Cron.MinuteInterval(60);

            RecurringJob.AddOrUpdate<IWalletTypeService>(x => x.checkExpireWallet(), corn);

        }
    }
}
