using RecordBot.CallBackModels;
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
    public class TomorrowAppointmentBackGroundTask : BackGroundTask
    {
        private readonly IAppointmentService _appointmentService;
        private readonly INotificationService _notificationService;
        private readonly IProcedureService _procedureService;
        public TomorrowAppointmentBackGroundTask(
            IAppointmentService appointmentService,
            INotificationService notificationService,
            IProcedureService procedureService) 
            : base(TimeSpan.FromHours(1), nameof(TomorrowAppointmentBackGroundTask))
        {
            _appointmentService = appointmentService;
            _notificationService = notificationService;
            _procedureService = procedureService;
        }

        protected override async Task Execute(CancellationToken ct)
        {
            DateTime tommorow = DateTime.UtcNow.AddDays(1);
            //получаем записи на завтрашний день
            var appointmentsTommorow = await _appointmentService.GetAppointmentsByDate(DateOnly.FromDateTime(tommorow), ct);
            foreach(var a in appointmentsTommorow)
            {
                var procedure = await _procedureService.GetProcedureByGuidId(a.ProcedureId, ct);
                string text = "Добрый день. Напоминаем Вам о записи:\n";

                //в типе хранится строка типа: Notif:Not_TA(tom appointment):appointment.Id
                string type = new CallBackDto(Dto_Objects.Notif, Dto_Action.Not_TA, a.Id).ToString();

                var isAdd = await _notificationService.AddNotification(a.UserId, type, $"{text}{a.dateTime}\n{procedure.Name}",a.dateTime.AddDays(-1),ct);

                if(isAdd) Console.WriteLine($"В БД добавлена нотификация: TomorrowApp_{a.Id}");
            }
        }
    }
}
