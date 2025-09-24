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
                    case nameof(Dto_Objects.MainMenu):
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.MM_Show): await _commandsForMainMenu.ShowMainMenu(update, cancellationToken); break;//показать главное меню
                        }
                        break;

                    case nameof(Dto_Objects.FreePeriod):
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.FP_Admin): await _commandsForFreePeriod.Admin(update, cancellationToken); break;// admin - периоды == добавить, посмотреть, назад
                            case nameof(Dto_Action.FP_Show): await _commandsForFreePeriod.ShowAllPeriodsCommand(update, cancellationToken); break;//администрирование FreePeriod
                        }
                        break;

                    case nameof(Dto_Objects.AdminMenu):
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.AM_Show): await _commandsForAdmin.AdminCommand(update, TypeQuery.CallBackQuery, cancellationToken); break; //admin == Услуги, Периоды
                        }break;

                    case nameof(Dto_Objects.Appointment):
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.App_Cancel): await _commandsForAppointments.AskApprovedDeleteAppointments(update, cancellationToken); break; //запросить подтверждение удаления записи
                            case nameof(Dto_Action.App_ShowAll): await _commandsForAppointments.ShowMyRecordsCommand(update, cancellationToken); break;//показать все записи пользователя
                            case nameof(Dto_Action.App_Delete): await _commandsForAppointments.DeleteAppoinment(update, cancellationToken); break;//удалить запись
                            case nameof(Dto_Action.App_Show): await _commandsForAppointments.AppointmentDetailShowCommand(update, callBack, cancellationToken); break; //показать детально запись c кнопкой отмена
                            case nameof(Dto_Action.App_ShowAdminMenu): await _commandsForAppointments.ShowAdminMenu(update, cancellationToken); break;//администрирование записей
                            case nameof(Dto_Action.App_ShowForAdminDates): await _commandsForAppointments.ShowForAdminDates(update, cancellationToken); break; //показать даты с акт записями
                            case nameof(Dto_Action.App_ShowByDate): await _commandsForAppointments.ShowAppointmentsByDate(update, cancellationToken); break;
                            case nameof(Dto_Action.App_EditAdmin): break;
                        }
                        break;

                    case nameof(Dto_Objects.Date): 
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.Date_Show): await _commandsForFreePeriod.ShowDateCommand(update, callBack, cancellationToken); break; //показать дату для выбора даты работы
                            case nameof(Dto_Action.Date_Select): await _commandsForFreePeriod.AddDateCommand(update, callBack, cancellationToken); break; //добавить дату
                        }
                        break;

                    case nameof(Dto_Objects.Proc): 
                        switch (callBack.Action)
                        {
                            case nameof(Dto_Action.Proc_Admin): await _commandsForProcedures.AdminProcedureCommand(update, cancellationToken); break; //администрирование процедур
                            case nameof(Dto_Action.Proc_ShowAllActiveForAdmin): await _commandsForProcedures.ShowActiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.admin); break;//показать все процедуры для АДМИНА
                            case nameof(Dto_Action.Proc_ShowAllActiveForUser): await _commandsForProcedures.ShowActiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.reserved); break;//показать все процедуры для ЮЗЕРА
                            case nameof(Dto_Action.Proc_ShowAllArchiveForAdmin): await _commandsForProcedures.ShowArchiveProceduresCommand(update, cancellationToken, ReasonShowProcedure.admin); break;//показать все процедуры 
                            case nameof(Dto_Action.Proc_SA): await _commandsForProcedures.ShowProcedureCommand(callBack, update, cancellationToken, ReasonShowProcedure.admin); break; //показать информацию о процедуре для АДМИНА (ShowDetailForAdmin)
                            case nameof(Dto_Action.Proc_SU): await _commandsForProcedures.ShowProcedureCommand(callBack, update, cancellationToken, ReasonShowProcedure.reserved); break; //показать информацию о процедуре для ЗАПИСАТЬСЯ (ShowDetailForUser)
                            case nameof(Dto_Action.Proc_ChangeActive): await _commandsForProcedures.ChangeIsActiveProcedureCommand(update, callBack, cancellationToken); break;//поменять активность процедуры
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
