using RecordBot.CallBackModels;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace RecordBot.Scenarios
{
    public class SendMessageToAdminScenario : IScenario
    {
        private readonly IUserService _userService;

        public SendMessageToAdminScenario(IUserService userService)
        {
            _userService = userService;
        }
        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.SendMessageToAdmin;
        }

        public async Task<ScenarioResult> HandleScenarioAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, text) = MessageInfo.GetMessageInfo(update);
            ScenarioResult scenarioResult = ScenarioResult.Transition;

            switch (context.CurrentStep)
            {
                case null:
                    InlineKeyboardButton[] buttons = new[]
                    {
                        InlineKeyboardButton.WithCallbackData("Отменить","cancel")
                    };
                    await botClient.EditMessageText(chatId, messageId, "Введите сообщение, которое вы хотите отправить администратору:",
                        cancellationToken:ct,
                        replyMarkup: new InlineKeyboardMarkup(buttons));
                    context.CurrentStep = "Message";
                    break;
                case "Message":
                    if (!string.IsNullOrEmpty(text))
                    {
                        var user = await _userService.GetUser(userId, ct);
                        var userName = MessageInfo.GetUserName(update);
                        long AdminId = Admins.admins.FirstOrDefault();
                        InlineKeyboardButton[] buttons1 = new[]
                        {
                            InlineKeyboardButton.WithCallbackData("✉️ Написать еще ","MessageToAdmin:Create"),
                            InlineKeyboardButton.WithCallbackData("🏠 Главное меню ",new CallBackDto("MainMenu","Show").ToString())
                        };
                        await botClient.SendMessage(AdminId, $"От пользователя {user.FirstName} {user.LastName}(<a href=\"tg://user?id={userId}\">{userName}</a>!) " +
                            $"поступило сообщение: {text}",
                            cancellationToken: ct,
                            parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                        await botClient.SendMessage(chatId, "Ваше сообщение отправлено.", 
                            cancellationToken: ct,
                            replyMarkup: new InlineKeyboardMarkup(buttons1));
                        scenarioResult = ScenarioResult.Completed;
                    }
                    break;
            }
            return scenarioResult;
        }
    }
}
