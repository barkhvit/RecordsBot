using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace RecordBot.BackGroundTask
{
    public class SendNotificationBackgroundTask : BackGroundTask
    {
        private readonly INotificationService _notificationService;
        private readonly ITelegramBotClient _botClient;
        public SendNotificationBackgroundTask(INotificationService notificationService, ITelegramBotClient botClient) 
            : base(TimeSpan.FromMinutes(1), nameof(SendNotificationBackgroundTask))
        {
            _notificationService = notificationService;
            _botClient = botClient;
        }

        //получаем нотификации для отправки
        //отправляем нотификации
        protected override async Task Execute(CancellationToken ct)
        {
            var notifications = await _notificationService.GetScheduleNotification(DateTime.UtcNow, ct);
            foreach(var n in notifications)
            {
                await _botClient.SendMessage(
                    n.User.TelegramId,
                    n.Text,
                    cancellationToken: ct);
                await _notificationService.MarkNotified(n.Id, ct);
            }
        }
    }
}
