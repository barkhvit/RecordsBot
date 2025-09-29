using RecordBot.CallBackModels;
using RecordBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

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
            //получаем список нотификаций
            var notifications = await _notificationService.GetScheduleNotification(DateTime.Now, ct);

            foreach(var n in notifications)
            {
                //получаем dto
                var dto = CallBackDto.FromString(n.Type);

                //в зависимости от типа нотификации делаем кнопки InlineKeyboardButtons
                var buttons = new List<InlineKeyboardButton>();
                switch (dto.Action)
                {
                    case nameof(Dto_Action.Not_TA):
                        buttons.Add(InlineKeyboardButton.WithCallbackData("✅ Подтвердить",
                            new CallBackDto(Dto_Objects.Appointment,Dto_Action.App_Cf,dto.Id).ToString()));
                        break;
                    default:
                        buttons.Add(InlineKeyboardButton.WithCallbackData("🏠 Главное меню",
                            new CallBackDto(Dto_Objects.MainMenu, Dto_Action.MM_Show).ToString()));
                        break;
                }

                var replyMarkup = new InlineKeyboardMarkup(buttons);

                //сообщение
                await _botClient.SendMessage(
                    n.User.TelegramId,
                    n.Text,
                    cancellationToken: ct,
                    replyMarkup: replyMarkup);
                await _notificationService.MarkNotified(n.Id, ct);
            }
        }
    }
}
