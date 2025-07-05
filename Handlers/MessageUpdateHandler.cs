using RecordBot.Commands;
using RecordBot.Enums;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Handlers
{
    public class MessageUpdateHandler : IUpdateHandler
    {
        const long AdminId = 1976535977;
        private readonly IUserService _userservice;
        private readonly IFreePeriodService _freePeriodService;
        private readonly ITelegramBotClient _telegramBotClient;
        private readonly IAppointmentService _appointmentService;
        private readonly IProcedureService _procedureService;
        private readonly CommandsForAppointments _commandsForAppointments;
        private readonly CommandsForAdmin _commandsForAdmin;
        private readonly CommandsForMainMenu _commandsForMainMenu;
        public MessageUpdateHandler(IUserService userservice, ITelegramBotClient telegramBotClient, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, IAppointmentService appointmentService)
        {
            _telegramBotClient = telegramBotClient;
            _userservice = userservice;
            _freePeriodService = freePeriodService;
            _appointmentService = appointmentService;
            _procedureService = procedureService;
            _commandsForAppointments = new CommandsForAppointments(_telegramBotClient, appointmentService, procedureService);
            _commandsForAdmin = new CommandsForAdmin(_telegramBotClient, appointmentService, procedureService);
            _commandsForMainMenu = new CommandsForMainMenu(_telegramBotClient, appointmentService, procedureService);
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Message.Text)
                {
                    case "/start": await StartCommand(update, cancellationToken); break;
                    case "/admin": await _commandsForAdmin.AdminCommand(update,TypeQuery.MessageQuery, cancellationToken); break;
                    case "/procedures": await ShowActiveProcedures(update, cancellationToken, ReasonShowProcedure.reserved); break;
                    case "/myrecords": await _commandsForAppointments.ShowMyRecordsCommand(update, cancellationToken);break;
                    default: await botClient.SendMessage(update.Message.Chat.Id, "Выберите команду из меню", cancellationToken: cancellationToken); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //показать активные процедуры
        private async Task ShowActiveProcedures(Update update, CancellationToken cancellationToken, ReasonShowProcedure reasonShowProcedure)
        {
            var procedures = await _procedureService.GetProceduresByActive(true, cancellationToken);
            await _telegramBotClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Выберите процедуру:",
                cancellationToken: cancellationToken,
                replyMarkup: Keyboards.KeyboardsForProcedures.GetAllProcedures(procedures, reasonShowProcedure));
        }

        

        private async Task StartCommand(Update update, CancellationToken cancellationToken)
        {
            var user = await _userservice.RegisterUser(update, cancellationToken);
            //главное меню бота
            await _telegramBotClient.SetMyCommands(
                commands: MenuCommandsService.mainMenu,
                scope: null,//для всех пользователей,
                languageCode:null);//язык по умолчанию

            await _commandsForMainMenu.ShowMainMenu(update, cancellationToken);
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        
    }
}
