using AccountingTer.Models;
using AccountingTer.TelegramExtentions;
using Microsoft.Extensions.Options;
using NuGet.Protocol.Plugins;
using System;
using System.Reflection;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace AccountingTer.Services
{
    public class TelegramBotService : IHostedService
    {
        private readonly TelegramOptions _telegramOptions;
        private readonly IServiceScopeFactory _scopeFactory;

        private ITelegramBotClient _botClient;

        private ReceiverOptions _receiverOptions;
        private CancellationTokenSource _cancellationToken1;

        public TelegramBotService(IServiceScopeFactory scopeFactory, IOptions<TelegramOptions> options)
        {
            _scopeFactory = scopeFactory;
            _telegramOptions = options.Value;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _botClient = new TelegramBotClient(_telegramOptions.Token);
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery
                },
                ThrowPendingUpdates = true,
            };
            using var cts = new CancellationTokenSource();

            _cancellationToken1 = cts;

            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);
        }

        public async Task DoDailyCommands()
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var telegramController = scope.ServiceProvider.GetRequiredService<TelegramController>();

            await SendStatictic(telegramController);
            await BackupDataBase(telegramController);
        }
        public async Task SendStatictic(TelegramController telegramController)
        {
            await telegramController.SendStatistic(_botClient);
        }
        private async Task BackupDataBase(TelegramController telegramController)
        {
            await telegramController.BackupDataBase(_botClient);
        }


        private async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();
            var telegramController = scope.ServiceProvider.GetRequiredService<TelegramController>();

            var controllerMethods = telegramController.GetType().GetMethods().Where(t => t.GetCustomAttribute<MessageMethodAttribute>() != null).Select(t => new ControllerMethodInfo(t.GetCustomAttribute<MessageMethodAttribute>(), t));
            var botNameData = await botClient.GetMyNameAsync();
            string botName = $"@{botNameData.Name}_bot";
            try
            {
                ControllerMethodInfo method = null;
                if (update.Type == UpdateType.Message)
                    method = controllerMethods.FirstOrDefault(t => update?.Message?.Text?.Split(" ").First().Replace(botName, "") == t.Attribute.Command && t.Attribute.CommandType == update.Type);
                else if (update.Type == UpdateType.CallbackQuery)
                    method = controllerMethods.FirstOrDefault(t => update.CallbackQuery.Data == t.Attribute.Command && t.Attribute.CommandType == update.Type);

                if (method != null)
                {
                    var commandTask = (Task)method.Method?.Invoke(telegramController, new object[] { botClient, update, cancellationToken });
                    await commandTask;
                }
                else
                {
                    await telegramController.UnknowCommand(botClient, update, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                if(Debug.Enable)
                    await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Ошибка\n{ex.ToString()}",
                    disableNotification:true,
                    replyToMessageId: update.Message.MessageId);

                return;
            }
        }

        private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationToken1.Cancel();
            return Task.CompletedTask;
        }
    }
}
