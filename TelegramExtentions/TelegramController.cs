using AccountingTer.Models;
using AccountingTer.Services;
using bybit.net.api.ApiServiceImp;
using bybit.net.api.Models.Account;
using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using CryptoExchange.Net.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
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
        private readonly IBybitRestClient _bybitRestClient;
        private readonly ApplicationContext _applicationContext;

        private readonly Regex CommadsRegex = new Regex(@"^(\/[a-zA-Z]*)@?\S* ?(\@\S*)? ?(\d*)? ?(.*)", RegexOptions.IgnoreCase);

        #region Markups

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

        public IReplyMarkup ConfirmCancelCommandMarkup
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

        #endregion
        
        public TelegramController(IOptions<TelegramOptions> options, ApplicationContext applicationContext, IBybitRestClient client)
        {
            _telegramOptions = options.Value;
            _applicationContext = applicationContext;
            _bybitRestClient = client;
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
                    replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
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
                replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
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
                replyToMessageId: update.Message.MessageId);
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
                    replyToMessageId: update.Message.MessageId);
            }
        }
        
        #endregion

        [MessageMethod("/register", UpdateType.Message)]
        public async Task Register(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId);
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

            if (_applicationContext.Owners.Any(t => t.TelegramLogin == userName))
            {
                await botClient.SendTextMessageAsync(
               update.Message.Chat.Id,
               "Такой пользователь уже зарегистрирован",
               replyToMessageId: update.Message.MessageId);
                return;
            }
            newUser.TelegramLogin = userName;
            if (commandData.Value != null)
            {
                addedBalanceEvent.Value = commandData.Value.Value;
                newUser.Balance = addedBalanceEvent.Value;
            }
            if (commandData.Description != null)
            {
                newUser.Description = commandData.Description;
            }
            newUser.Description = commandData.Description != null ? commandData.Description : newUser.TelegramLogin;

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
                $"Новый пользователь добавлен!\n@{newUser.TelegramLogin}({newUser.Description}) - {newUser.Balance}$",
                replyToMessageId: update.Message.MessageId);
            return;
        }

        #region Stats

        [MessageMethod("/balance", UpdateType.Message)] // needTest
        public async Task Balance(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var owners = _applicationContext.Owners;
            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                string.Join(' ', owners.Select(t => $"Баланс пользователя {t.TelegramLogin}({t.Description}): {t.Balance}$\n")) + $"Общий баланс: {owners.Sum(t => t.Balance)}$",
                replyToMessageId: update.Message.MessageId);
        }

        [MessageMethod("/balancebybit", UpdateType.Message)]
        public async Task BalanceBybit(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {

            var res = await _bybitRestClient.V5Api.Account.GetAllAssetBalancesAsync(Bybit.Net.Enums.AccountType.Fund);

            if (res.Success == false)
            {
                await botClient.SendTextMessageAsync(
                 update.Message.Chat.Id,
                 $"Ошибка запроса",
                 replyToMessageId: update.Message.MessageId);
            }


            string data = string.Join(" | ", res.Data.Balances.Select(t => $"{t.Asset} - {t.WalletBalance}"));

            await botClient.SendTextMessageAsync(
                 update.Message.Chat.Id,
                 $"{data}",
                 replyToMessageId: update.Message.MessageId);
        }

        [MessageMethod("/stats", UpdateType.Message)] // needTest
        public async Task Statistics(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var owners = _applicationContext.Owners;
            var balanceEventsToday = from balanceEvents in _applicationContext.BalanceEvents
                                     where balanceEvents.DateTime.Date == DateTime.Today
                                     join owner in _applicationContext.Owners on balanceEvents.OwnerId equals owner.Id
                                     select new { balanceEvents, owner };

            var plusTransaction = balanceEventsToday.Where(t => t.balanceEvents.IsAdded == true);
            var minusTransaction = balanceEventsToday.Where(t => t.balanceEvents.IsAdded == false);

            var plusToday = plusTransaction.Sum(t => t.balanceEvents.Value);
            var minusToday = minusTransaction.Sum(t => t.balanceEvents.Value);

            var balanceNow = _applicationContext.Owners.Sum(t => t.Balance);
            var todayResult = plusToday - minusToday;
            var balanceTomorrow = balanceNow - plusToday + minusToday;

            var anyoneChange = (int)((plusTransaction.Where(t => t.balanceEvents.OwnerBalanceId < 0).Sum(t => t.balanceEvents.Value) - minusTransaction.Where(t => t.balanceEvents.OwnerBalanceId < 0).Sum(t => t.balanceEvents.Value)) / 2);

            var ownerStatistic = owners.Select(o => new { UserName = o.TelegramLogin, Value = (plusTransaction.Where(t => t.balanceEvents.OwnerBalanceId == o.Id).Where(t => t.balanceEvents.OwnerBalanceId > 0).Sum(t => t.balanceEvents.Value) - minusTransaction.Where(t => t.balanceEvents.OwnerBalanceId == o.Id).Where(t => t.balanceEvents.OwnerBalanceId > 0).Sum(t => t.balanceEvents.Value) + anyoneChange) });

            await botClient.SendTextMessageAsync(update.Message.Chat.Id,
                $"Статистика на {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}:\n\n" +
                string.Join(string.Empty, balanceEventsToday
                .Select(t => $"{t.balanceEvents.DateTime.ToString("HH:mm")} | {(t.balanceEvents.IsAdded == true ? "+" : "-")}{t.balanceEvents.Value}$ {(owners.Any(o => o.Id == t.balanceEvents.OwnerBalanceId) ? $"со своего счета @{owners.First(o => o.Id == t.balanceEvents.OwnerBalanceId).TelegramLogin}" : "с общего счета")} Вызвал @{t.owner.TelegramLogin} - {t.balanceEvents.Description}\n")) +
                $"\nИтог:\n" +
                string.Join(" | ", ownerStatistic.Select(t => $"{t.UserName} : {(t.Value > 0 ? "+" : "")}{t.Value}$")) +
                $"\nНачало: {balanceTomorrow}$ | {(todayResult > 0 ? "+" : "")}{todayResult}$ | Сейчас: {balanceNow}$",
                replyToMessageId: update.Message.MessageId);
        }

        #endregion

        [MessageMethod("/changebalance", UpdateType.Message)] // needTest
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
              replyToMessageId: update.Message.MessageId);
            }
        }

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
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/withdraw"),
                            new KeyboardButton("/withdrawme"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/balance"),
                            new KeyboardButton("/stats")
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/info"),
                            new KeyboardButton("/hide"),
                        },
                        new KeyboardButton[]
                        {
                            new KeyboardButton("/changebalance"),
                        }
                    });
            replyKeyboard.ResizeKeyboard = true;

            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Показанны возможные комманды",
                replyMarkup: replyKeyboard);
        }

        [MessageMethod("/hide", UpdateType.Message)] // complite
        public async Task HideCommands(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => await botClient.SendTextMessageAsync(update.Message.Chat.Id, "Комманды скрыты", replyMarkup: new ReplyKeyboardRemove());

        [MessageMethod("/info", UpdateType.Message)] // complite
        public async Task Info(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                new List<InlineKeyboardButton[]>()
                {
                    new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithUrl("Сайт с данными", _telegramOptions.Url)
                    }
                });

            await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Комманды бота:\n" +
                "/start - показать меню команд\r\n" +
                "/hide - скрыть меню команд\r\n" +
                "/info - помощь\r\n" +
                "/add - добавить на общий счет \r\n" +
                "/addme - добавить только мне\r\n" +
                "/withdraw - снять с общего счета\r\n" +
                "/withdrawme - снять с моего счета\r\n" +
                "/balance - узнать баланс\r\n" +
                "/stats - подсчитать смету за сегодняшний день\n\n\n" +
                "Пример использования с указанием данных:\n/add 500 купили биток - распределить 500$ между двумя балансами поровну с описанием \"купили биток\"",
                replyMarkup: inlineKeyboard);
        }

        [MessageMethod("/dropdb", UpdateType.Message)]
        public async Task DropBD(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Message.From.Username != "Nesect" && false)
            {
                await botClient.SendTextMessageAsync(
                    update.Message.Chat.Id,
                    "Выполнять команды на изменение баланса могут только владельцы!",
                    replyToMessageId: update.Message.MessageId);
                return;
            }

            await botClient.SendTextMessageAsync(
                  update.Message.Chat.Id,
                  $"Команда: {update.Message.Text}\nДропнуть базу данных?",
                  replyMarkup: ConfirmCancelCommandMarkup,
                  replyToMessageId: update.Message.MessageId);
        }

        #endregion

        #region CallbackQuery

        [MessageMethod("cancelcommand", UpdateType.CallbackQuery)] // complite
        public async Task CancelCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) => await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, "Запрос был отменен. Данные не внесены!");

        [MessageMethod("confirm", UpdateType.CallbackQuery)] // complite
        public async Task ConfirmCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var callbackQuery = update.CallbackQuery;
            var replyMessageWithCommand = callbackQuery.Message.Text;
            var botNameData = await botClient.GetMyNameAsync();
            string botName = $"@{botNameData.Name}_bot";
            await ExecuteCommand(botClient, update, cancellationToken, replyMessageWithCommand.Replace(botName, ""));

            await botClient.EditMessageTextAsync(update.CallbackQuery.Message.Chat, update.CallbackQuery.Message.MessageId, "Запрос был выполнен. Данные внесены в базу данных");
        }
        #endregion

        public async Task UnknowCommand(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type == UpdateType.Message)
            {
                if (update.Message.ReplyToMessage != null)
                {
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
                        replyToMessageId: update.Message.MessageId);
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

        #region SupportMethod

        private async Task<bool> UserIsOwner(ITelegramBotClient botClient, Update update) => await _applicationContext.Owners.AnyAsync(t => t.TelegramLogin == update.Message.From.Username);

        private async Task SendInstruction(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken, CommandData commandData)
        {
            await botClient.SendTextMessageAsync(
                   update.Message.Chat.Id,
                   $"{commandData.Command}\nВведите сумму и описание или отменить запрос!",
                   replyMarkup: CancelCommadMarkup,
                   replyToMessageId: update.Message.MessageId);
        }

        private async Task<bool> CheckMessageSignature(ITelegramBotClient botClient, Update update)
        {
            if (CommadsRegex.IsMatch(update.Message.Text) == false)
            {
                await botClient.SendTextMessageAsync(
                update.Message.Chat.Id,
                "Ошибка при обработке команды",
                replyToMessageId: update.Message.MessageId);
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

            var owner = _applicationContext.Owners.FirstOrDefault(t => t.TelegramLogin == update.CallbackQuery.From.Username);
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
                var ownerid = await _applicationContext.Owners.FirstOrDefaultAsync(t => t.TelegramLogin == commandData.UserName);
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
                int changeValue = balanceEvent.Value;
                if (balanceEvent.IsAdded == false)
                    changeValue = changeValue * -1;

                var putOwner = await _applicationContext.Owners.FirstOrDefaultAsync(t => t.Id == balanceEvent.OwnerBalanceId);
                putOwner.Balance += changeValue;
                _applicationContext.Update(putOwner);
            }
            else
            {
                var owners = _applicationContext.Owners;
                int changeValue = (int)(balanceEvent.Value / owners.Count());
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
