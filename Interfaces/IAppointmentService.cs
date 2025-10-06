using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Interfaces
{
    public interface IAppointmentService
    {
        Task<IReadOnlyList<Appointment>> GetUserAppointments(Guid userId, CancellationToken ct);
        Task<bool> CancelAppointment(Guid appoinmentId, CancellationToken ct);
        Task<IReadOnlyList<DateTime>> GetSlotsForAppointment(Guid procedureId, CancellationToken ct);

        Task<T?> GetAppointmentById<T>(Guid Id, CancellationToken ct) where T: class, IAppointment;
        Task Add<T>(T appointment, CancellationToken ct)where T: class, IAppointment;
        Task<int> UpdateAsync<T>(T appointment, CancellationToken ct) where T: class, IAppointment;

        Task<IReadOnlyList<AppointmentsView>> GetActualyAppointments(CancellationToken ct);
        Task<IReadOnlyList<AppointmentsView>> GetAppointmentsByDate(DateOnly date, CancellationToken ct);
        
    }
}
