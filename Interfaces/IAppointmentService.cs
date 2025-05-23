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
        Task<Appointment> CreateAppointment(long userId, int procedureId, DateTime dateTime);
        Task<List<Appointment>> GetUserAppointments(long userId);
        Task<bool> CancelAppointment(int appoinmentId);
    }
}
