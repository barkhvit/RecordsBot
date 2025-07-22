using RecordBot.CallBackModels;
using RecordBot.Enums;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using RecordBot.Keyboards;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Scenarios
{
    public class AddAppointmentScenario : IScenario
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IProcedureService _procedureService;
        private readonly IFreePeriodService _freePeriodService;
        private readonly IUserService _userService;
        public AddAppointmentScenario(IAppointmentService appointmentService, IProcedureService procedureService, IFreePeriodService freePeriodService,
            IUserService userService)
        {
            _appointmentService = appointmentService;
            _procedureService = procedureService;
            _freePeriodService = freePeriodService;
            _userService = userService;
        }

        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.AddAppointment;
        }

        public async Task<ScenarioResult> HandleScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);
            ScenarioResult scenarioResult = ScenarioResult.Transition;
            string[] inputText = text.Split(':');
            CallBackDto callBackDto = new("", "");
            ProcedureCallBackDto procedureCallBackDto = new("", "", null);

            if (inputText.Length > 1)
            {
                callBackDto = CallBackDto.FromString(text);
                procedureCallBackDto = ProcedureCallBackDto.FromString(text);
            }
            
            
            switch (context.CurrentStep)
            {
                case null:
                    var user = await _userService.GetUser(userId, ct);
                    context.Data["UserId"] = user.Id;
                    //если процедура не выбрана
                    if (procedureCallBackDto.ProcedureId == null)
                    {
                        var procedures = await _procedureService.GetAllProcedures(ct); //все процедуры
                        //просим пользователя выбрать процедуру
                        await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken:ct);
                        await botClient.EditMessageText(chatId,messageId, "Выберите процедуру:",cancellationToken:ct,
                            replyMarkup: KeybordsForAppointments.GetAllProcedures(procedures,ReasonShowProcedure.reserved));
                        context.CurrentStep = "Процедура";
                        break;
                    }
                    //если уже выбрана процедура
                    context.Data["ProcedureId"] = procedureCallBackDto.ProcedureId;
                    var slots = await _appointmentService.GetSlotsForAppointment((Guid)procedureCallBackDto.ProcedureId, ct);
                    var dates = slots.Select(s => DateOnly.FromDateTime(s)).Distinct().ToList();
                    await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await botClient.EditMessageText(chatId,messageId, "Выберите дату", cancellationToken: ct,
                        replyMarkup: KeybordsForAppointments.GetKeybordDates(dates, ReasonShowDates.ForReservedProcedures));
                    context.CurrentStep = "Дата";

                    break;

                case "Процедура":
                    context.Data["ProcedureId"] = procedureCallBackDto.ProcedureId;
                    var slots1 = await _appointmentService.GetSlotsForAppointment((Guid)procedureCallBackDto.ProcedureId, ct);
                    var dates1 = slots1.Select(s => DateOnly.FromDateTime(s)).Distinct().ToList();
                    await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await botClient.EditMessageText(chatId,messageId, "Выберите дату", cancellationToken: ct,
                        replyMarkup: KeybordsForAppointments.GetKeybordDates(dates1, ReasonShowDates.ForReservedProcedures));
                    context.CurrentStep = "Дата";
                    break;

                case "Дата":
                    
                    //если дата выбрана корректно
                    if (DateOnly.TryParseExact(callBackDto.Action,"dd.MM.yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None,out var dateOnly))
                    {
                        context.Data["Дата"] = dateOnly;
                        var procedureId = (Guid)context.Data["ProcedureId"];
                        var procedure = await _procedureService.GetProcedureByGuidId(procedureId, ct);
                        var slots2 = await _appointmentService.GetSlotsForAppointment(procedureId, ct); //слоты для резервирования
                        var timesForReserved = slots2.Where(s => DateOnly.FromDateTime(s) == dateOnly).Select(s=>TimeOnly.FromDateTime(s)).ToList();
                        if (timesForReserved == null)
                        {
                            await botClient.SendMessage(chatId, "Выберите другую дату, на данную дату нет свободного времени");
                        }
                        await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                        await botClient.EditMessageText(chatId,messageId, "Выберите время", cancellationToken:ct,
                            replyMarkup: KeybordsForAppointments.GetKeybordTimes(timesForReserved,ReasonShowTimes.ForReservedProcedures, procedureId));
                        context.CurrentStep = "Время";
                        break;
                    }
                    await botClient.DeleteMessage(chatId, messageId, cancellationToken: ct);
                    break;

                case "Время":
                    //если время выбрано корректно
                    if(TimeOnly.TryParseExact(callBackDto.Action,"HH.mm",CultureInfo.InvariantCulture,DateTimeStyles.None, out var timeOnly))
                    {
                        await SetTime(botClient, update, context, timeOnly, ct);
                        context.CurrentStep = "Подтверждение";
                        break;
                    }
                    await botClient.DeleteMessage(chatId, messageId, cancellationToken: ct);
                    break;
                case "Подтверждение":
                    //если пользоватеь подтвердил
                    if(callBackDto.Action == "done")
                    {
                        await SetAppointment(botClient, update, context, ct);
                        scenarioResult = ScenarioResult.Completed;
                    }
                    break;
            }
            return scenarioResult;
        }

        private async Task SetTime(ITelegramBotClient botClient, Update update, ScenarioContext context, TimeOnly timeOnly, CancellationToken ct)
        {
            context.Data["Время"] = timeOnly;

            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);

            var procedure = await _procedureService.GetProcedureByGuidId((Guid)context.Data["ProcedureId"], ct);
            
            Appointment appointment = new Appointment()
            {
                Id = Guid.NewGuid(),
                dateTime = new DateTime((DateOnly)context.Data["Дата"], timeOnly),
                ProcedureId = (Guid)context.Data["ProcedureId"],
                UserId = (Guid)context.Data["UserId"],
                isConfirmed = false
            };
            context.Data["Запись"] = appointment;

            string mesText = $"Вы хотите записаться на процедуру:\n" +
                $"{procedure.Name}\n" +
                $"{appointment.dateTime.ToString("dd.MM.yyyy HH:mm")}";
            
            await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await botClient.EditMessageText(chatId,messageId, mesText, cancellationToken: ct,
                replyMarkup: new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("❌ Отмена ","cancel"),
                    InlineKeyboardButton.WithCallbackData("👍 Подтвердить ","procedure:done"),
                } );
        }
        private async Task SetAppointment(ITelegramBotClient botClient, Update update, ScenarioContext context, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);

            var appointment = (Appointment)context.Data["Запись"];
            await _appointmentService.Add(appointment, ct);
            await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
            await botClient.EditMessageText(chatId,messageId, "Вы записаны, информацию о записи можете посмотреть в Главном меню - Мои записи",
                cancellationToken: ct,
                replyMarkup: Keyboards.KeyBoardsForMainMenu.MainMenu());
        }
    }
}
