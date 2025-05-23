using RecordBot.Handlers;
using RecordBot.Interfaces;
using RecordBot.Repository;
using RecordBot.Scenario;
using RecordBot.Services;
using Telegram.Bot;

namespace RecordBot
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            //бот-клиент @BarkhvitRecordsBot
            string token = Environment.GetEnvironmentVariable("TELEGRAM_RecordsBOT_TOKEN", EnvironmentVariableTarget.User);
            ITelegramBotClient botClient = new TelegramBotClient(token);

            //репозитории
            IUserRepository userRepository = new JsonUserRepository("users");
            IFreePeriodRepository freePeriodRepository = new JsonFreePeriodRepository("FreePeriods");
            IProcedureRepository procedureRepository = new JsonProcedureRepository("Procedures");

            //сервисы
            IUserService userService = new UserService(userRepository);
            IFreePeriodService freePeriodService = new FreePeriodService(freePeriodRepository);
            IProcedureService procedureService = new ProcedureService(procedureRepository);
            ICreateProcedureService createProcedureService = new CreateProcedureService(procedureService);

            //контроллер
            var botController = new BotController(botClient, userService, freePeriodService, procedureService, createProcedureService);

            //CancellationToken
            var _cts = new CancellationTokenSource();

            //подписываем на методы делегаты - события
            botController.OnHandleUpdateStarted += StartMessage;
            botController.OnHandleUpdateComplete += EndMessage;

            //прослушка бота
            try
            {
                botClient.StartReceiving(
                updateHandler: botController.HandleUpdateAsync,
                errorHandler: botController.HandleErrorAsync,
                cancellationToken: _cts.Token);

                Console.WriteLine("Бот запущен");

                await Task.Delay(-1, _cts.Token);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if(botController != null)
                {
                    //отписка событий от методов
                    botController.OnHandleUpdateStarted -= StartMessage;
                    botController.OnHandleUpdateComplete -= EndMessage;
                }
            }

            void StartMessage(long TelegramId, string FirstName, string message)
            {
                Console.WriteLine($"Началась обработка сообщения \"{message}\" от {FirstName}({TelegramId})");
            }
            void EndMessage(long TelegramId, string FirstName, string message)
            {
                Console.WriteLine($"Закончилась обработка сообщения \"{message}\" от {FirstName}({TelegramId})");
            }
        }
    }
}
