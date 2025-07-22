using RecordBot.Interfaces;
using RecordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RecordBot.Repository
{
    public class JsonFreePeriodRepository : IFreePeriodRepository
    {
        private readonly string _storage;
        public JsonFreePeriodRepository(string storage)
        {
            _storage = storage;
            try
            {
                Directory.CreateDirectory(_storage);
            }
            catch(Exception)
            {
                Console.WriteLine("Произошла ошибка при создании репозитория FreePeriod");
                throw;
            }
        }
        public async Task Add(FreePeriod freePeriod, CancellationToken cancellationToken)
        {
            string dirPath = Path.Combine(_storage, freePeriod.Date.ToString());
            string filePath = Path.Combine(dirPath, $"{freePeriod.FreePeriodId}.json");
            Directory.CreateDirectory(dirPath);
            string json = JsonSerializer.Serialize(freePeriod, new JsonSerializerOptions { WriteIndented = true});
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        public async Task<IReadOnlyList<FreePeriod>> GetAllPeriods(CancellationToken cancellationToken)
        {
            List<FreePeriod> freePeriods = new();
            foreach(string dir in Directory.GetDirectories(_storage))
            {
                foreach(string file in Directory.GetFiles(dir))
                {
                    string json = await File.ReadAllTextAsync(file,cancellationToken);
                    var item = JsonSerializer.Deserialize<FreePeriod>(json);
                    if (item != null) freePeriods.Add(item);
                }
            }
            return freePeriods.Where(p => p.Date>=DateOnly.FromDateTime(DateTime.Now)).ToList().AsReadOnly();
        }
        

        public async Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken)
        {
            var freePeriods = await GetAllPeriods(cancellationToken);
            var dates = freePeriods.Select(d => d.Date).ToList().AsReadOnly();
            return dates;
        }

        

        //разделение периода
        public async Task<bool> SplitPeriod(FreePeriod freePeriod, DateTime dateTime, int duration, CancellationToken ct)
        {
            DateTime startPeriod = new DateTime(freePeriod.Date, freePeriod.StartTime);
            DateTime finishPeriod = new DateTime(freePeriod.Date, freePeriod.FinishTime);
            DateTime startAppointment = dateTime;
            DateTime finishAppointment = dateTime.AddMinutes(duration);

            FreePeriod freePeriod1 = new(){FreePeriodId = Guid.NewGuid(),Date = freePeriod.Date,
                StartTime = TimeOnly.FromDateTime(startPeriod),FinishTime = TimeOnly.FromDateTime(startAppointment)};

            FreePeriod freePeriod2 = new(){FreePeriodId = Guid.NewGuid(),Date = freePeriod.Date,
                StartTime = TimeOnly.FromDateTime(finishAppointment),FinishTime = TimeOnly.FromDateTime(finishPeriod)};

            bool isDelete = await Delete(freePeriod, ct);
            if (isDelete)
            {
                //добавляем новые периоды
                if(startPeriod < startAppointment && finishPeriod > finishAppointment)
                {
                    await Add(freePeriod1, ct);
                    await Add(freePeriod2, ct);
                    return true;
                }
                else if(startPeriod == startAppointment && finishPeriod > finishAppointment)
                {
                    await Add(freePeriod2, ct); return true;
                }
                else if (startPeriod < startAppointment && finishPeriod == finishAppointment)
                {
                    await Add(freePeriod1, ct); return true;
                }
            }
            return false;
        }

        //удаление периода
        public async Task<bool> Delete(FreePeriod freePeriod, CancellationToken ct)
        {
            string pathFile = Path.Combine(_storage, freePeriod.Date.ToString(), $"{freePeriod.FreePeriodId}.json");
            try
            {
                if (!File.Exists(pathFile)) return false;
                await Task.Run(() => File.Delete(pathFile), ct);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IReadOnlyList<FreePeriod>> GetPeriodsByDate(DateOnly dateOnly, CancellationToken ct)
        {
            var periods = await GetAllPeriods(ct);
            return periods.Where(p => p.Date == dateOnly).ToList();
        }
    }
}
