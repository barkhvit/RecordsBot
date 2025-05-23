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
        public MessageUpdateHandler(IUserService userservice, ITelegramBotClient telegramBotClient, IFreePeriodService freePeriodService, 
            IProcedureService procedureService)
        {
            _telegramBotClient = telegramBotClient;
            _userservice = userservice;
            _freePeriodService = freePeriodService;
        }
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Message.Text)
                {
                    case "/start": await StartCommand(update, cancellationToken); break;
                    case "/admin": await AdminCommand(update, cancellationToken); break;
                    default: await botClient.SendMessage(update.Message.Chat.Id, "Выберите команду из меню", cancellationToken: cancellationToken); break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        

        private async Task AdminCommand(Update update, CancellationToken cancellationToken)
        {
            long TelegramUserId = update.Message.Chat.Id;
            if (TelegramUserId == AdminId)
            {
                await _telegramBotClient.SendMessage(
                chatId: update.Message.Chat.Id,
                text: "Выберите действие",
                replyMarkup: Keyboards.Keyboards.GetAdminKeybord(),
                cancellationToken: cancellationToken);
            }
            else
            {
                await _telegramBotClient.SendMessage(TelegramUserId, "Вы не являетесь администратором", cancellationToken: cancellationToken);
            }
        }

        private async Task StartCommand(Update update, CancellationToken cancellationToken)
        {
            var user = await _userservice.RegisterUser(update, cancellationToken);
            //главное меню бота
            await _telegramBotClient.SetMyCommands(
                commands: MenuCommandsService.mainMenu,
                scope: null,//для всех пользователей,
                languageCode:null);//язык по умолчанию
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        
    }
}
