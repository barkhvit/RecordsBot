using LinqToDB;
using RecordBot.DataAccess;
using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class SqlAppointmentRepository : IAppointmentRepository
    {
        private readonly IDataContextFactory<DBContext> _factory;
        public SqlAppointmentRepository(IDataContextFactory<DBContext> dataContextFactory)
        {
            _factory = dataContextFactory;
        }
        public async Task Add(Appointment appointment, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            await context.InsertAsync(ModelMapper.MapToModel(appointment), token: ct);
        }

        public async Task<bool> Delete(Guid appointmentId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var appointment = context.appointmentModels.Where(a => a.Id == appointmentId);
            if (appointment != null)
            {
                await context.appointmentModels
                    .Where(a => a.Id == appointmentId)
                    .DeleteAsync(ct);
                return true;
            }
            return false;
        }
        //записи на сегодня и позже
        public async Task<IReadOnlyList<Appointment>> GetActualAppointments(CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var appointments = await context.appointmentModels
                .Where(a => a.dateTime >= DateTime.Now)
                .ToListAsync(ct);
            return appointments.Select(a => ModelMapper.MapFromModel(a)).ToList();
        }

        public async Task<IReadOnlyList<Appointment>> GetAllAppointments(CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var appointments = await context.appointmentModels.ToListAsync(ct);
            return appointments.Select(a => ModelMapper.MapFromModel(a)).ToList();
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByDate(DateOnly dateOnly, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var appointments = await context.appointmentModels
                .Where(a => DateOnly.FromDateTime(a.dateTime) == dateOnly)
                .ToListAsync(ct);
            return appointments.Select(a => ModelMapper.MapFromModel(a)).ToList();
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            var appointments = await context.appointmentModels
                .Where(a => a.UserId == UserId)
                .ToListAsync(ct);
            return appointments.Select(ModelMapper.MapFromModel).ToList();
        }
    }
}
