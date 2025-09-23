using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RecordBot.BackGroundTask;
using RecordBot.Handlers;
using RecordBot.Interfaces;
using RecordBot.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace RecordBot.Services
{
    public class BotBackgroundService : BackgroundService
    {
        private readonly ILogger<BotBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private CancellationTokenSource _cts;
        public BotBackgroundService(ILogger<BotBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _cts = new CancellationTokenSource();
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Bot service starting...");
            using(var scope = _serviceProvider.CreateScope())
            {
                var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
                var botController = scope.ServiceProvider.GetRequiredService<BotController>();

                //подписываем на методы делегаты - события
                botController.OnHandleUpdateStarted += StartMessage;
                botController.OnHandleUpdateComplete += EndMessage;

                //1. Создаем runner для фоновых задач
                var backgroundTaskRunner = scope.ServiceProvider.GetRequiredService<BackGroundTaskRunner>();

                //2. Регистрируем задачу создания сообщения о завтрашних записях
                //    Таймаут - 1 час, передаем репозиторий и клиент бота
                var tomorrowAppointmentBackGroundTask = scope.ServiceProvider.GetRequiredService<TomorrowAppointmentBackGroundTask>();
                backgroundTaskRunner.AddTask(tomorrowAppointmentBackGroundTask);

                //3. Фоновая задача отправки нотификаций
                var sendNotificationBackgroundTask = scope.ServiceProvider.GetRequiredService<SendNotificationBackgroundTask>();
                backgroundTaskRunner.AddTask(sendNotificationBackgroundTask);

                //. Запускаем фоновые задачи
                //    Передаем токен отмены, чтобы задачи могли остановиться при завершении приложения
                backgroundTaskRunner.StartTasks(_cts.Token);


                try
                {
                    //прослушка
                    botClient.StartReceiving(
                        updateHandler: botController.HandleUpdateAsync,
                        errorHandler: botController.HandleErrorAsync,
                        cancellationToken: _cts.Token);

                    _logger.LogInformation("Bot started successfuly");

                    //ждем сигнала остановки
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        await Task.Delay(1000, stoppingToken);
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error starting bot service");
                    throw;
                }
                finally
                {
                    if (botController != null)
                    {
                        //отписка событий от методов
                        botController.OnHandleUpdateStarted -= StartMessage;
                        botController.OnHandleUpdateComplete -= EndMessage;
                    }
                }
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Bot service stopping...");
            _cts.Cancel();
            await base.StopAsync(cancellationToken);
            _logger.LogInformation("Bot service stopped");
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
