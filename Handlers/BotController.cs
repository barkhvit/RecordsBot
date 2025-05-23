using RecordBot.Interfaces;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace RecordBot.Handlers
{
    public delegate void MessageEventHandler(long TelegramId, string FirstName, string message);
    public class BotController : IUpdateHandler
    {
        //обработчики апдейтов
        private readonly MessageUpdateHandler _messageUpdateHandler; //текстовые сообщения
        private readonly CallBackUpdateHandler _callBackUpdateHandler; //колбэки
        private readonly ReplyToMessageUpdateHandler _replyToMessageUpdateHandler; //текстовые ответы на сообщения 

        //события
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateComplete;

        public BotController(ITelegramBotClient botClient, IUserService userService, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, ICreateProcedureService createProcedureService)
        {
            _replyToMessageUpdateHandler = new ReplyToMessageUpdateHandler(botClient, procedureService, createProcedureService);
            _messageUpdateHandler = new MessageUpdateHandler(userService, botClient, freePeriodService, procedureService);
            _callBackUpdateHandler = new CallBackUpdateHandler(userService, botClient, freePeriodService, procedureService, createProcedureService);
        }

        //определяем тип апдейта и перенаправляем в нужный Handler
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        OnHandleUpdateStarted.Invoke(update.Message.From.Id, update.Message.From.FirstName, update.Message.Text);
                        //если текстовое сообщение - это ответ на другое собщение
                        if (update.Message.ReplyToMessage != null)
                        {
                            await _replyToMessageUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                            return;
                        }
                        await _messageUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                        OnHandleUpdateComplete.Invoke(update.Message.From.Id, update.Message.From.FirstName, update.Message.Text);
                        return;
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        await _callBackUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                        return;
                    default:
                        await botClient.SendMessage(update.Message.Chat.Id, "Неизвестная команда", cancellationToken: cancellationToken);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
        }   

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
