using RecordBot.BackGroundTask;
using RecordBot.Commands;
using RecordBot.DataAccess;
using RecordBot.Handlers;
using RecordBot.Interfaces;
using RecordBot.Repository;
using RecordBot.Scenarios;
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
            //string token = Environment.GetEnvironmentVariable("BotForTesting", EnvironmentVariableTarget.User);
            ITelegramBotClient botClient = new TelegramBotClient(token);

            //база данных
            string connectionString = "User ID=postgres;Password=Alekseev4+;Host=localhost;Port=5432;Database=RecordBot;Include Error Detail=true";
             
            IDataContextFactory<DBContext> dataContextFactory = new DataContextFactory(connectionString);
            
            //репозитории
            IScenarioContextRepository scenarioContextRepository = new InMemoryScenarioContextRepository();
            IUserRepository userRepository = new SqlUserRepository(dataContextFactory);
            IFreePeriodRepository freePeriodRepository = new SqlFreePeriodRepository(dataContextFactory);
            IProcedureRepository procedureRepository = new SqlProcedureRepository(dataContextFactory);
            IAppointmentRepository appointmentRepository = new SqlAppointmentRepository(dataContextFactory);

            //сервисы
            IUserService userService = new UserService(userRepository);
            IFreePeriodService freePeriodService = new FreePeriodService(freePeriodRepository);
            IProcedureService procedureService = new ProcedureService(procedureRepository);
            IAppointmentService appointmentService = new AppointmentsService(appointmentRepository, freePeriodService,procedureService);
            INotificationService notificationService = new NotificationService(dataContextFactory);

            //сценарии
            IEnumerable<IScenario> scenarios = new List<IScenario>
            {
                new AddFreePeriodScenario(freePeriodService), 
                new AddProcedureScenario(procedureService),
                new AddAppointmentScenario(appointmentService, procedureService,freePeriodService, userService),
                new SendMessageToAdminScenario(userService)
            };

            //контроллер ответов от пользователя
            var botController = new BotController(botClient, userService, freePeriodService, procedureService
                , appointmentService, scenarios, scenarioContextRepository);

            //CancellationToken
            var _cts = new CancellationTokenSource();

            //подписываем на методы делегаты - события
            botController.OnHandleUpdateStarted += StartMessage;
            botController.OnHandleUpdateComplete += EndMessage;

            //1. Создаем runner для фоновых задач
            var backgroundTaskRunner = new BackGroundTaskRunner();

            //2. Регистрируем задачу создания сообщения о завтрашних записях
            //    Таймаут - 1 час, передаем репозиторий и клиент бота
            backgroundTaskRunner.AddTask(new TomorrowAppointmentBackGroundTask(appointmentService, notificationService, procedureService));

            //3. Фоновая задача отправки нотификаций
            backgroundTaskRunner.AddTask(new SendNotificationBackgroundTask(notificationService, botClient));

            //. Запускаем фоновые задачи
            //    Передаем токен отмены, чтобы задачи могли остановиться при завершении приложения
            backgroundTaskRunner.StartTasks(_cts.Token);


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
