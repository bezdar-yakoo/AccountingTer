using AccountingTer.Models;
using AccountingTer.Services;
using bybit.net.api.ApiServiceImp;
using bybit.net.api.Models.Account;
using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using CryptoExchange.Net;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace AccountingTer.TelegramExtentions
{
    public class TelegramController
    {
        private readonly TelegramOptions _telegramOptions;
        private readonly ApplicationContext _applicationContext;
        private InlineKeyboardMarkup CancelCommadMarkup
        {
            get
            {
                return new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>()
                    {
                        new InlineKeyboardButton[]
                        {
                            InlineKeyboardButton.WithCallbackData("Отменить запрос", "cancelcommand"),
                        }
                    });
            }
        }
        private IReplyMarkup ConfirmCancelCommandMarkup
        {
            get
            {
                return new InlineKeyboardMarkup(
                                new List<InlineKeyboardButton[]>()
                                {
                                    new InlineKeyboardButton[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Подтвердить", "confirm"),
                                        InlineKeyboardButton.WithCallbackData("Отменить запрос", "cancelcommand"),
                                    }
                                });
            }

        }

        private readonly Regex CommadsRegex = new Regex(@"^(\/[a-zA-Z]*)@?\S* ?(\@\S*)? ?(\d*)? ?(.*)", RegexOptions.IgnoreCase);

        public TelegramController(IOptions<TelegramOptions> options, ApplicationContext applicationContext)
        {
            _telegramOptions = options.Value;
            _applicationContext = applicationContext;
        }

        #region AddCommands

        [MessageMethod("/add", UpdateType.Message)]
        public async Task Add(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (await CheckMessageSignature(botClient, update) == false) return;
            if (await UserIsOwner(botClient, update) == false)
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Выполнять команды на изменение баланса могут только владельцы!",
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);


            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {update.Message.Text}\nРаспределить {commandData.Value} между двумя балансами поровнус описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        [MessageMethod("/addto", UpdateType.Message)]
        public async Task AddTo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            if (await UserIsOwner(botClient, update) == false) return;

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);

            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {update.Message.Text}\nДобавить {commandData.Value} на баланс {update.Message.From.Username} с описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        [MessageMethod("/addme", UpdateType.Message)]
        public async Task AddToMe(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (await CheckMessageSignature(botClient, update) == false) return;
            if (await UserIsOwner(botClient, update) == false) return;

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);

            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {commandData.Text}\nДобавить {commandData.Value} на баланс {update.Message.From.Username} с описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        #endregion

        #region WithdrawCommands

        [MessageMethod("/withdraw", UpdateType.Message)]
        public async Task Withdraw(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (await CheckMessageSignature(botClient, update) == false) return;
            if (await UserIsOwner(botClient, update) == false) return;

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);


            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {update.Message.Text}\nСписать {commandData.Value} с общего баланса с описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        [MessageMethod("/withdrawme", UpdateType.Message)]
        public async Task WithdrawForMe(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (await CheckMessageSignature(botClient, update) == false) return;
            if (await UserIsOwner(botClient, update) == false) return;

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);


            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {update.Message.Text}\nСписать {commandData.Value} с баланса {update.Message.From.Username} с описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        [MessageMethod("/withdrawto", UpdateType.Message)]
        public async Task WithdrawTo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            if (await UserIsOwner(botClient, update) == false) return;

            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);

            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    $"Команда: {update.Message.Text}\nСписать {commandData.Value} с баланса @{commandData.UserName} с описанием {commandData.Description}?",
                    replyMarkup: ConfirmCancelCommandMarkup,
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        #endregion

        #region Debug

        [MessageMethod("/chatinfo", UpdateType.Message)] // needTest
        public async Task GetChatInfo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (Debug.Enable)
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                    JsonSerializer.Serialize(update.Message.Chat),
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            else
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
              "Включите дебаг режим",
               replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/debugenable", UpdateType.Message)]
        public async Task DebugEnable(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.Enable = true;
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Debug mode: {Debug.Enable}",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/debugdisable", UpdateType.Message)]
        public async Task DebugDisable(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            Debug.Enable = false;
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Debug mode: {Debug.Enable}",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }
        #endregion

        #region Stats

        [MessageMethod("/balance", UpdateType.Message)] // needTest
        public async Task Balance(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var owners = _applicationContext.Owners;
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                string.Join(' ', owners.Select(t => $"Баланс пользователя {t.UserName}({t.Description}): {t.Balance}$\n")) + $"Общий баланс: {owners.Sum(t => t.Balance)}$",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/balancebybit", UpdateType.Message)]
        public async Task BalanceBybit(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var bybitCredData = await _applicationContext.StringProperties.FirstOrDefaultAsync(t => t.Key == Debug.BybitCredentials);
            if (bybitCredData == null)
            {
                await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Креды указанны не правильно",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId  );
                return;
            }
            var creds = bybitCredData.Value.Split(':');
            var asd = new BybitRestClient(options =>
            {
                options.ApiCredentials = new ApiCredentials(creds[0], creds[1]);
            });

            var res = await asd.V5Api.Account.GetAllAssetBalancesAsync(Bybit.Net.Enums.AccountType.Fund);

            if (res.Success == false)
            {
                await botClient.SendTextMessageAsync(
                 update.Message.Chat.Id,
                 $"Ошибка запроса",
                 replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }


            string data = string.Join(" | ", res.Data.Balances.Select(t => $"{t.Asset} - {t.WalletBalance}"));

            await botClient.SendTextMessageAsync(
                 update.Message.Chat.Id,
                 $"{data}",
                 replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/stats", UpdateType.Message)] // needTest
        public async Task Statistics(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (DateTime.TryParse(update.Message.Text.Replace("/stats", ""), out DateTime result))
                await SendStatistic(botClient, update, result);
            else
                await SendStatistic(botClient, update, DateTime.Today);
        }
        public async Task BackupDataBase(ITelegramBotClient botClient)
        {
            var settings = await _applicationContext.StringProperties.FirstOrDefaultAsync(t => t.Key == "IdsForBackupDataBase");
            if (settings == null)
                return;
            var idsForBackup = settings.Value.Split(", ").Select(t => long.Parse(t));
            var dirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            DirectoryInfo directoryInfo = new DirectoryInfo(dirPath);
            var files = directoryInfo.GetFiles("mydb*");

            foreach (var id in idsForBackup)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var stream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
                        InputFile iof = InputFile.FromStream(stream, file.Name);
                        var send = await botClient.SendDocumentAsync(id, iof, caption: $"Дамп базы за {DateTime.Now.ToString("d.MM.yy HH:mm")}", disableNotification: true);
                    }
                    catch
                    {

                    }
                }
            }
        }
        public async Task SendStatistic(ITelegramBotClient botClient, Update? update = null, DateTime? dateTime = null)
        {
            IEnumerable<long> chatsIdString = null;

            DateTime dt = DateTime.Today;

            if (dateTime != null) dt = dateTime.Value;

            if (update != null)
            {
                chatsIdString = new long[] { update.Message.Chat.Id };
            }
            else
            {
                var chatsId = await _applicationContext.StringProperties.FirstOrDefaultAsync(t => t.Key == "ChatsForStatistic");
                if (chatsId == null)
                    return;
                else
                    chatsIdString = chatsId.Value.Split(", ").Select(t => long.Parse(t));
            }
            var owners = _applicationContext.Owners;
            var balanceEventsToday = from balanceEvents in _applicationContext.BalanceEvents
                                     where balanceEvents.DateTime.Date == dt
                                     join owner in _applicationContext.Owners on balanceEvents.OwnerId equals owner.Id
                                     select new { balanceEvents, owner };

            var plusTransaction = balanceEventsToday.Where(t => t.balanceEvents.IsAdded == true);
            var minusTransaction = balanceEventsToday.Where(t => t.balanceEvents.IsAdded == false);

            var plusToday = plusTransaction.Sum(t => t.balanceEvents.Value);
            var minusToday = minusTransaction.Sum(t => t.balanceEvents.Value);

            var balanceNow = _applicationContext.Owners.Sum(t => t.Balance);
            var todayResult = plusToday - minusToday;
            var balanceTomorrow = balanceNow - plusToday + minusToday;

            var anyoneChange = ((plusTransaction.Where(t => t.balanceEvents.OwnerBalanceId < 0).Sum(t => t.balanceEvents.Value) - minusTransaction.Where(t => t.balanceEvents.OwnerBalanceId < 0).Sum(t => t.balanceEvents.Value)) / 2);

            var ownerStatistic = owners.Select(o => new { UserName = o.UserName, Value = (plusTransaction.Where(t => t.balanceEvents.OwnerBalanceId == o.Id).Where(t => t.balanceEvents.OwnerBalanceId > 0).Sum(t => t.balanceEvents.Value) - minusTransaction.Where(t => t.balanceEvents.OwnerBalanceId == o.Id).Where(t => t.balanceEvents.OwnerBalanceId > 0).Sum(t => t.balanceEvents.Value) + anyoneChange) });

            foreach (var item in chatsIdString)
            {
                try
                {
                    await botClient.SendTextMessageAsync(item,
                                  $"Статистика на {dt.ToString("dd.MM.yyyy HH:mm")}:\n\n" +
                                  string.Join(string.Empty, balanceEventsToday
                                  .Select(t => $"{t.balanceEvents.DateTime.ToString("HH:mm")} | {(t.balanceEvents.IsAdded == true ? "+" : "-")}{t.balanceEvents.Value}$ {(owners.Any(o => o.Id == t.balanceEvents.OwnerBalanceId) ? $"со своего счета @{owners.First(o => o.Id == t.balanceEvents.OwnerBalanceId).UserName}" : "с общего счета")} Вызвал @{t.owner.UserName} - {t.balanceEvents.Description}\n")) +
                                  $"\nИтог:\n" +
                                  string.Join(" | ", ownerStatistic.Select(t => $"{t.UserName} : {(t.Value > 0 ? "+" : "")}{t.Value}$")) +
                                  $"\nНачало: {balanceTomorrow}$ | {(todayResult > 0 ? "+" : "")}{todayResult}$ | Сейчас: {balanceNow}$", disableNotification: true
                                  , messageThreadId: update?.Message.MessageThreadId);
                }
                catch { }
            }


        }

        #endregion

        #region DatabaseValues

        [MessageMethod("/propinfo", UpdateType.Message)] // complite
        public async Task DatabasePropertiesInfo(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var props = await _applicationContext.StringProperties.Select(t => new { t.Key, t.Value, t.Description }).ToListAsync();
            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                 "Данные, доступные для редактирования:\n" +
                 string.Join(string.Empty, props.Select(t => $"{t.Key} - {t.Description}\n")) +
                "\nПример использования: /setprop ChatsForStatistic 54, 96 ,45    - теперь бот будет присылать статистику на указанные чаты"
                , disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/setprop", UpdateType.Message)] // complite
        public async Task SetDatabaseProperties(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string message = update.Message.Text.Replace("/setprop ", "");
            if (string.IsNullOrWhiteSpace(message))
            {
                await botClient.SendTextMessageAsync(
               update.Message.Chat.Id,
               "Параметр не передан"
               , disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            var tt = message.Split(" ");
            var propKey = tt[0];
            var propValue = message.Replace(propKey, "");

            if (string.IsNullOrWhiteSpace(propKey) || string.IsNullOrWhiteSpace(propValue))
            {
                await botClient.SendTextMessageAsync(
               update.Message.Chat.Id,
               "Переданы не все параметры", disableNotification: true
               , messageThreadId: update.Message.MessageThreadId);
                return;
            }
            var dbProp = await _applicationContext.StringProperties.FirstOrDefaultAsync(t => t.Key == propKey);
            if (dbProp == null)
            {
                if (Debug.Enable)
                    await botClient.SendTextMessageAsync(
                   update.Message.Chat.Id,
                   $"Запись {propKey} не найдена"
                   , disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            dbProp.Value = propValue.Trim();
            _applicationContext.Update(dbProp);
            await _applicationContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(
                  update.Message.Chat.Id,
                  $"Запись {propKey} успешно изменена"
                  , disableNotification: true, messageThreadId: update.Message.MessageThreadId);

        }

        [MessageMethod("/getprop", UpdateType.Message)] // complite
        public async Task GetDatabaseProperties(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var props = await _applicationContext.StringProperties.Select(t => new { t.Key, t.Value }).ToListAsync();
            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Данные, записанные в базу данных:\n" +
                string.Join(string.Empty, props.Select(t => $"{t.Key} : {t.Value}\n"))
                , disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }


        #endregion

        #region HelpCommands

        [MessageMethod("/start", UpdateType.Message)] // compile
        public async Task Start(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var replyKeyboard =
                new ReplyKeyboardMarkup(
                    new List<KeyboardButton[]>()
                    {
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/add"),
                            new KeyboardButton("/addme"),
                            new KeyboardButton("/addto"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/withdraw"),
                            new KeyboardButton("/withdrawme"),
                            new KeyboardButton("/withdrawto"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/changebalance"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/balance"),
                            new KeyboardButton("/stats"),
                            new KeyboardButton("/balancebybit"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/info"),
                            new KeyboardButton("/hide"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/changebalance"),
                        },
                    });
            replyKeyboard.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Показанны возможные комманды",
                replyMarkup: replyKeyboard, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/hide", UpdateType.Message)] // complite
        public async Task HideCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Комманды скрыты", replyMarkup: new ReplyKeyboardRemove(), disableNotification: true, messageThreadId: update.Message.MessageThreadId);

        [MessageMethod("/info", UpdateType.Message)] // complite
        public async Task Info(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var urlData = await _applicationContext.StringProperties.FirstOrDefaultAsync(t => t.Key == Debug.Url);

            InlineKeyboardMarkup inlineKeyboard = null;

            if (urlData != null)
            if (string.IsNullOrWhiteSpace(urlData.Value) == false)
            {
                inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>()
                {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithUrl("Сайт с данными", urlData.Value)
                }
                });
            }

            if (update.Message.From.Username != "Nesect" && update.Message.From.Username != "mishkanyaa")
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,

                "Комманды бота:\n\n" +
                "Баланс:\n" +
                "/balance - узнать баланс\n" +
                "/balancebybit - узнать баланс биржи\n" +
                "/stats - подсчитать смету за указанный день (по умолчанию сегодня) (/stats 27.09.2024 23:59)\n" +

                "\n" +

                "Пополнить баланс:\n" +
                "/add - добавить на общий счет (/add сколько описание)\n" +
                "/addme - добавить только мне (/addme сколько описание)\n" +
                "/addto - добавить на чужой счет (/addto @кому сколько описание)\n" +

                "\n" +

                "Снять с баланса:\n" +
                "/withdraw - снять с общего счета (/withdraw сколько описание)\n" +
                "/withdrawme - снять с моего счета (/withdrawme сколько описание)\n" +
                "/withdrawto - снять с чужого счета (/withdrawto @кому сколько описание)\n" +

                "\n" +

                "Изменить баланс:\n" +
                "/changebalance - посчитать разность юалансов и добавить недостоющую сумму (/changebalance баланс)\n" +

                "\n" +

                "Помощь:\n" +
                "/info - данное сообщение\n" +
                "/start - показать меню команд\n" +
                "/hide - скрыть меню команд\n" +

                "\n" +

                "Пример использования с указанием данных:\n/addto @nesect 500 твой оффер выкупили - пополнить баланс @nesect на 500$ с описанием \"купили биток\"",
                replyMarkup: inlineKeyboard, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
            else
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,

                "Команды бота:\n\n" +
                "Баланс:\n" +
                "/balance - узнать баланс\n" +
                "/balancebybit - узнать баланс биржи\n" +
                "/stats - подсчитать смету за указанный день (по умолчанию сегодня) (/stats 27.09.2024 23:59)\n" +

                "\n" +

                "Пополнить баланс:\n" +
                "/add - добавить на общий счет (/add сколько описание)\n" +
                "/addme - добавить только мне (/addme сколько описание)\n" +
                "/addto - добавить на чужой счет (/addto @кому сколько описание)\n" +

                "\n" +

                "Снять с баланса:\n" +
                "/withdraw - снять с общего счета (/withdraw сколько описание)\n" +
                "/withdrawme - снять с моего счета (/withdrawme сколько описание)\n" +
                "/withdrawto - снять с чужого счета (/withdrawto @кому сколько описание)\n" +

                "\n" +

                "Изменить баланс:\n" +
                "/changebalance - посчитать разность юалансов и добавить недостоющую сумму (/changebalance баланс)\n" +

                "\n" +

                "Помощь:\n" +
                "/info - данное сообщение\n" +
                "/start - показать меню команд\n" +
                "/hide - скрыть меню команд\n" +

                "\n" +

                "Работа с данными из бд:\n" +
                "/propinfo - список всех данных и их описание\n" +
                "/getprop -  список всех данных и их значение \n" +
                "/setprop - изменить данные в базе данных (/setprop название значение)\n" +

                "\n" +

                "Системные:\n" +
                "/dropdb - очистить базу данных\n" +
                "/register - зарегистрировать нового пользователя (/register @кого баланс описание)\n" +
                "/deleteuser - удалить пользователя (/deleteuser @кого)\n" +
                "/chatinfo - получить информацию о чате\n" +
                "/debugdisable - выключить режим дебага\n" +
                "/chatinfo - включить режим дебага\n" +

                "\n" +

                "Пример использования с указанием данных:\n/addto @nesect 500 твой оффер выкупили - пополнить баланс @nesect на 500$ с описанием \"купили биток\"",
                replyMarkup: inlineKeyboard, disableNotification: true, messageThreadId: update.Message.MessageThreadId);

            }
        }

        [MessageMethod("/dropdb", UpdateType.Message)]
        public async Task DropBD(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Username != "Nesect" && update.Message.From.Username != "mishkanyaa")
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "У вас недостаточно прав!",
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }

            await botClient.SendTextMessageAsync(
                  update.Message.Chat.Id,
                  $"Команда: {update.Message.Text}\nДропнуть базу данных?",
                  replyMarkup: ConfirmCancelCommandMarkup,
                  replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        [MessageMethod("/deleteuser", UpdateType.Message)]
        public async Task DeleteUser(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Username != "Nesect" && update.Message.From.Username != "mishkanyaa")
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                   "У вас недостаточно прав!",
                    replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);

            if (commandData.UserName == null)
            {
                await botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                       "Не указан id пользователя",
                        replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }

            var user = await _applicationContext.Owners.FirstOrDefaultAsync(t => t.UserName == commandData.UserName);
            if (user == null)
            {
                await botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                       "Пользователь не найден",
                        replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }

            _applicationContext.Owners.Remove(user);
            await _applicationContext.SaveChangesAsync();

            await botClient.SendTextMessageAsync(
                       update.Message.Chat.Id,
                      $"Пользователь {commandData.UserName} удален!",
                       replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        #endregion

        #region CallbackQuery

        [MessageMethod("cancelcommand", UpdateType.CallbackQuery)] // complite
        public async Task CancelCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, "Запрос был отменен. Данные не внесены!");

        [MessageMethod("confirm", UpdateType.CallbackQuery)] // complite
        public async Task ConfirmCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                var callbackQuery = update.CallbackQuery;
                var replyMessageWithCommand = callbackQuery.Message.Text;
                var botNameData = await botClient.GetMyNameAsync();
                string botName = $"@{botNameData.Name}_bot";
                await ExecuteCommand(botClient, update, cancellationToken, replyMessageWithCommand.Replace(botName, ""));

                await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, "Запрос был выполнен. Данные внесены в базу данных");
            }
            catch (Exception ex)
            {

                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                $"Ошибка\n{ex.ToString()}",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message?.MessageThreadId);
                return;
            }
        }
        #endregion

        public async Task UnknowCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message.ReplyToMessage != null)
                {
                    if (update.Message.ReplyToMessage.From.IsBot == false) return;

                    var userAnswer = update.Message;
                    var botQuestion = userAnswer.ReplyToMessage;
                    var command = botQuestion.Text.Split("\n").First();

                    async Task SendConfirmMessage(string messageText)
                    {
                        await botClient.EditMessageTextAsync(botQuestion.Chat, botQuestion.MessageId, botQuestion.Text, replyMarkup: null);
                        await botClient.SendTextMessageAsync(
                        update.Message.Chat.Id,
                        messageText,
                        replyMarkup: new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>()
                        {
                                    new InlineKeyboardButton[]
                                    {
                                        InlineKeyboardButton.WithCallbackData("Подтвердить", "confirm"),
                                        InlineKeyboardButton.WithCallbackData("Отменить запрос", "cancelcommand"),
                                    }
                        }),
                        replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                    }


                    var commandParams = userAnswer.Text;
                    var paramCollection = commandParams.Split(" ");

                    (string, string) valueWithDescription = (paramCollection.First(), "Описание отсутствует");

                    if (paramCollection.Length > 1)
                    {
                        valueWithDescription.Item2 = commandParams.Replace(valueWithDescription.Item1, "");
                    }

                    if (command == "/add")
                    {
                        await SendConfirmMessage($"Команда: /add {userAnswer.Text}\n" +
                            $"Распределить {valueWithDescription.Item1} между двумя балансами поровну с описанием \"{valueWithDescription.Item2}\"?");
                    }
                    else if (command == "/addme")
                    {
                        await SendConfirmMessage($"Команда: /addme {userAnswer.Text}\n" +
                            $"Добавить {valueWithDescription.Item1} на баланс {userAnswer.From.Username} с описанием \"{valueWithDescription.Item2}\"?");
                    }
                    else if (command == "/withdraw")
                    {
                        await SendConfirmMessage($"Команда: /withdraw {userAnswer.Text}\n" +
                            $"Списать {valueWithDescription.Item1} с общего баланса с описанием \"{valueWithDescription.Item2}\"?");
                    }
                    else if (command == "/withdrawme")
                    {
                        await SendConfirmMessage($"Команда: /withdrawme {userAnswer.Text}\n" +
                            $"Списать {valueWithDescription.Item1} с баланса {userAnswer.From.Username} с описанием \"{valueWithDescription.Item2}\"?");
                    }
                    else if (command == "/changebalance")
                    { // Текущий зарегистрированный баланс: {balanceNow}. Указанный баланс {valueStr}. Распределить {balanceNow - value} между двумя балансами поровну?"
                        var balanceNow = _applicationContext.Owners.Sum(t => t.Balance);
                        await SendConfirmMessage($"Команда: /changebalance {userAnswer.Text}\n" +
                           $"Текущий зарегистрированный баланс: {balanceNow}. Указанный баланс {userAnswer.Text}. Распределить прибыль ({int.Parse(valueWithDescription.Item1) - balanceNow}) поровну между двумя балансами поровну?");
                    }
                    else
                    {
                        Console.WriteLine("Команда не распознана");
                    }
                }
            }
        }

        [MessageMethod("/changebalance", UpdateType.Message)] 
        public async Task ChangeBalance(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (await CheckMessageSignature(botClient, update) == false) return;
            if (await UserIsOwner(botClient, update) == false) return;


            var collection = CommadsRegex.Match(update.Message.Text).Groups;
            var commandData = new CommandData(collection);
            var balanceNow = _applicationContext.Owners.Sum(t => t.Balance);


            if (commandData.Value == null)
            {
                await SendInstruction(botClient, update, cancellationToken, commandData);
            }
            else
            {
                await botClient.SendTextMessageAsync(
              update.Message.Chat.Id,
              $"Команда: {update.Message.Text}\nТекущий зарегистрированный баланс: {balanceNow}. Указанный баланс {commandData.Value}. Распределить {balanceNow - commandData.Value.Value} между двумя балансами поровну?",
              replyMarkup: ConfirmCancelCommandMarkup,
              replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            }
        }

        [MessageMethod("/register", UpdateType.Message)]
        public async Task Register(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            var collection = CommadsRegex.Match(update.Message.Text).Groups;

            var commandData = new CommandData(collection);

            var newUser = new Owner();

            var addedBalanceEvent = new BalanceEvent()
            {
                Description = "Регистрация",
                IsAdded = true
            };
            string userName = commandData.UserName;
            if (userName == null)
                userName = update.Message.From.Username;

            if (_applicationContext.Owners.Any(t => t.UserName == userName))
            {
                await botClient.SendTextMessageAsync(
               update.Message.Chat.Id,
               "Такой пользователь уже зарегистрирован",
               replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return;
            }
            newUser.UserName = userName;
            if (commandData.Value != null)
            {
                addedBalanceEvent.Value = commandData.Value.Value;
                newUser.Balance = addedBalanceEvent.Value;
            }
            if (commandData.Description != null)
            {
                newUser.Description = commandData.Description;
            }
            newUser.Description = commandData.Description != null ? commandData.Description : newUser.UserName;

            var userEntity = await _applicationContext.Owners.AddAsync(newUser);
            await _applicationContext.SaveChangesAsync();
            if (commandData.Value != null)
            {
                addedBalanceEvent.OwnerBalanceId = userEntity.Entity.Id;
                addedBalanceEvent.OwnerId = userEntity.Entity.Id;
                await _applicationContext.BalanceEvents.AddAsync(addedBalanceEvent);
            }
            await _applicationContext.SaveChangesAsync();
            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                $"Новый пользователь добавлен!\n@{newUser.UserName}({newUser.Description}) - {newUser.Balance}$",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
            return;
        }

        #region SupportMethod

        private async Task<bool> UserIsOwner(ITelegramBotClient botClient, Update update) => await _applicationContext.Owners.AnyAsync(t => t.UserName == update.Message.From.Username);

        private async Task SendInstruction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, CommandData commandData)
        {
            await botClient.SendTextMessageAsync(
                   update.Message.Chat.Id,
                   $"{commandData.Command}\nВведите сумму и описание или отменить запрос!",
                   replyMarkup: CancelCommadMarkup,
                   replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
        }

        private async Task<bool> CheckMessageSignature(ITelegramBotClient botClient, Update update)
        {
            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId, disableNotification: true, messageThreadId: update.Message.MessageThreadId);
                return false;
            }
            return true;
        }

        private async Task ExecuteCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, string text)
        {

            Regex commandFromText = new Regex(@"(?<=(Команда: ))(.*)+?(?=(\n))", RegexOptions.IgnoreCase);
            var commandText = commandFromText.Match(text).Value;
            Console.WriteLine(commandText); // /add 4656 asdasd as

            var collection = CommadsRegex.Match(commandText).Groups;
            var commandData = new CommandData(collection);

            if (commandData.Command == "/dropdb")
            {
                _applicationContext.Owners.RemoveRange(_applicationContext.Owners);
                _applicationContext.BalanceEvents.RemoveRange(_applicationContext.BalanceEvents);
                await _applicationContext.SaveChangesAsync();
                return;
            }

            if (commandData.Command == null || commandData.Value == null)
            {
                Console.WriteLine($"Ошибка!\n{text}");
                return;
            }

            var owner = _applicationContext.Owners.FirstOrDefault(t => t.UserName == update.CallbackQuery.From.Username);
            if (owner == null)
            {
                Console.WriteLine($"Ошибка!\n{update.CallbackQuery.From.Username} не найден в бд");
                return;
            }
            var balanceEvent = new BalanceEvent()
            {
                DateTime = DateTime.Now,
                OwnerId = owner.Id,
                Value = commandData.Value.Value,
                IsAdded = false,
                OwnerBalanceId = -1,
            };
            if (commandData.Command.EndsWith("me"))
                balanceEvent.OwnerBalanceId = owner.Id;
            if (commandData.Command.EndsWith("to"))
            {
                var ownerid = await _applicationContext.Owners.FirstOrDefaultAsync(t => t.UserName == commandData.UserName);
                if (ownerid != null)
                    balanceEvent.OwnerBalanceId = ownerid.Id;
            }
            if (commandData.Command.StartsWith("/add"))
                balanceEvent.IsAdded = true;

            if (commandData.Description != null)
                balanceEvent.Description = commandData.Description;


            if (commandData.Command == "/changebalance")
            {
                var balanceNow = _applicationContext.Owners.Sum(t => t.Balance);
                var newBalance = commandData.Value.Value;
                var balanceChenge = newBalance - balanceNow;
                if (balanceChenge > 0)
                    balanceEvent.IsAdded = true;
                balanceEvent.Value = Math.Abs(balanceChenge);
                balanceEvent.Description = "Итог сметы";
            }

            if (balanceEvent.OwnerBalanceId > 0)
            {
                double changeValue = balanceEvent.Value;
                if (balanceEvent.IsAdded == false)
                    changeValue = changeValue * -1;

                var putOwner = await _applicationContext.Owners.FirstOrDefaultAsync(t => t.Id == balanceEvent.OwnerBalanceId);
                putOwner.Balance += changeValue;
                _applicationContext.Update(putOwner);
            }
            else
            {
                var owners = _applicationContext.Owners;
                double changeValue = balanceEvent.Value / owners.Count();
                if (balanceEvent.IsAdded == false)
                    changeValue = changeValue * -1;
                await owners.ForEachAsync(t => t.Balance += changeValue);
                _applicationContext.UpdateRange(owners);

            }
            await _applicationContext.BalanceEvents.AddAsync(balanceEvent);
            await _applicationContext.SaveChangesAsync();
        }

        #endregion
    }
}
