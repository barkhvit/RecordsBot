using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface INotificationService
    {
        //Создает нотификацию. Если запись с userId и type уже есть, то вернуть false и не добавлять запись, иначе вернуть true
        Task<bool> AddNotification(
            Guid userId,
            string type,
            string text,
            DateTime scheduleAt,
            CancellationToken ct);

        //Возвращает нотификации, у которых IsNotified = false && ScheduledAt <= scheduledBefore
        Task<IReadOnlyList<Notification>> GetScheduleNotification(DateTime sheduledBefore, CancellationToken ct);

        //меняет статус отправки IsNotified на true
        Task MarkNotified(Guid notificationId, CancellationToken ct);
    }
}
