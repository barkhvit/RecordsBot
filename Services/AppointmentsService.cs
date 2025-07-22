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
            var appointments = await _appointmentRepository.GetAllAppointments(ct);
            var appointment = appointments.FirstOrDefault(a => a.Id == appoinmentId);
            var isDelete = await _appointmentRepository.Delete(appoinmentId, ct);
            var procedure = await _procedureService.GetProcedureByGuidId(appointment.ProcedureId, ct);
            if (isDelete)
            {
                FreePeriod freePeriod = new FreePeriod()
                {
                    FreePeriodId = Guid.NewGuid(),
                    Date = DateOnly.FromDateTime(appointment.dateTime),
                    StartTime = TimeOnly.FromDateTime(appointment.dateTime),
                    FinishTime = TimeOnly.FromDateTime(appointment.dateTime).AddMinutes(procedure.DurationMinutes)
                };
                return await _freePeriodService.Add(freePeriod, ct);
            }
            return isDelete;
        }

        public async Task<Appointment?> CreateAppointment(Guid userId, Procedure procedure, DateTime dateTime, CancellationToken ct)
        {
            //получаем период для бронирования
            var freePeriodForReserved = await _freePeriodService.GetFreePeriodForReserved(procedure, dateTime, ct);

            if (freePeriodForReserved != null)
            {
                Appointment appointment = new()
                {
                    Id = Guid.NewGuid(),
                    dateTime = dateTime,
                    UserId = userId,
                    isConfirmed = false,
                    ProcedureId = procedure.Id
                };
                bool isSplit = await _freePeriodService.SplitPeriod(freePeriodForReserved, dateTime, procedure.DurationMinutes, ct);
                if (isSplit)
                {
                    await _appointmentRepository.Add(appointment, ct);
                    return appointment;
                }
            }
            return null;
        }

        public async Task<IReadOnlyList<Appointment>> GetUserAppointments(Guid userId, CancellationToken ct)
        {
            var allAppointments = await _appointmentRepository.GetAppointmentsByUserId(userId, ct);
            return allAppointments.Where(a => a.dateTime >= DateTime.Now).ToList().AsReadOnly();
        }

        public async Task<Appointment?> GetAppointmentById(Guid Id, CancellationToken ct)
        {
            var allAppointments = await _appointmentRepository.GetAllAppointments(ct);
            return allAppointments.FirstOrDefault(a => a.Id == Id);
        }

        //возвращает возможные слоты 
        public async Task<IReadOnlyList<DateTime>> GetSlotsForAppointment(Guid procedureId, CancellationToken ct)
        {
            List<DateTime> dateTimes = new();
            var periods = await _freePeriodService.GetAllPeriods(ct);
            var appointments = await _appointmentRepository.GetActualAppointments(ct);
            var procedure = await _procedureService.GetProcedureByGuidId(procedureId, ct);
            if (periods == null) return dateTimes;
            //добавляем все возможные слоты
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
                var startDateTime = a.dateTime;
                var finishDateTime = a.dateTime.AddMinutes(proc.DurationMinutes);
                dateTimes = dateTimes.Where(t => t < startDateTime || t >= finishDateTime).ToList();
            }
            return dateTimes;
        }

        public async Task Add(Appointment appointment, CancellationToken ct)
        {
            await _appointmentRepository.Add(appointment, ct);
        }

    }
}
