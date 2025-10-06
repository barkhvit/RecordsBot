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
        Task Add<T>(T appointment, CancellationToken ct) where T: class, IAppointment;
        Task<IReadOnlyList<T>> GetAllAppointments<T>(CancellationToken ct) where T : class, IAppointment;
        Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct);
        Task<T> GetAppointmentById<T>(Guid Id, CancellationToken ct) where T : class, IAppointment;

        Task<bool> Delete(Guid appointmentId, CancellationToken ct);
        Task<IReadOnlyList<T>> GetAppointmentsByDate<T>(DateOnly dateOnly, CancellationToken ct) where T : class, IAppointment;
        Task<IReadOnlyList<T>> GetActualAppointments<T>(CancellationToken ct) where T : class, IAppointment;
        Task<int> UpdateAsync<T>(T appointment, CancellationToken ct) where T : class, IAppointment;
    }
}
