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
        Task<Appointment?> CreateAppointment(long userId, Procedure procedure, DateTime dateTime, CancellationToken ct);
        Task<IReadOnlyList<Appointment>> GetUserAppointments(long userId, CancellationToken ct);
        Task<bool> CancelAppointment(Guid appoinmentId, CancellationToken ct);
        Task<Appointment?> GetAppointmentById(Guid Id, CancellationToken ct);
    }
}
