using RecordBot.CallBackModels;
using RecordBot.Interfaces;
using RecordBot.Models;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace RecordBot.Commands
{
    public class CommandsForFreePeriod : Commands
    {
        private readonly IFreePeriodService _freePeriodService;
        public CommandsForFreePeriod(ITelegramBotClient telegramBotClient, IAppointmentService appointmentService, IProcedureService procedureService, IFreePeriodService freePeriodService) 
            : base(telegramBotClient, appointmentService, procedureService)
        {
            _freePeriodService = freePeriodService;
        }

        internal async Task Admin(Update update, CancellationToken cancellationToken)
        {
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _telegramBotClient.EditMessageText(
                messageId: update.CallbackQuery.Message.MessageId,
                chatId: update.CallbackQuery.Message.Chat.Id,
                text: "Выберите действие",
                cancellationToken: cancellationToken,
                replyMarkup: Keyboards.Keyboards.GetDateAdminKeybord()
                );
        }

        //показать все периоды, доступные для записей
        public async Task ShowAllPeriodsCommand(Update update, CancellationToken cancellationToken)
        {
            string text = "";
            var freePeriods = await _freePeriodService.GetAllPeriods(cancellationToken);
            foreach (FreePeriod freePeriod in freePeriods)
            {
                text = text + $"{freePeriod.Date}: {freePeriod.StartTime} - {freePeriod.FinishTime}, {freePeriod.Duration} мин.";
            }
            await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, text, cancellationToken: cancellationToken);
        }

        //выбор даты "<--" "date" "-->"
        public async Task ShowDateCommand(Update update, DateCallBackDto dateCallBackDto, CancellationToken cancellationToken)
        {
            if (DateOnly.TryParse(dateCallBackDto.dateOnly.ToString(), out var date))
            {
                await _telegramBotClient.EditMessageText(
                    chatId: update.CallbackQuery.Message.Chat.Id,
                    text: "Выберите дату для добавления:",
                    messageId: update.CallbackQuery.Message.Id,
                    cancellationToken: cancellationToken,
                    replyMarkup: Keyboards.Keyboards.GetInlineDate(date)
                    );
                await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id);
            }

        }

        //админ нажал на дату, добавляем период свободного времени по выбранной дате с 9 до 18, если дата еще не выбрана
        public async Task AddDateCommand(Update update, DateCallBackDto dateCallBackDto, CancellationToken cancellationToken)
        {
            if (DateOnly.TryParse(dateCallBackDto.dateOnly.ToString(), out var date))
            {
                FreePeriod freePeriod = new()
                {
                    FreePeriodId = Guid.NewGuid(),
                    Date = date,
                    StartTime = new TimeOnly(9, 0),
                    FinishTime = new TimeOnly(18, 0)
                };
                bool result = await _freePeriodService.Add(freePeriod, cancellationToken);
                if (result)
                {
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                    await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Добавлена дата {date.ToString("dd.MM.yyyy")} для записи");
                }
                else
                {
                    await _telegramBotClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: cancellationToken);
                    await _telegramBotClient.SendMessage(update.CallbackQuery.Message.Chat.Id, $"Дата {date.ToString("dd.MM.yyyy")} уже добавлена");
                }
            }
        }

        
    }
}
