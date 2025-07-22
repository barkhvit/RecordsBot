using RecordBot.Commands;
using RecordBot.Enums;
using RecordBot.Interfaces;
using RecordBot.Models;
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
        private readonly IProcedureService _procedureService;
        private readonly IAppointmentService _appointmentService;
        private readonly CommandsForAppointments _commandsForAppointments;
        private readonly CommandsForAdmin _commandsForAdmin;
        private readonly CommandsForFreePeriod _commandsForFreePeriod;
        private readonly CommandsForProcedures _commandsForProcedures;
        private readonly CommandsForMainMenu _commandsForMainMenu;

        public CallBackUpdateHandler(IUserService userservice, ITelegramBotClient telegramBotClient, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, IAppointmentService appointmentService)
        {
            _userservice = userservice;
            _freePeriodService = freePeriodService;
            _telegramBotClient = telegramBotClient;
            _procedureService = procedureService;
            _appointmentService = appointmentService;
            _commandsForAppointments = new CommandsForAppointments(_telegramBotClient, appointmentService, procedureService, userservice);
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
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
