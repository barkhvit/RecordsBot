using LinqToDB;
using RecordBot.DataAccess;
using RecordBot.DataAccess.Model;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class NotificationService : INotificationService
    {
        private readonly IDataContextFactory<DBContext> _factory;
        public NotificationService(IDataContextFactory<DBContext> factory)
        {
            _factory = factory;
        }

        //Создает нотификацию. Если запись с userId и type уже есть, то вернуть false и не добавлять запись, иначе вернуть true
        public async Task<bool> AddNotification(Guid userId, string type, string text, DateTime scheduleAt, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            bool exists = await context.notificationModel.AnyAsync(n => n.UserId == userId && n.Type == type, ct);
            if (exists) return false;// Запись уже существует

            // создаем новую нотификацию и добавляем в БД
            NotificationModel notification = new()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Type = type,
                Text = text,
                ScheduledAt = scheduleAt,
                NotifiedAt = null,
                IsNotified = false
            };

            await context.InsertAsync(notification, token: ct);
            return true;
        }

        //Возвращает нотификации, у которых IsNotified = false && ScheduledAt <= scheduledBefore
        public async Task<IReadOnlyList<Notification>> GetScheduleNotification(DateTime sheduledBefore, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var notifications = await context.notificationModel
                .LoadWith(n => n.User)
                .Where(n => n.IsNotified == false && n.ScheduledAt <= sheduledBefore)
                .ToListAsync(token: ct);
            return notifications.Select(ModelMapper.MapFromModel).ToList();
        }

        //меняет статус отправки IsNotified на true и задаем время отправки сообщения
        public async Task MarkNotified(Guid notificationId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            await context.notificationModel
                .Where(n => n.Id == notificationId)
                .Set(n => n.IsNotified, true)
                .Set(n => n.NotifiedAt, DateTime.UtcNow)
                .UpdateAsync(token: ct);
        }
    }
}
