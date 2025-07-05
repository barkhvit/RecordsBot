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

        public async Task<Appointment?> CreateAppointment(long userId, Procedure procedure, DateTime dateTime, CancellationToken ct)
        {
            //получаем период для бронирования
            var freePeriodForReserved = await _freePeriodService.GetFreePeriodForReserved(procedure, dateTime, ct);

            if (freePeriodForReserved != null)
            {
                Appointment appointment = new()
                {
                    Id = Guid.NewGuid(),
                    dateTime = dateTime,
                    TelegramUserId = userId,
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

        public async Task<IReadOnlyList<Appointment>> GetUserAppointments(long userId, CancellationToken ct)
        {
            var allAppointments = await _appointmentRepository.GetAppointmentsByTelegramUserId(userId, ct);
            return allAppointments.Where(a => a.dateTime >= DateTime.UtcNow).ToList().AsReadOnly();
        }

        public async Task<Appointment?> GetAppointmentById(Guid Id, CancellationToken ct)
        {
            var allAppointments = await _appointmentRepository.GetAllAppointments(ct);
            return allAppointments.FirstOrDefault(a => a.Id == Id);
        }

    }
}
