using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RecordBot.Commands
{
    public class Commands
    {
        protected readonly ITelegramBotClient _telegramBotClient;
        protected readonly IAppointmentService _appointmentService;
        protected readonly IProcedureService _procedureService;

        public Commands(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService)
        {
            _telegramBotClient = telegramBotClient;
            _appointmentService = appointmentService;
            _procedureService = procedureService;
        }

        public static (long chatId, long userId, int messageId, string? text) GetMessageInfo(Update update)
        {
            return update switch
            {
                { Type: UpdateType.Message, Message: var msg }
                    => (msg.Chat.Id, msg.From.Id, msg.MessageId, msg.Text),

                { Type: UpdateType.CallbackQuery, CallbackQuery: var cbq }
                    => (cbq.Message.Chat.Id, cbq.From.Id, cbq.Message.MessageId, cbq.Data),

                _ => throw new InvalidOperationException("Неизвестный тип сообщения от пользователя")
            };
        }
    }
}
