using RecordBot.Helpers;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Scenarios
{
    public class AddProcedureScenario : IScenario
    {
        private readonly IProcedureService _procedureService;

        public AddProcedureScenario(IProcedureService procedureService)
        {
            _procedureService = procedureService;
        }

        public bool CanHandle(ScenarioType scenarioType) => ScenarioType.AddProcedure == scenarioType;

        public async Task<ScenarioResult> HandleScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);

            ScenarioResult scenarioResult = ScenarioResult.Transition;
            switch (context.CurrentStep)
            {
                case null:
                    await botClient.AnswerCallbackQuery(update.CallbackQuery.Id, cancellationToken: ct);
                    await botClient.EditMessageText(chatId, messageId, "Введите название услуги:",
                        replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"),
                        cancellationToken:ct);

                    context.CurrentStep = "Название";
                    scenarioResult = ScenarioResult.Transition;
                    break;
                case "Название":
                    if(text == "" || text == null)
                    {
                        await botClient.SendMessage(chatId, "Некорректное название, введите название процедуры:",
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"), cancellationToken: ct);
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Введите описание услуги", cancellationToken: ct,
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                        context.Data["Название"] = text;
                        context.CurrentStep = "Описание";
                        scenarioResult = ScenarioResult.Transition;
                        ;
                    }break;

                case "Описание":
                    if (text == "" || text == null)
                    {
                        await botClient.SendMessage(chatId, "Некорректное описание, введите описание процедуры:",
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"), cancellationToken: ct);
                    }
                    else
                    {
                        context.Data["Описание"] = text;
                        await botClient.SendMessage(chatId, "Введите стоимость процедуры:",
                            cancellationToken: ct, replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                        context.CurrentStep = "Стоимость";
                        scenarioResult = ScenarioResult.Transition;
                    }break;

                case "Стоимость":
                    if (Decimal.TryParse(text,out Decimal result))
                    {
                        context.Data["Стоимость"] = result;
                        await botClient.SendMessage(chatId, "Введите длительность процедуры с минутах:",
                            cancellationToken: ct, replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"));
                        context.CurrentStep = "Длительность";
                        scenarioResult = ScenarioResult.Transition;
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Некорректная стоимость, введите стоимость процедуры:",
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"), cancellationToken: ct);
                    }break;
                case "Длительность":
                    if(int.TryParse(text,out int durInt))
                    {
                        context.Data["Длительность"] = durInt;
                        var procedure = new Procedure(
                            name: (string)context.Data["Название"],
                            description: (string)context.Data["Описание"],
                            price: (decimal)context.Data["Стоимость"],
                            duration: (int)context.Data["Длительность"]);
                        context.Data["Процедура"] = procedure;
                        context.CurrentStep = "Завершение";
                        string mesText = $"Добавить процедуру:\nНазвание: {procedure.Name}\nОписание: {procedure.Description}\n" +
                            $"Стоимость: {procedure.Price}\nДлительность: {procedure.DurationMinutes}";
                        scenarioResult = ScenarioResult.Transition;

                        await botClient.SendMessage(chatId, mesText, cancellationToken: ct,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"),
                                    InlineKeyboardButton.WithCallbackData("Добавить","addProcedure")
                                }
                            }));
                    }
                    else
                    {
                        await botClient.SendMessage(chatId, "Некорректная длительность, введите длительность процедуры в минутах:",
                            replyMarkup: InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"), cancellationToken: ct);
                    }
                    break;
                case "Завершение":
                    if(text == "addProcedure")
                    {
                        var procedure = (Procedure)context.Data["Процедура"];
                        await _procedureService.AddProcedure(procedure, ct);
                        await botClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                        await botClient.SendMessage(chatId, "Процедура добавлена", cancellationToken: ct,
                            replyMarkup: Keyboards.KeyBoardsForMainMenu.BackToMainMenu());
                        scenarioResult = ScenarioResult.Completed;
                    }
                    else
                    {
                        var procedure = (Procedure)context.Data["Процедура"];
                        string mesText = $"Добавить процедуру:\nНазвание: {procedure.Name}\nОписание: {procedure.Description}\n" +
                            $"Стоимость: {procedure.Price}\nДлительность: {procedure.DurationMinutes}";
                        scenarioResult = ScenarioResult.Transition;

                        await botClient.SendMessage(chatId, mesText, cancellationToken: ct,
                            replyMarkup: new InlineKeyboardMarkup(new[]
                            {
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData("Отменить действие", "cancel"),
                                    InlineKeyboardButton.WithCallbackData("Добавить","addProcedure")
                                }
                            }));
                    }break;

                default: scenarioResult = ScenarioResult.Transition; break;
            }
            return scenarioResult;
        }
    }
}
