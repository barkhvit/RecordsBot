using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IAppointmentRepository
    {
        Task Add(Appointment appointment, CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetAllAppointments(CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct);
        Task<bool> Delete(Guid appointmentId, CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetAppointmentsByDate(DateOnly dateOnly, CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetActualAppointments(CancellationToken ct);
    }
}
