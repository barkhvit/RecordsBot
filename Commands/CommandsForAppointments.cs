using RecordBot.Interfaces;
using RecordBot.Scenario.InfoStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using RecordBot.CallBackModels;
using RecordBot.Models;

namespace RecordBot.Commands
{
    public class CommandsForAppointments : Commands
    {
        public CommandsForAppointments(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService)
            : base(telegramBotClient, appointmentService, procedureService)
        {
        }

        //показать все записи пользователя CallBackDto("Appointment","ShowAll")
        public async Task ShowMyRecordsCommand(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            var records = await _appointmentService.GetUserAppointments(userId, ct);
            string textMessage = records == null || records.Count == 0 ? "У вас нет записей." : "Ваши записи:";
            InlineKeyboardMarkup? inlineKeyboardMarkup = Keyboards.KeyboardsForMyRecords.GetShowMyRecordsKeybord(records);

            if(update.Type == UpdateType.CallbackQuery)
            {
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                await _telegramBotClient.EditMessageText(
                    messageId:messageId,
                    chatId: chatId,
                    text: textMessage,
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: ct);
            }
            else if(update.Type == UpdateType.Message)
            {
                await _telegramBotClient.SendMessage(
                    chatId: chatId,
                    text: textMessage,
                    replyMarkup: inlineKeyboardMarkup,
                    cancellationToken: ct);
            }
                
        }

        //показать детально конкретную запись пользователя, вывести кнопки НАЗАД(все записи) и ОТМЕНИТЬ ЗАПИСЬ
        public async Task AppointmentDetailShowCommand(Update update, AppointmentCallBackDto appointmentCallBackDto, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.AppointmentId, ct);
          
            var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId,ct);

            AppointmentCallBackDto appointmentCallBackDto1 = new AppointmentCallBackDto("Appointment", "Cancel", appointment.Id );

            if (update.Type == UpdateType.CallbackQuery) await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);

            await _telegramBotClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: $"{procedure.Name}\nДата: {appointment.dateTime.ToString("dd.MM.yyyy HH:mm")}\nСтоимость: {procedure.Price.ToString()} рублей.",
                cancellationToken: ct,
                replyMarkup: new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️ Назад ", new CallBackDto("Appointment","ShowAll").ToString()),
                    InlineKeyboardButton.WithCallbackData("❌ Отменить запись  ", new AppointmentCallBackDto("Appointment", "Cancel", appointmentCallBackDto.AppointmentId).ToString())
                });
           
        }

        //отправить пользователю сообщение с подтверждением об удалении записи
        internal async Task AskApprovedDeleteAppointments(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            AppointmentCallBackDto appointmentCallBackDto = AppointmentCallBackDto.FromString(text);
            if (appointmentCallBackDto.AppointmentId != null)
            {
                var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.AppointmentId, ct);
                var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId, ct);
                string mesText = $"🚨 Вы действительно хотите отменить запись на {appointment.dateTime.ToString("hh.MM.yyyy HH:mm")}?";
                if (update.Type == UpdateType.CallbackQuery) await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                await _telegramBotClient.EditMessageText(
                    chatId: chatId,
                    messageId:messageId,
                    text: mesText,
                    cancellationToken: ct,
                    replyMarkup: new InlineKeyboardButton[]
                    {
                        InlineKeyboardButton.WithCallbackData("Да", new AppointmentCallBackDto("Appointment","Delete",appointment.Id).ToString()),
                        InlineKeyboardButton.WithCallbackData("Нет", new CallBackDto("Appointment","ShowAll").ToString())
                    } );
            }
        }

        //удаление записи и отправка сообщения
        internal async Task DeleteAppoinment(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            AppointmentCallBackDto appointmentCallBackDto = AppointmentCallBackDto.FromString(text);
            if (appointmentCallBackDto.AppointmentId != null)
            {
                var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.AppointmentId, ct);
                var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId, ct);
                var isCancel = await _appointmentService.CancelAppointment(appointment.Id, ct);
                if (isCancel)
                {
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await _telegramBotClient.EditMessageText(
                        messageId: messageId,
                        chatId: chatId,
                        text: "Запись удалена",
                        cancellationToken:ct);
                }
            }
        }
    }
}
