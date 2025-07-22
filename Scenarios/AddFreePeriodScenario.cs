using RecordBot.CallBackModels;
using RecordBot.DataAccess.Model;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using RecordBot.Keyboards;
using RecordBot.Models;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Scenarios
{
    public class AddFreePeriodScenario : IScenario
    {
        private readonly IFreePeriodService _freePeriodService;
        public AddFreePeriodScenario(IFreePeriodService freePeriodService)
        {
            _freePeriodService = freePeriodService;
        }

        public bool CanHandle(ScenarioType scenarioType) => scenarioType == ScenarioType.AddPeriod;

        public async Task<ScenarioResult> HandleScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);
            ScenarioResult scenarioResult = ScenarioResult.Transition;
            switch (context.CurrentStep)
            {
                case null:
                    await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await botClient.EditMessageText(chatId, messageId, "Введите дату в формате: дд.мм.гггг", cancellationToken:ct,
                        replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                    context.CurrentStep = "Дата";
                    scenarioResult = ScenarioResult.Transition;
                    break;

                case "Дата":
                    if(DateTime.TryParseExact(text, "dd.MM.yyyy",CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime date))
                    {
                        context.Data["Дата периода"] = DateOnly.FromDateTime(date);
                        await botClient.SendMessage(chatId, "Введите время начала периода в формате: ЧЧ:ММ",
                            cancellationToken:ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие","cancel"));
                        context.CurrentStep = "Начало периода";
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Некорректный ввод даты!\nВведите дату в формате: дд.мм.гггг", cancellationToken: ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                    }
                    scenarioResult = ScenarioResult.Transition;
                    break;

                case "Начало периода":
                    if(DateTime.TryParseExact(text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime time))
                    {
                        context.Data["Начало периода"] = TimeOnly.FromDateTime(time);
                        await botClient.SendMessage(chatId, "Введите время окончания периода в формате: ЧЧ:ММ",
                            cancellationToken: ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                        context.CurrentStep = "Окончание периода";
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Некорректный ввод времени!\nВведите время начала периода в формате: ЧЧ:ММ", cancellationToken: ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                    }
                    break;

                case "Окончание периода":
                    if (DateTime.TryParseExact(text, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime finishtime))
                    {
                        //проверка, что время окончания больше времени начала
                        if(TimeOnly.FromDateTime(finishtime) <= (TimeOnly)context.Data["Начало периода"])
                        {
                            await botClient.SendMessage(chatId, "Время окончания должно быть больше времени начала!\nВведите время окончания периода в формате: ЧЧ:ММ", cancellationToken: ct,
                                replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                            break;
                        }

                        context.Data["Окончание периода"] = TimeOnly.FromDateTime(finishtime);
                        var freePeriod = new FreePeriod()
                        {
                            FreePeriodId = Guid.NewGuid(),
                            Date = (DateOnly)context.Data["Дата периода"],
                            StartTime = (TimeOnly)context.Data["Начало периода"],
                            FinishTime = (TimeOnly)context.Data["Окончание периода"]
                        };

                        // проверка на пересечения периодов
                        var mergePeriod = await _freePeriodService.GetMergePeriod(freePeriod, ct);
                        
                        if (mergePeriod != null)
                        {

                            string mesText = $"Этот период пересекается с периодом:\n" +
                                $"{mergePeriod.Date.ToString("dd.MM.yyyy")} {mergePeriod.StartTime.ToString("HH:mm")}-{mergePeriod.FinishTime.ToString("HH:mm")}\n" +
                                $"Действие отменено.";

                            await botClient.SendMessage(chatId, mesText, cancellationToken: ct,
                                replyMarkup: KeyBoardsForMainMenu.BackToMainMenu());

                            scenarioResult = ScenarioResult.Completed;
                            break;
                        }

                        //если все ок, то добавляем новый период
                        await _freePeriodService.Add(freePeriod, ct);
                        string messText = $"Период:\n" +
                            $"{freePeriod.Date} {freePeriod.StartTime}:{freePeriod.FinishTime}\n" +
                            $"добавлен.";
                        await botClient.SendMessage(chatId, messText, cancellationToken: ct,
                            replyMarkup: KeyBoardsForMainMenu.BackToMainMenu());
                        scenarioResult = ScenarioResult.Completed;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Некорректный ввод времени!\nВведите время окончания периода в формате: ЧЧ:ММ", cancellationToken: ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                    }
                    break;
            }
            return scenarioResult;
        }
    }
}
