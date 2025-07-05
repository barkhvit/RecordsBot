using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace RecordBot.Helpers
{
    public static class MessageInfo
    {
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
