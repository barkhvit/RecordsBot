using RecordBot.Interfaces;
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
using System.Security.Cryptography;
using System.Globalization;
using RecordBot.Helpers;

namespace RecordBot.Commands
{
    public class CommandsForAppointments : Commands
    {
        private readonly IUserService _userService;
        public CommandsForAppointments(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService, IUserService userService)
            : base(telegramBotClient, appointmentService, procedureService)
        {
            _userService = userService;
        }

        //показать все записи пользователя CallBackDto("Appointment","ShowAll")
        public async Task ShowMyRecordsCommand(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            var user = await _userService.GetUser(userId, ct);

            var records = await _appointmentService.GetUserAppointments(user.Id, ct);
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
        public async Task AppointmentDetailShowCommand(Update update, CallBackDto appointmentCallBackDto, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.Id, ct);
          
            var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId,ct);

            if (update.Type == UpdateType.CallbackQuery) await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);

            await _telegramBotClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: $"{procedure.Name}\nДата: {appointment.dateTime.ToString("dd.MM.yyyy HH:mm")}\nСтоимость: {procedure.Price.ToString()} рублей.",
                cancellationToken: ct,
                replyMarkup: new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️ Назад ", new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_ShowAll).ToString()),
                    InlineKeyboardButton.WithCallbackData("❌ Отменить запись  ", new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_Cancel, appointmentCallBackDto.Id).ToString())
                });
           
        }

        //отправить пользователю сообщение с подтверждением об удалении записи
        internal async Task AskApprovedDeleteAppointments(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            CallBackDto appointmentCallBackDto = CallBackDto.FromString(text);
            if (appointmentCallBackDto.Id != null)
            {
                var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.Id, ct);
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
                        InlineKeyboardButton.WithCallbackData("Да", new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_Delete,appointment.Id).ToString()),
                        InlineKeyboardButton.WithCallbackData("Нет", new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_ShowAll).ToString())
                    } );
            }
        }

        //удаление записи и отправка сообщения
        internal async Task DeleteAppoinment(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);
            CallBackDto appointmentCallBackDto = CallBackDto.FromString(text);
            if (appointmentCallBackDto.Id != null)
            {
                var appointment = await _appointmentService.GetAppointmentById((Guid)appointmentCallBackDto.Id, ct);
                var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId, ct);
                var isCancel = await _appointmentService.CancelAppointment(appointment.Id, ct);
                if (isCancel)
                {
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await _telegramBotClient.EditMessageText(
                        messageId: messageId,
                        chatId: chatId,
                        text: "Запись удалена",
                        cancellationToken:ct,
                        replyMarkup: Keyboards.KeyBoardsForMainMenu.MainMenu());
                }
            }
        }

        //меню администрирования записей
        internal async Task ShowAdminMenu(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            List<InlineKeyboardButton[]> btn = new List<InlineKeyboardButton[]>
            {
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("👀 Посмотреть ",new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_ShowForAdminDates).ToString()),
                    InlineKeyboardButton.WithCallbackData("📝 Редактировать ",new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_EditAdmin).ToString())
                },
                new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("⬅️ Назад",new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString())
                }
            };

            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(chatId, messageId, "Выберите действие с записями:",
                cancellationToken: ct,
                replyMarkup: new InlineKeyboardMarkup(btn));
        }

        //выводим администратору список дат, где есть записи
        internal async Task ShowForAdminDates(Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            var appointments = await _appointmentService.GetActualyAppointments(ct);
            var dates = appointments.Select(a => DateOnly.FromDateTime(a.dateTime)).Distinct().ToList();
            string textMessage = "Записей нет.";
            //список кнопок делаем по три в ряд
            List<List<InlineKeyboardButton>> inlineKeyboardButtons = new List<List<InlineKeyboardButton>>();

            if (appointments != null && appointments.Count > 0)
            {
                textMessage = "Выберите дату:";
                //список кнопок 
                var allButtons = dates.Select(d => InlineKeyboardButton.WithCallbackData($"{d.ToString("dd.MM.yyyy")}",
                    new CallBackDto(Dto_Objects.Appointment, Dto_Action.App_ShowByDate, date:d).ToString())).ToList();

                for (int i = 0; i < allButtons.Count; i += 3)
                {
                    List<InlineKeyboardButton> row = allButtons.Skip(i).Take(3).ToList();
                    inlineKeyboardButtons.Add(row);
                }
                
            }
            inlineKeyboardButtons.Add(new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData("🏠 Главное меню",new CallBackDto(Dto_Objects.MainMenu,Dto_Action.MM_Show).ToString()),
                InlineKeyboardButton.WithCallbackData("Меню администратора",new CallBackDto(Dto_Objects.AdminMenu,Dto_Action.AM_Show).ToString())
            });
            InlineKeyboardMarkup inlineKeyboardMarkup = new InlineKeyboardMarkup(inlineKeyboardButtons);
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await _telegramBotClient.EditMessageText(chatId, messageId, textMessage, cancellationToken: ct,
                replyMarkup: inlineKeyboardMarkup);
        }

        internal async Task ShowAppointmentsByDate(Update update, CancellationToken cancellationToken)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = GetMessageInfo(update);

            var dto = CallBackDto.FromString(text);

            if(dto.Date!=null)
            {
                var date = (DateOnly)dto.Date;
                var appointments = await _appointmentService.GetAppointmentsByDate(date, cancellationToken);
                string textMessage = "";
                if (appointments == null)
                {
                    textMessage = $"На {date.ToString("dd.MM.yyyy")} нет записей";
                }
                else
                {
                    textMessage = $"Записи на {date.ToString("dd.MM.yyyy")}:";
                    foreach(var a in appointments)
                    {
                        var procedure = await _procedureService.GetProcedureByGuidId(a.ProcedureId, cancellationToken);
                        var user = await _userService.GetUserByUserId(a.UserId, cancellationToken);
                        var userName = await MessageInfo.GetUsernameByTelegramId(user.TelegramId, _telegramBotClient, cancellationToken);

                        string userLink = userName != null ? $"\n{TimeOnly.FromDateTime(a.dateTime)} - " +
                            $"{user.FirstName} {user.LastName} (<a href=\"tg://user?id={user.TelegramId}\">{userName}</a>) - {procedure.Name}":
                            $"\n{TimeOnly.FromDateTime(a.dateTime)} - {user.FirstName} {user.LastName} - {procedure.Name}"; 

                        textMessage += userLink;
                    }
                }
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                await _telegramBotClient.EditMessageText(chatId, messageId, textMessage, cancellationToken: cancellationToken,
                    replyMarkup:Keyboards.KeyBoardsForMainMenu.BackToMainMenu(),
                    parseMode: ParseMode.Html);
            }
        }
    }
}
