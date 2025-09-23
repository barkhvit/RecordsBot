using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        static async Task Main(string[] args)
        {
            try
            {
                IHost host = Host.CreateDefaultBuilder(args)
                .UseWindowsService(options =>
                {
                    options.ServiceName = "BotRiveGoshService";
                })
                .ConfigureServices(ConfigureService)
                .ConfigureLogging(logging =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventLog(settings =>
                    {
                        settings.SourceName = "BotRiveGoshService";
                    });
                })
                .Build();

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Критическая ошибка: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                // Логируйте в файл или другую систему
                File.WriteAllText("crash_log.txt", ex.ToString());

                // Даем время прочитать сообщение перед закрытием
                Console.WriteLine("Нажмите любую клавишу для выхода...");
                Console.ReadKey();
            }
        }

        //public static async Task Main(string[] args)
        //{
        //    //бот-клиент @BarkhvitRecordsBot
        //    //string token = Environment.GetEnvironmentVariable("TELEGRAM_RecordsBOT_TOKEN", EnvironmentVariableTarget.User);
        //    string token = Environment.GetEnvironmentVariable("BotForTesting", EnvironmentVariableTarget.User);
        //    ITelegramBotClient botClient = new TelegramBotClient(token);

        //    //база данных
        //    string connectionString = "User ID=postgres;Password=Alekseev4+;Host=localhost;Port=5432;Database=RecordBot;Include Error Detail=true";

        //    IDataContextFactory<DBContext> dataContextFactory = new DataContextFactory(connectionString);

        //    //репозитории
        //    IScenarioContextRepository scenarioContextRepository = new InMemoryScenarioContextRepository();
        //    IUserRepository userRepository = new SqlUserRepository(dataContextFactory);
        //    IFreePeriodRepository freePeriodRepository = new SqlFreePeriodRepository(dataContextFactory);
        //    IProcedureRepository procedureRepository = new SqlProcedureRepository(dataContextFactory);
        //    IAppointmentRepository appointmentRepository = new SqlAppointmentRepository(dataContextFactory);

        //    //сервисы
        //    IUserService userService = new UserService(userRepository);
        //    IFreePeriodService freePeriodService = new FreePeriodService(freePeriodRepository);
        //    IProcedureService procedureService = new ProcedureService(procedureRepository);
        //    IAppointmentService appointmentService = new AppointmentsService(appointmentRepository, freePeriodService,procedureService);
        //    INotificationService notificationService = new NotificationService(dataContextFactory);

        //    //сценарии
        //    IEnumerable<IScenario> scenarios = new List<IScenario>
        //    {
        //        new AddFreePeriodScenario(freePeriodService), 
        //        new AddProcedureScenario(procedureService),
        //        new AddAppointmentScenario(appointmentService, procedureService,freePeriodService, userService),
        //        new SendMessageToAdminScenario(userService)
        //    };

        //    //контроллер ответов от пользователя
        //    var botController = new BotController(botClient, userService, freePeriodService, procedureService
        //        , appointmentService, scenarios, scenarioContextRepository);

        //    //CancellationToken
        //    var _cts = new CancellationTokenSource();

        //    //подписываем на методы делегаты - события
        //    botController.OnHandleUpdateStarted += StartMessage;
        //    botController.OnHandleUpdateComplete += EndMessage;

        //    //1. Создаем runner для фоновых задач
        //    var backgroundTaskRunner = new BackGroundTaskRunner();

        //    //2. Регистрируем задачу создания сообщения о завтрашних записях
        //    //    Таймаут - 1 час, передаем репозиторий и клиент бота
        //    backgroundTaskRunner.AddTask(new TomorrowAppointmentBackGroundTask(appointmentService, notificationService, procedureService));

        //    //3. Фоновая задача отправки нотификаций
        //    backgroundTaskRunner.AddTask(new SendNotificationBackgroundTask(notificationService, botClient));

        //    //. Запускаем фоновые задачи
        //    //    Передаем токен отмены, чтобы задачи могли остановиться при завершении приложения
        //    backgroundTaskRunner.StartTasks(_cts.Token);


        //    //прослушка бота
        //    try
        //    {
        //        botClient.StartReceiving(
        //            updateHandler: botController.HandleUpdateAsync,
        //            errorHandler: botController.HandleErrorAsync,
        //            cancellationToken: _cts.Token);

        //        Console.WriteLine("Бот запущен");

        //        await Task.Delay(-1, _cts.Token);
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        if(botController != null)
        //        {
        //            //отписка событий от методов
        //            botController.OnHandleUpdateStarted -= StartMessage;
        //            botController.OnHandleUpdateComplete -= EndMessage;
        //        }
        //    }

        //    void StartMessage(long TelegramId, string FirstName, string message)
        //    {
        //        Console.WriteLine($"Началась обработка сообщения \"{message}\" от {FirstName}({TelegramId})");
        //    }
        //    void EndMessage(long TelegramId, string FirstName, string message)
        //    {
        //        Console.WriteLine($"Закончилась обработка сообщения \"{message}\" от {FirstName}({TelegramId})");
        //    }
        //}

        private static void ConfigureService(IServiceCollection services)
        {
            // Регистрируем BackgroundService
            services.AddHostedService<BotBackgroundService>();

            //регистрация бота
            services.AddSingleton<ITelegramBotClient>(sp =>
            {
                //string token = Environment.GetEnvironmentVariable("TELEGRAM_RecordsBOT_TOKEN", EnvironmentVariableTarget.User);
                string token = Environment.GetEnvironmentVariable("BotForTesting", EnvironmentVariableTarget.User);
                return new TelegramBotClient(token);
            });

            //database
            services.AddSingleton<IDataContextFactory<DBContext>>(sp =>
            {
                string connectionString = "User ID=postgres;Password=Alekseev4+;Host=localhost;Port=5432;Database=RecordBot;Include Error Detail=true";
                return new DataContextFactory(connectionString);
            });

            //handlers
            services.AddScoped<BotController>();
            services.AddScoped<MessageUpdateHandler>();
            services.AddScoped<CallBackUpdateHandler>();
            services.AddScoped<ReplyToMessageUpdateHandler>();

            //repositories
            services.AddScoped<IAppointmentRepository, SqlAppointmentRepository>();
            services.AddScoped<IFreePeriodRepository, SqlFreePeriodRepository>();
            services.AddScoped<IProcedureRepository, SqlProcedureRepository>();
            services.AddScoped<IUserRepository, SqlUserRepository>();
            services.AddScoped<IScenarioContextRepository, InMemoryScenarioContextRepository>();

            //services
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IFreePeriodService, FreePeriodService>();
            services.AddScoped<IProcedureService, ProcedureService>();
            services.AddScoped<IAppointmentService, AppointmentsService>();
            services.AddScoped<INotificationService, NotificationService>();

            //commands
            services.AddScoped<Commands.Commands>();
            services.AddScoped<CommandsForAdmin>();
            services.AddScoped<CommandsForAppointments>();
            services.AddScoped<CommandsForFreePeriod>();
            services.AddScoped<CommandsForMainMenu>();
            services.AddScoped<CommandsForProcedures>();

            // Background tasks
            services.AddScoped<SendNotificationBackgroundTask>();
            services.AddScoped<TomorrowAppointmentBackGroundTask>();

            // Регистрируем все фоновые задачи как IBackGroundTask
            services.AddScoped<IBackGroundTask>(provider =>
                provider.GetRequiredService<SendNotificationBackgroundTask>());

            services.AddScoped<IBackGroundTask>(provider =>
                provider.GetRequiredService<TomorrowAppointmentBackGroundTask>());

            services.AddScoped<BackGroundTaskRunner>();

            //scenarios
            services.AddScoped<IScenario, AddAppointmentScenario>();
            services.AddScoped<IScenario, AddFreePeriodScenario>();
            services.AddScoped<IScenario, AddProcedureScenario>();
            services.AddScoped<IScenario, SendMessageToAdminScenario>();
            services.AddSingleton<IEnumerable<IScenario>>(sp =>
            {
                var appointmentService = sp.GetRequiredService<IAppointmentService>();
                var procedureService = sp.GetRequiredService<IProcedureService>();
                var freePeriodService = sp.GetRequiredService<IFreePeriodService>();
                var userService = sp.GetRequiredService<IUserService>();
                return new List<IScenario>
                {
                    new AddAppointmentScenario(appointmentService, procedureService, freePeriodService,userService),
                    new AddFreePeriodScenario(freePeriodService),
                    new AddProcedureScenario(procedureService),
                    new SendMessageToAdminScenario(userService)
                };
            });

        }
    }
}
