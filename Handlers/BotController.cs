using RecordBot.CallBackModels;
using RecordBot.Commands;
using RecordBot.Helpers;
using RecordBot.Interfaces;
using RecordBot.Keyboards;
using RecordBot.Models;
using RecordBot.Scenarios;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RecordBot.Handlers
{
    public delegate void MessageEventHandler(long TelegramId, string FirstName, string message);
    public class BotController : IUpdateHandler
    {
        private readonly ITelegramBotClient _botClient;

        //сценарии
        private readonly IScenarioContextRepository _scenarioContextRepository;
        private readonly IEnumerable<IScenario> _scenarios; //сценарии

        //обработчики апдейтов
        private readonly MessageUpdateHandler _messageUpdateHandler; //текстовые сообщения
        private readonly CallBackUpdateHandler _callBackUpdateHandler; //колбэки
        private readonly ReplyToMessageUpdateHandler _replyToMessageUpdateHandler; //текстовые ответы на сообщения 

        //сервисы
        private readonly IUserService _userService;

        //события
        public event MessageEventHandler? OnHandleUpdateStarted;
        public event MessageEventHandler? OnHandleUpdateComplete;

        public BotController(ITelegramBotClient botClient, IUserService userService, IFreePeriodService freePeriodService, 
            IProcedureService procedureService, IAppointmentService appointmentService, IEnumerable<IScenario> scenarios, IScenarioContextRepository scenarioContextRepository)
        {
            _botClient = botClient;
            _scenarios = scenarios;
            _scenarioContextRepository = scenarioContextRepository;
            _replyToMessageUpdateHandler = new ReplyToMessageUpdateHandler(botClient, procedureService);
            _messageUpdateHandler = new MessageUpdateHandler(userService, botClient, freePeriodService, procedureService, appointmentService);
            _callBackUpdateHandler = new CallBackUpdateHandler(userService, botClient, freePeriodService, procedureService, appointmentService);
            _userService = userService;
        }



        //определяем тип апдейта и перенаправляем в нужный Handler
        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, Text) = MessageInfo.GetMessageInfo(update);
            var user = await _userService.GetUser(userId, cancellationToken);
            OnHandleUpdateStarted.Invoke(chatId, user.FirstName, Text);

            try
            {
                

                //обработка Cancel - отмена сценария
                if(Text == "/cancel" || Text == "cancel")
                {
                    await _scenarioContextRepository.ResetContext(userId, cancellationToken);
                    await _botClient.AnswerCallbackQuery(update.CallbackQuery.Id);
                    await _botClient.EditMessageText(chatId,messageId, "Действие отменено.", cancellationToken: cancellationToken);
                    await _botClient.SendMessage(chatId, "Выберите действие:", cancellationToken: cancellationToken,
                        replyMarkup: KeyBoardsForMainMenu.MainMenu());
                    return;
                }

                //проверяем, есть ли у пользователя сценарий
                var context = await _scenarioContextRepository.GetContext(userId, cancellationToken);
                if (context != null)
                {
                    await ProcessScenario(context, update, cancellationToken);
                    return;
                }

                //ЗАПУСК СЦЕНАРИЕВ
                if (Text != null)
                {
                    switch (Text)
                    {
                        case "Procedure:Create":    //создание процедуры(услуги)
                            await SetNewContext(update, ScenarioType.AddProcedure, cancellationToken);
                            return;

                        case "FreePeriod:Create": //создание периода
                            await SetNewContext(update, ScenarioType.AddPeriod, cancellationToken);
                            return;

                        case "MessageToAdmin:Create"://сценарий "Написать администратору"
                            await SetNewContext(update, ScenarioType.SendMessageToAdmin, cancellationToken);
                            return;
                    }
                    //создание записи
                    if (Text == "Appointment:Create" || Text.Contains("Procedure:CreateAppointment"))
                    {
                        await SetNewContext(update, ScenarioType.AddAppointment, cancellationToken);
                        return;
                    }
                }
                

                //в зависимости от типа Update переключаем в нужный обработчик
                switch (update.Type)
                {
                    case Telegram.Bot.Types.Enums.UpdateType.Message:
                        //если текстовое сообщение - это ответ на другое собщение
                        if (update.Message.ReplyToMessage != null)
                        {
                            await _replyToMessageUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                            return;
                        }
                        await _messageUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                        return;
                    case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                        await _callBackUpdateHandler.HandleUpdateAsync(botClient, update, cancellationToken);
                        return;
                    default:
                        await botClient.SendMessage(update.Message.Chat.Id, "Неизвестная команда", cancellationToken: cancellationToken);
                        break;
                }


            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            finally
            {
                OnHandleUpdateComplete.Invoke(chatId, user.FirstName, Text);
            }
        }   

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private async Task SetNewContext(Update update, ScenarioType scenarioType, CancellationToken ct)
        {
            // Получаем данные из update с помощью pattern matching
            var (chatId, userId, messageId, Text) = MessageInfo.GetMessageInfo(update);
            //создаем новый контекст
            var newContext = new ScenarioContext(userId, scenarioType);
            await _scenarioContextRepository.SetContext(userId, newContext, ct);
            //запускаем сценарий
            await ProcessScenario(newContext, update, ct);
        }

        private IScenario GetScenario(ScenarioType scenario)
        {
            var handler = _scenarios.FirstOrDefault(s => s.CanHandle(scenario));
            return handler ?? throw new ArgumentException($"Нет сценария типа: {scenario}");
        }

        private async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            //получаем IScenario
            var scenario = GetScenario(context.CurrentScenario);
            var scenarioResult = await scenario.HandleScenarioAsync(_botClient, context, update, ct);
            if(scenarioResult == ScenarioResult.Completed)
            {
                await _scenarioContextRepository.ResetContext(context.UserId, ct);
            }
            else
            {
                await _scenarioContextRepository.SetContext(context.UserId, context, ct);
            }
        }
    }
}
