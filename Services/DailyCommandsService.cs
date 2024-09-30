
using AccountingTer.TelegramExtentions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace AccountingTer.Services
{
    public class DailyCommandsService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        private Task _myLongRunningTask;
        public DailyCommandsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _myLongRunningTask = OpenSubscriptionAndProcessAsync(cancellationToken);
        }
        private async Task OpenSubscriptionAndProcessAsync(CancellationToken cancellationToken)
        {
            do
            {
                var scope = _scopeFactory.CreateScope();
                var serviceProvide = scope.ServiceProvider;
                var applicaliotContext = serviceProvide.GetRequiredService<ApplicationContext>();

                int hourForStatistic = 00;

                var list = await applicaliotContext.StringProperties.ToListAsync();

                var sadas = await applicaliotContext.StringProperties.FirstOrDefaultAsync(t => t.Key == Debug.DailyStatisticHour);
                if (sadas != null)

                    if (int.TryParse(sadas.Value, out int hourForStatisticInt))

                        hourForStatistic = hourForStatisticInt;
                if (DateTime.Now.Hour == hourForStatistic)
                {
                    var telegramBotService = serviceProvide.GetRequiredService<TelegramBotService>();
                    await telegramBotService.DoDailyCommands();
                }
                await Task.Delay(TimeSpan.FromHours(1), cancellationToken);
            }
            while (!cancellationToken.IsCancellationRequested);
        }
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }
}
