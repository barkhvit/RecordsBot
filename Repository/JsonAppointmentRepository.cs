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

        public async Task Add(Appointment appointment, CancellationToken ct)
        {
            string dirPath = Path.Combine(_storage, appointment.UserId.ToString());
            string filePath = Path.Combine(dirPath, $"{appointment.Id}.json");
            Directory.CreateDirectory(dirPath);
            string json = JsonSerializer.Serialize(appointment, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, ct);
        }

        public async Task<IReadOnlyList<Appointment>> GetAllAppointments(CancellationToken ct)
        {
            List<Appointment> appoimntments = new();
            foreach(string dir in Directory.GetDirectories(_storage))
            {
                foreach(string file in Directory.GetFiles(dir))
                {
                    string json = await File.ReadAllTextAsync(file, ct);
                    var appointment = JsonSerializer.Deserialize<Appointment>(json);
                    if(appointment != null) appoimntments.Add(appointment);
                }
            }
            return appoimntments.AsReadOnly();
        }
        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByUserId(Guid UserId, CancellationToken ct)
        {
            var appointments = await GetAllAppointments(ct);
            return appointments.Where(a => a.UserId == UserId).ToList();
        }

        public async Task<bool> Delete(Guid appointmentId, CancellationToken ct)
        {
            var appointments = await GetAllAppointments(ct);
            var appointment = appointments.FirstOrDefault(a => a.Id == appointmentId);
            if (appointment!=null)
            {
                string file = Path.Combine(_storage, appointment.UserId.ToString(), $"{appointment.Id}.json");
                File.Delete(file);
                return true;
            }
            return false;
        }

        public async Task<IReadOnlyList<Appointment>> GetAppointmentsByDate(DateOnly dateOnly, CancellationToken ct)
        {
            var appoiintments = await GetAllAppointments(ct);
            return appoiintments.Where(a => DateOnly.FromDateTime(a.dateTime) == dateOnly).ToList();
        }

        public async Task<IReadOnlyList<Appointment>> GetActualAppointments(CancellationToken ct)
        {
            var appointments = await GetAllAppointments(ct);
            return appointments.Where(a => DateOnly.FromDateTime(a.dateTime) >= DateOnly.FromDateTime(DateTime.Now)).ToList();
        }
    }
}
