using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Handlers
{
    public class CallBackUpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userservice;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IFreePeriodService _freePeriodService;
        private readonly ICreateProcedureService _createProcedureService;
        private readonly IProcedureService _procedureService;

        public CallBackUpdateHandler(IUserService userservice, ITelegramBotClient telegramBotClient, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, ICreateProcedureService createProcedureService)
        {
            _userservice = userservice;
            _freePeriodService = freePeriodService;
            _telegramBotClient = telegramBotClient;
            _procedureService = procedureService;
            _createProcedureService = createProcedureService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery == null||update.CallbackQuery.Data == null || update.CallbackQuery.Message == null) throw new NullReferenceException();
            string[] _callBackQuery = update.CallbackQuery.Data.Split(':', 2);
            switch (_callBackQuery[0])
            {
                case "admindate_showdate": await ShowDateCommand(update, _callBackQuery[1], cancellationToken); break; //показать дату для выбора даты работы
                case "admindate_selectdate": await AddDateCommand(update, _callBackQuery[1], cancellationToken); break; //добавить дату
                case "admin": switch (_callBackQuery[1])
                    {
                        case "freeperiod": await ShowAllPeriodsCommand(update, cancellationToken); break;//администрирование FreePeriod
                        case "dates": await AdminDatesCommand(update, cancellationToken); break;//администрирование дат
                        case "procedures": await AdminProcedureCommand(update, cancellationToken); break; //администрирование процедур
                    }
                    break;
                case "adminprocedure_add": await StartCreateProcedureProcess(update,cancellationToken); break; //запуск сценария создания новой процедуры
                case "adminprocedure_addnew": await AddNewProcedure(update, cancellationToken); break;//добавление процедуры, завершение сценария добавления новой процедуры
                case "adminprocedure_deletenew": await DeleteNewProcedure(update, cancellationToken); break;
            }
        }

        //отмена добавления новой процедуры
        private async Task DeleteNewProcedure(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _createProcedureService.GetToStart(update.CallbackQuery.From.Id, cancellationToken);
            await _telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: "Добавление отменено",
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
        }

        //добавление процедуры, завершение процесса добавления новой процедуры
        private async Task AddNewProcedure(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            var procedure = await _createProcedureService.GetProcedure(update.CallbackQuery.From.Id, cancellationToken);
            if (procedure != null)
            {
                await _procedureService.AddProcedure(procedure, cancellationToken);
                await _createProcedureService.GetToStart(update.CallbackQuery.From.Id, cancellationToken);
                await _telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: "Услуга добавлена",
                    replyMarkup: null,
                    cancellationToken: cancellationToken);
            }
        }

        //администрирование процедур, нажатие на /admin -> Услуги
        private async Task AdminProcedureCommand(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _telegramBotClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Выберите действия с процедурами:",
                cancellationToken: cancellationToken,
                replyMarkup:Keyboards.Keyboards.GetProcedureAdminKeybord()
                );
        }

        //админ выбрал "Даты работы", сообщение "Администрирование дат для записей" и на выбор "добавить дату","посмотреть даты"
        private async Task AdminDatesCommand(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.SendMessage(
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Выберите действие",
                cancellationToken:cancellationToken,
                replyMarkup: Keyboards.Keyboards.GetDateAdminKeybord()
                );
        }

        //админ нажал на дату, добавляем период свободного времени по выбранной дате с 9 до 18, если дата еще не выбрана
        private async Task AddDateCommand(Update update, string callBackQuery, CancellationToken cancellationToken)
        {
            if(DateOnly.TryParse(callBackQuery, out var date))
            {
                FreePeriod freePeriod = new()
                {
                    FreePeriodId = Guid.NewGuid(),
                    Date = date,
                    StartTime = new TimeOnly(9, 0),
                    FinishTime = new TimeOnly(18, 0)
                };
                bool result = await _freePeriodService.Add(freePeriod, cancellationToken);
                if (result)
                {
                    await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Добавлена дата {date.ToString("dd.MM.yyyy")} для записи");
                }
                else
                {
                    await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Дата {date.ToString("dd.MM.yyyy")} уже добавлена");
                }
            }
        }

        //выбор даты "<--" "date" "-->"
        private async Task ShowDateCommand(Update update, string callBackQuery, CancellationToken cancellationToken)
        {
            if (DateOnly.TryParse(callBackQuery, out var date))
            {
                await _telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "Выберите дату для добавления:",
                    messageId:update.CallbackQuery.Message.Id,
                    cancellationToken:cancellationToken,
                    replyMarkup: Keyboards.Keyboards.GetInlineDate(date)
                    );
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            }
            
        }

        //показать все периоды, доступные для записей
        private async Task ShowAllPeriodsCommand(Update update, CancellationToken cancellationToken)
        {
            string text = "";
            var freePeriods = await _freePeriodService.GetAllPeriods(cancellationToken);
            foreach (FreePeriod freePeriod in freePeriods)
            {
                text = text + $"{freePeriod.Date}: {freePeriod.StartTime} - {freePeriod.FinishTime}, {freePeriod.Duration} мин.";
            }
            await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, text, cancellationToken: cancellationToken);
        }

        //запуск процесса создания новой услуги
        private async Task StartCreateProcedureProcess(Update update,CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _createProcedureService.GetToStart(update.CallbackQuery.Message.From.Id, cancellationToken);
            await _telegramBotClient.SendMessage( 
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "Создание услуги: введите название",
                    replyMarkup: new ForceReplyMarkup { Selective = true, InputFieldPlaceholder = "название услуги" },
                    cancellationToken: cancellationToken
                    );
            //обновить словари
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
