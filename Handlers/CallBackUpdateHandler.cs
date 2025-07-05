using RecordBot.Commands;
using RecordBot.Enums;
using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Scenario.InfoStorage;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using RecordBot.CallBackModels;
using RecordBot.Helpers;

namespace RecordBot.Handlers
{
    public class CallBackUpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userservice;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IFreePeriodService _freePeriodService;
        private readonly ICreateProcedureService _createProcedureService;
        private readonly IProcedureService _procedureService;
        private readonly IAppointmentService _appointmentService;
        private readonly InfoRepositoryService _infoRepositoryService;
        private readonly CommandsForAppointments _commandsForAppointments;
        private readonly CommandsForAdmin _commandsForAdmin;
        private readonly CommandsForFreePeriod _commandsForFreePeriod;
        private readonly CommandsForProcedures _commandsForProcedures;
        private readonly CommandsForMainMenu _commandsForMainMenu;

        public CallBackUpdateHandler(IUserService userservice, ITelegramBotClient telegramBotClient, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, ICreateProcedureService createProcedureService, IAppointmentService appointmentService, InfoRepositoryService infoRepositoryService)
        {
            _userservice = userservice;
            _freePeriodService = freePeriodService;
            _telegramBotClient = telegramBotClient;
            _procedureService = procedureService;
            _createProcedureService = createProcedureService;
            _appointmentService = appointmentService;
            _infoRepositoryService = infoRepositoryService;
            _commandsForAppointments = new CommandsForAppointments(_telegramBotClient, appointmentService, procedureService);
            _commandsForAdmin = new CommandsForAdmin(_telegramBotClient, appointmentService, procedureService);
            _commandsForFreePeriod = new CommandsForFreePeriod(_telegramBotClient, appointmentService, procedureService, _freePeriodService);
            _commandsForProcedures = new CommandsForProcedures(_telegramBotClient, appointmentService, procedureService);
            _commandsForMainMenu = new CommandsForMainMenu(_telegramBotClient, appointmentService, procedureService);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.CallbackQuery == null||update.CallbackQuery.Data == null || update.CallbackQuery.Message == null) throw new NullReferenceException();
            string[] _callBackQuery = update.CallbackQuery.Data.Split(':', 2);
            CallBackDto callBack = null;
            if (_callBackQuery.Length > 1)
            {
                callBack = CallBackDto.FromString(update.CallbackQuery.Data);

                switch (callBack.Object)
                {
                    case "MainMenu":
                        switch (callBack.Action)
                        {
                            case "Show": await _commandsForMainMenu.ShowMainMenu(update, cancellationToken); break;//показать главное меню
                        }
                        break;
                    case "FreePeriod":
                        var freePeriodCallBackDto = FreePeriodCallBackDto.FromString(update.CallbackQuery.Data);
                        switch (callBack.Action)
                        {
                            case "Admin": await _commandsForFreePeriod.Admin(update, cancellationToken); break;// admin - периоды == добавить, посмотреть, назад
                            case "Show": await _commandsForFreePeriod.ShowAllPeriodsCommand(update, cancellationToken); break;//администрирование FreePeriod
                        }
                        break;
                    case "AdminMenu":
                        switch (callBack.Action)
                        {
                            case "Show": await _commandsForAdmin.AdminCommand(update, TypeQuery.CallBackQuery, cancellationToken); break; //admin == Услуги, Периоды
                        }break;

                    case "Appointment":
                        var appointmentCallBackDto = AppointmentCallBackDto.FromString(update.CallbackQuery.Data);
                        switch (callBack.Action)
                        {
                            case "Cancel": await _commandsForAppointments.AskApprovedDeleteAppointments(update, cancellationToken); break; //запросить подтверждение удаления записи
                            case "ShowAll": await _commandsForAppointments.ShowMyRecordsCommand(update, cancellationToken); break;//показать все записи пользователя
                            case "Delete": await _commandsForAppointments.DeleteAppoinment(update, cancellationToken); break;//удалить запись
                            case "Show": await _commandsForAppointments.AppointmentDetailShowCommand(update, appointmentCallBackDto, cancellationToken); break; //показать детально запись c кнопкой отмена
                        }
                        break;
                    case "Date": var dateCallBackDto = DateCallBackDto.FromString(update.CallbackQuery.Data);
                        switch (callBack.Action)
                        {
                            case "Show": await _commandsForFreePeriod.ShowDateCommand(update, dateCallBackDto, cancellationToken); break; //показать дату для выбора даты работы
                            case "Select": await _commandsForFreePeriod.AddDateCommand(update, dateCallBackDto, cancellationToken); break; //добавить дату
                        }
                        break;
                    case "Procedure": var procedureCallBackDto = ProcedureCallBackDto.FromString(update.CallbackQuery.Data);
                        switch (callBack.Action)
                        {
                            case "Admin": await _commandsForProcedures.AdminProcedureCommand(update, cancellationToken); break; //администрирование процедур
                            case "ShowAllActiveForAdmin": await _commandsForProcedures.ShowActiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.admin); break;//показать все процедуры для АДМИНА
                            case "ShowAllActiveForUser": await _commandsForProcedures.ShowActiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.reserved); break;//показать все процедуры для ЮЗЕРА
                            case "ShowAllArchiveForAdmin": await _commandsForProcedures.ShowArchiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.admin); break;//показать все процедуры 
                            case "SDFA": await _commandsForProcedures.ShowProcedureCommand(procedureCallBackDto, update, cancellationToken, ReasonShowProcedure.admin); break; //показать информацию о процедуре для АДМИНА (ShowDetailForAdmin)
                            case "SDFU": await _commandsForProcedures.ShowProcedureCommand(procedureCallBackDto, update, cancellationToken, ReasonShowProcedure.reserved); break; //показать информацию о процедуре для ЗАПИСАТЬСЯ (ShowDetailForUser)
                            case "ChangeActive": await _commandsForProcedures.ChangeIsActiveProcedureCommand(update, procedureCallBackDto, cancellationToken); break;//поменять активность процедуры
                        }
                        break;
                }
            }

            
            switch (_callBackQuery[0])
            {

               
                //--------- ЗАПИСЬ НА ПРОЦЕДУРУ:
                //case "showprocedureForReserved": await _commandsForProcedures.ShowProcedureCommand(procedureCallBackDto, cancellationToken, ReasonShowProcedure.reserved); break; //показать информацию о процедуре для ЗАПИСАТЬСЯ
                case "reservedOnProcedure": await ReservedOnProcedureCommand_Date(update, _callBackQuery, cancellationToken); break; //пользователь выбрал процедуру и нажал Записаться
                case "DateForReserved": await ReservedOnProcedureCommand_Time(update, _callBackQuery, cancellationToken); break;//пользователь выбрал дату для резервирования
                case "TimeForReserved": await ReservedOnProcedureCommand_Finish(update, _callBackQuery, cancellationToken); break; //пользователь выбрал время для резервирования
                case "reservedOnProcedureDone": await ReservedOnProcedureCommand_Done(update, _callBackQuery, cancellationToken); break;//пользователь подтвердил создание записи
            }
        }

        
        

        private async Task ReservedOnProcedureCommand_Done(Update update, string[] callBackQuery, CancellationToken ct)
        {
            //получаем процедуру, Дату и Время из хранилища
            var context = await _infoRepositoryService.GetOrCreateContext(update.CallbackQuery.From.Id, ct);
            var procedureId = (Guid)context.Data["процедура"];
            var procedure = await _procedureService.GetProcedureByGuidId(procedureId, ct);
            var dateForReserved = (DateOnly)context.Data["дата процедуры"];
            var timeForReserved = (TimeOnly)context.Data["время процедуры"];

            //создаем Appointment CreateAppointment
            Appointment? appointment = await _appointmentService.CreateAppointment(update.CallbackQuery.From.Id, procedure, new DateTime(dateForReserved,timeForReserved), ct);

            //если процедура создалась, то пишем что ОК, если нет, то не ок
            string textMessage = appointment == null ? "ОШИБКА!!!" : "Вы записаны";
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(
                chatId: update.CallbackQuery.Message.Chat.Id,
                messageId: update.CallbackQuery.Message.MessageId,
                text: textMessage,
                cancellationToken: ct);
        }

        //пользователь выбрал процедуру, нажал кнопку ЗАПИСАТЬСЯ и выбрал определенную дату и время
        private async Task ReservedOnProcedureCommand_Finish(Update update, string[] callBackQuery, CancellationToken ct)
        {
            // получаем из callBackQuery время бронирования
            if(TimeOnly.TryParse(callBackQuery[1],out TimeOnly timeForReserved))
            {
                //получаем из временного хранилища процедуру, дату
                var context = await _infoRepositoryService.GetOrCreateContext(update.CallbackQuery.From.Id, ct);
                var procedureId = (Guid)context.Data["процедура"];
                var procedure = await _procedureService.GetProcedureByGuidId(procedureId, ct);
                var dateForReserved = (DateOnly)context.Data["дата процедуры"];

                //время записываем во временное хранилище
                await _infoRepositoryService.AddProcedureTimeInfo(update.CallbackQuery.From.Id, timeForReserved, ct);

                //текстовое сообщение пользователю
                string textMessage = $"Проверьте и нажмите ПОДТВЕРДИТЬ.\n\n" +
                    $"Услуга:{procedure.Name}\nДата и время: {dateForReserved.ToString("dd.MM.yyyy")} {timeForReserved.ToString("HH:mm")}";

                //клавиатура: отмена и ПОДТВЕРДИТЬ
                InlineKeyboardMarkup inlineKeyboardMarkup = Keyboards.Keyboards.GetKeybordConfirmReserved();
                //запрашиваем у пользователя подтверждение
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                await _telegramBotClient.EditMessageText(
                    messageId: update.CallbackQuery.Message.MessageId,
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: textMessage,
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: ct);
            }
        }

        //пользователь выбрал процедуру, нажал кнопку ЗАПИСАТЬСЯ и выбрал определенную дату
        private async Task ReservedOnProcedureCommand_Time(Update update, string[] callBackQuery, CancellationToken cancellationToken)
        {
            // получаем из ответа дату и записываем ее во временное хранилище
            if(DateOnly.TryParse(callBackQuery[1],out DateOnly dateForReserve))
            {
                //получаем из временного хранилища процедуру и сохраняем дату во временное хранилище
                var context = await _infoRepositoryService.GetOrCreateContext(update.CallbackQuery.From.Id, cancellationToken);
                await _infoRepositoryService.AddProcedureDateInfo(update.CallbackQuery.From.Id, dateForReserve, cancellationToken);
                var procedureId = (Guid)context.Data["процедура"];
                var procedure = await _procedureService.GetProcedureByGuidId(procedureId, cancellationToken);

                //получаем доступные DateTime для резервирования
                var dateTimeForReserve = await _freePeriodService.GetDateTimeForReserved(procedure, cancellationToken);

                //выбираем TimeOnly по дате бронирования
                var timesForReserve = dateTimeForReserve.Where(d => DateOnly.FromDateTime(d) == dateForReserve).
                    Select(d => TimeOnly.FromDateTime(d)).ToList();
                InlineKeyboardMarkup inlineKeyboardMarkup = Keyboards.Keyboards.GetKeybordTimes(timesForReserve, ReasonShowTimes.ForReservedProcedures, procedureId);

                //отправляем пользователю сообщение с выбором времени для бронирования
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                await _telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: $"{procedure.Name} на {dateForReserve.ToString("dd.MM.yyyy")}. Доступное время:",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
            
        }


        //пользователь выбрал процедуру и нажал кнопку ЗАПИСАТЬСЯ
        private async Task ReservedOnProcedureCommand_Date(Update update, string[] callBackQuery, CancellationToken cancellationToken)
        {
            //получаем ID процедуры и саму процедуру
            Procedure? procedure;
            if(Guid.TryParse(callBackQuery[1], out Guid ProcedureId))
            {  
                // и саму процедуру
                procedure = await _procedureService.GetProcedureByGuidId(ProcedureId, cancellationToken);

                //записываем ID процедуры во временное хранилище
                await _infoRepositoryService.AddProcedureIdInfo(update.CallbackQuery.From.Id, ProcedureId, cancellationToken);

                //получаем доступные DateTime для резервирования
                var dateTimeForReserve = await _freePeriodService.GetDateTimeForReserved(procedure, cancellationToken);

                //отправляем пользователю доступные даты для резервирования
                var datesForReserve = dateTimeForReserve.Select(dt => DateOnly.FromDateTime(dt)).Distinct();
                InlineKeyboardMarkup inlineKeyboardMarkup = Keyboards.Keyboards.GetKeybordDates(datesForReserve, ReasonShowDates.ForReservedProcedures);

                //отправляем пользователю доступные даты для резервирования
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                await _telegramBotClient.EditMessageText(
                    chatId:update.CallbackQuery.Message.Chat.Id,
                    messageId: update.CallbackQuery.Message.MessageId,
                    text: "Выберите дату:",
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: cancellationToken);
            }
        }

        
        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
