using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class JsonAppointmentRepository : IAppointmentRepository
    {
        private readonly string _storage;
        public JsonAppointmentRepository(string storage)
        {
            _storage = storage;
            try
            {
                Directory.CreateDirectory(_storage);
            }
            catch (Exception)
            {
                Console.WriteLine("Произошла ошибка при создании репозитория Appointment");
                throw;
            }
        }

        public async Task Add<T>(T appointment, CancellationToken ct) where T: class, IAppointment
        {
            switch (appointment)
            {
                case Appointment app:
                    string dirPath = Path.Combine(_storage, app.UserId.ToString());
                    string filePath = Path.Combine(dirPath, $"{app.Id}.json");
                    Directory.CreateDirectory(dirPath);
                    string json = JsonSerializer.Serialize(app, new JsonSerializerOptions { WriteIndented = true });
                    await File.WriteAllTextAsync(filePath, json, ct);
                    return;
            }
            throw new ArgumentException($"Не поддерживается тип {typeof(T)}");
        }

        public async Task<IReadOnlyList<T>> GetAllAppointments<T>(CancellationToken ct) where T: class, IAppointment
        {
            if(typeof(T) == typeof(Appointment))
            {
                List<Appointment> appointments = new();
                foreach (string dir in Directory.GetDirectories(_storage))
                {
                    foreach (string file in Directory.GetFiles(dir))
                    {
                        string json = await File.ReadAllTextAsync(file, ct);
                        var appointment = JsonSerializer.Deserialize<Appointment>(json);
                        if (appointment != null) appointments.Add(appointment);
                    }
                }
                return appointments.Cast<T>().ToList().AsReadOnly();
            }
            throw new ArgumentException($"Не поддерживается тип {typeof(T)}");
        }
        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct)
        {
            var appointments = await GetAllAppointments<Appointment>(ct);
            return appointments.Where(a => a.UserId == UserId).ToList();
        }

        public async Task<bool> Delete(Guid appointmentId, CancellationToken ct)
        {
            //реализовано только для Appointments
            var appointments = await GetAllAppointments<Appointment>(ct);
            var appointment = appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment!=null)
            {
                string file = Path.Combine(_storage, appointment.UserId.ToString(), $"{appointment.Id}.json");
                File.Delete(file);
                return true;
            }
            return false;
        }

        public async Task<IReadOnlyList<T>> GetAppointmentsByDate<T>(DateOnly dateOnly, CancellationToken ct) where T: class, IAppointment
        {
            //только для Appointments
            if(typeof(T) == typeof(Appointment))
            {
                var appointments = await GetAllAppointments<Appointment>(ct);
                return appointments.Where(a => DateOnly.FromDateTime(a.DateTime) == dateOnly).Cast<T>().ToList();
            }
            throw new ArgumentException($"Тип {typeof(T)} не поддерживается");
        }

        public async Task<IReadOnlyList<T>> GetActualAppointments<T>(CancellationToken ct) where T: class, IAppointment
        {
            //только для Appointments
            if(typeof(T) == typeof(Appointment))
            {
                var appointments = await GetAllAppointments<Appointment>(ct);
                return appointments.Where(a => DateOnly.FromDateTime(a.DateTime) >= DateOnly.FromDateTime(DateTime.Now)).Cast<T>().ToList();
            }
            throw new ArgumentException($"Тип {typeof(T)} не поддерживается");
        }

        public Task<int> UpdateAsync<T>(T appointment, CancellationToken ct) where T: class, IAppointment
        {
            throw new NotImplementedException();
        }

        Task<T> IAppointmentRepository.GetAppointmentById<T>(Guid Id, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
