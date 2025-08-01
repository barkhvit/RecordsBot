using RecordBot.Interfaces;
using RecordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace RecordBot.BackGroundTask
{
    public class NotificationBackGroundTask : BackGroundTask
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IUserService _userService;
        private readonly ITelegramBotClient _botClient;
        public NotificationBackGroundTask(
            IAppointmentService appointmentService,
            IUserService userService,
            ITelegramBotClient botClient) 
            : base(TimeSpan.FromSeconds(30), nameof(NotificationBackGroundTask))
            {
                _appointmentService = appointmentService;
                _userService = userService;
                _botClient = botClient;
            }

        protected override async Task Execute(CancellationToken ct)
        {
            DateTime tommorow = DateTime.UtcNow.AddDays(1);
            var appointmentsTommorow = await _appointmentService.GetAppointmentsByDate(DateOnly.FromDateTime(tommorow),ct);
            if (appointmentsTommorow != null)
            {
                foreach(var a in appointmentsTommorow)
                {
                    var user = await _userService.GetUserByUserId(a.UserId, ct);
                    await _botClient.SendMessage(user.TelegramId, $"У вас запись на {tommorow.ToString("dd.MM.yyyy")}",cancellationToken:ct);
                }
            }
            else
            {
                Console.WriteLine($"записей на {tommorow.ToString("dd.MM.yyyy")} нет.");
            }
        }
    }
}
