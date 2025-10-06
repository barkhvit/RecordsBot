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

        public async Task Add<T>(T appointment, CancellationToken ct) where T:class, IAppointment
        {
            using var context = _factory.CreateDataContext();
            if (typeof(T) == typeof(Appointment))
                await context.InsertAsync(ModelMapper.MapToModel((Appointment)(object)appointment), token: ct);

            if (typeof(T) == typeof(GuestAppointment))
                await context.InsertAsync(ModelMapper.MapToModel((GuestAppointment)(object)appointment), token: ct);
        }

        public async Task<bool> Delete(Guid appointmentId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();

            // Пытаемся удалить из основной таблицы
            var deletedFromRegular = await context.appointmentModels
                .Where(a => a.Id == appointmentId)
                .DeleteAsync(ct);

            if (deletedFromRegular > 0)
                return true;

            // Если не нашли в основной, пытаемся удалить из гостевой
            var deletedFromGuest = await context.guestAppointmentsModel
                .Where(a => a.Id == appointmentId)
                .DeleteAsync(ct);

            return deletedFromGuest > 0;
        }

        //записи на сегодня и позже
        public async Task<IReadOnlyList<T>> GetActualAppointments<T>(CancellationToken ct) where T : class, IAppointment
        {
            using var context = _factory.CreateDataContext();
            if(typeof(T) == typeof(Appointment)) // если тип Appointment
            {
                var appointments = await context.appointmentModels
                    .Where(a => a.DateTime >= DateTime.Now)
                    .ToListAsync(ct);
                return appointments.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            if (typeof(T) == typeof(GuestAppointment)) //если тип GuestAppointment
            {
                var guestAppointments = await context.guestAppointmentsModel
                    .Where(a => a.DateTime >= DateTime.Now)
                    .ToListAsync(ct);
                return guestAppointments.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            throw new ArgumentException($"Unsupported type: {typeof(T)}");
        }

        public async Task<IReadOnlyList<T>> GetAllAppointments<T>(CancellationToken ct) where T: class, IAppointment
        {
            using var context = _factory.CreateDataContext();

            if(typeof(T) == typeof(Appointment))
            {
                var appointments = await context.appointmentModels.ToListAsync(ct);
                return appointments.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            if (typeof(T) == typeof(GuestAppointment))
            {
                var guestAppointments = await context.guestAppointmentsModel.ToListAsync(ct);
                return guestAppointments.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            throw new ArgumentException($"Unsupported type: {typeof(T)}");
        }

        public async Task<IReadOnlyList<T>> GetAppointmentsByDate<T>(DateOnly dateOnly, CancellationToken ct) where T: class, IAppointment
        {
            using var context = _factory.CreateDataContext();
            var startDateTime = dateOnly.ToDateTime(TimeOnly.MinValue);
            var endDateTime = dateOnly.ToDateTime(TimeOnly.MaxValue);

            if(typeof(T) == typeof(Appointment))
            {
                var appointments = await context.appointmentModels
                    .Where(a => a.DateTime >= startDateTime && a.DateTime < endDateTime)
                    .ToListAsync(ct);
                return appointments.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            if(typeof(T) == typeof(GuestAppointment))
            {
                var guestAppointment = await context.guestAppointmentsModel
                    .Where(a => a.DateTime >= startDateTime && a.DateTime < endDateTime)
                    .ToListAsync(ct);
                return guestAppointment.Select(a => ModelMapper.MapFromModel(a)).Cast<T>().ToList();
            }
            throw new ArgumentException($"Unsupported type: {typeof(T)}");
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct)
        {
            using var context = _factory.CreateDataContext();
            
            var appointments = await context.appointmentModels
                .Where(a => a.UserId == UserId)
                .ToListAsync(ct);
            return appointments.Select(ModelMapper.MapFromModel).ToList();
        }

        public async Task<int> UpdateAsync<T>(T appointment, CancellationToken ct) where T: class, IAppointment
        {
            using var context = _factory.CreateDataContext();

            switch (appointment)
            {
                case Appointment app:
                    return await context.appointmentModels
                        .Where(a => a.Id == app.Id)
                        .Set(a => a.DateTime, app.DateTime)
                        .Set(a => a.IsConfirmed, app.IsConfirmed)
                        .Set(a => a.UserId, app.UserId)
                        .Set(a => a.ProcedureId, app.ProcedureId)
                        .UpdateAsync(ct);
                case GuestAppointment gApp:
                    return await context.guestAppointmentsModel
                        .Where(a => a.Id == gApp.Id)
                        .Set(a => a.DateTime, gApp.DateTime)
                        .Set(a => a.IsConfirmed, gApp.IsConfirmed)
                        .Set(a => a.Name, gApp.Name)
                        .Set(a => a.Phone, gApp.Phone)
                        .Set(a => a.ProcedureId, gApp.ProcedureId)
                        .UpdateAsync(ct);
            }
            throw new ArgumentException($"Unsupported type: {typeof(T)}");
        }

        public async Task<T> GetAppointmentById<T>(Guid Id, CancellationToken ct) where T: class, IAppointment
        {
            using var context = _factory.CreateDataContext();

            //ищем в основной таблице
            var appointment = await context.appointmentModels
                    .FirstOrDefaultAsync(a => a.Id == Id, token: ct);
            if (appointment != null)
            {
                return (T)(object)ModelMapper.MapFromModel(appointment);
            }
            //ищем в гостевой таблице
            else
            {
                var guestAppointment = await context.guestAppointmentsModel
                        .FirstOrDefaultAsync(a => a.Id == Id, token: ct);
                if (guestAppointment != null)
                {
                    return (T)(object)ModelMapper.MapFromModel(guestAppointment);
                }
            }

            throw new ArgumentException($"Неподдерживается тип: {typeof(T)}");
        }
    }
}
