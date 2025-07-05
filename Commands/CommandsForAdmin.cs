using Microsoft.VisualBasic;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RecordBot.Commands
{
    public class CommandsForAdmin : Commands
    {
        public CommandsForAdmin(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService)
            : base(telegramBotClient, appointmentService, procedureService)
        {
        }

        //выбрали команду /admin, выводим меню админа
        public async Task AdminCommand(Update update, TypeQuery typeQuery, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            if (Admins.admins.Contains(userId))
            {
                if(typeQuery == TypeQuery.CallBackQuery)
                {
                    await _telegramBotClient.EditMessageText(
                    messageId: messageId,
                    chatId: chatId,
                    text: "Меню администратора:",
                    replyMarkup: Keyboards.KeyboardsForProcedures.GetAdminKeybord(),
                    cancellationToken: ct);
                }
                else
                {
                    await _telegramBotClient.SendMessage(
                    chatId: chatId,
                    text: "Выберите действие",
                    replyMarkup: Keyboards.KeyboardsForProcedures.GetAdminKeybord(),
                    cancellationToken: ct);
                }
                
            }
            else
            {
                await _telegramBotClient.SendMessage(chatId, "Вы не являетесь администратором", cancellationToken: ct);
            }
        }


    }
}
