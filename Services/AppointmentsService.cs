using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Services
{
    public class AppointmentsService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IFreePeriodService _freePeriodService;
        private readonly IProcedureService _procedureService;

        public AppointmentsService(IAppointmentRepository _appointmentRepository, IFreePeriodService _freePeriodService, IProcedureService procedureService)
        {
            this._appointmentRepository = _appointmentRepository;
            this._freePeriodService = _freePeriodService;
            _procedureService = procedureService;
        }

        public async Task<bool> CancelAppointment(Guid appoinmentId, CancellationToken ct)
        {
            return await _appointmentRepository.Delete(appoinmentId, ct);
        }

        public async Task<IReadOnlyList<Appointment>> GetUserAppointments(Guid userId, CancellationToken ct)
        {
            var allAppointments = await _appointmentRepository.GetAppointmentsByUserId(userId, ct);
            return allAppointments.Where(a => a.DateTime >= DateTime.Now).ToList().AsReadOnly();
        }

        public async Task<T?> GetAppointmentById<T>(Guid Id, CancellationToken ct) where T: class, IAppointment
        {
            return await _appointmentRepository.GetAppointmentById<T>(Id, ct);
        }


        //возвращает возможные слоты 
        public async Task<IReadOnlyList<DateTime>> GetSlotsForAppointment(Guid procedureId, CancellationToken ct)
        {
            List<DateTime> dateTimes = new();

            var periods = await _freePeriodService.GetAllPeriods(ct); // периоды
            
            var appointments = await _appointmentRepository.GetActualAppointments<Appointment>(ct); //список записей основной табл
            
            var guestAppointments = await _appointmentRepository.GetActualAppointments<GuestAppointment>(ct);//список записей вручную

            var procedure = await _procedureService.GetProcedureByGuidId(procedureId, ct); //процедура по ID из аргументов

            if (periods == null) return dateTimes;

            //добавляем все возможные слоты пока без учета записей
            foreach(var p in periods)
            {
                var dateTime = new DateTime(p.Date, p.StartTime);
                while (dateTime <= new DateTime(p.Date, p.FinishTime).AddMinutes(-procedure.DurationMinutes))
                {
                    if (dateTime > DateTime.Now)
                    {
                        dateTimes.Add(dateTime);
                    }
                    dateTime = dateTime.AddMinutes(procedure.DurationMinutes);
                }
            }
            if (appointments == null) return dateTimes;

            //проверяем слоты на попадание в записи
            foreach(var a in appointments)
            {
                //процедура каждой записи, начало и окончание
                var proc = await _procedureService.GetProcedureByGuidId(a.ProcedureId, ct);
                var startDateTime = a.DateTime;
                var finishDateTime = a.DateTime.AddMinutes(proc.DurationMinutes);
                dateTimes = dateTimes.Where(t => t < startDateTime || t >= finishDateTime).ToList();
            }

            //проверка на попадание в слоты ручных записей
            foreach (var a in guestAppointments)
            {
                //процедура каждой записи, начало и окончание
                var proc = await _procedureService.GetProcedureByGuidId(a.ProcedureId, ct);
                var startDateTime = a.DateTime;
                var finishDateTime = a.DateTime.AddMinutes(proc.DurationMinutes);
                dateTimes = dateTimes.Where(t => t < startDateTime || t >= finishDateTime).ToList();
            }

            return dateTimes;
        }

        public async Task Add<T>(T appointment, CancellationToken ct) where T: class, IAppointment
        {
            await _appointmentRepository.Add(appointment, ct);
        }

        public async Task<IReadOnlyList<AppointmentsView>> GetActualyAppointments(CancellationToken ct)
        {
            //получаем из основной таблицы записи Appointment
            var appointments = await _appointmentRepository.GetActualAppointments<Appointment>(ct);

            //получаем из гостевой GuestAppointment
            var guestAppointments = await _appointmentRepository.GetActualAppointments<GuestAppointment>(ct);

            //объединяем
            return ConcateAppointments(appointments, guestAppointments);
        }

        public async Task<IReadOnlyList<AppointmentsView>> GetAppointmentsByDate(DateOnly date, CancellationToken ct)
        {
            //получаем из основной таблицы записи Appointment
            var appointments = await _appointmentRepository.GetAppointmentsByDate<Appointment>(date, ct);

            //получаем из гостевой GuestAppointment
            var guestAppointments = await _appointmentRepository.GetAppointmentsByDate<GuestAppointment>(date, ct);

            //объединяем
            return ConcateAppointments(appointments, guestAppointments);
        }

        public async Task<int> UpdateAsync<T>(T appointment, CancellationToken ct) where T: class, IAppointment
        {
            return await _appointmentRepository.UpdateAsync(appointment, ct);
        }

        // объединение таблиц Appointment и guestAppointment
        private IReadOnlyList<AppointmentsView> ConcateAppointments(IReadOnlyList<Appointment> appointments, IReadOnlyList<GuestAppointment> guestAppointments)
        {
            var concatApp = new List<AppointmentsView>();
            foreach (var a in appointments)
            {
                concatApp.Add(new AppointmentsView()
                {
                    Id = a.Id,
                    DateTime = a.DateTime,
                    IsConfirmed = a.IsConfirmed,
                    UserId = a.UserId,
                    ProcedureId = a.ProcedureId
                });
            }
            foreach (var ga in guestAppointments)
            {
                concatApp.Add(new AppointmentsView()
                {
                    Id = ga.Id,
                    DateTime = ga.DateTime,
                    IsConfirmed = ga.IsConfirmed,
                    Name = ga.Name,
                    Phone = ga.Phone,
                    ProcedureId = ga.ProcedureId
                });
            }
            return concatApp;
        }
    }
}
