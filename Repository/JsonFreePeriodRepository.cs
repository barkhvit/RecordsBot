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
                    string json = File.ReadAllText(file);
                    var item = JsonSerializer.Deserialize<FreePeriod>(json);
                    if (item != null) freePeriods.Add(item);
                }
            }
            return await Task.FromResult(freePeriods.AsReadOnly());
        }
        public Task<IReadOnlyList<FreePeriod>> GetFreePeriods(Procedure procedure, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<DateOnly>> GetDates(CancellationToken cancellationToken)
        {
            var freePeriods = await GetAllPeriods(cancellationToken);
            var dates = freePeriods.Select(d => d.Date).ToList().AsReadOnly();
            return dates;
        }

        public Task<IReadOnlyList<DateTime>> GetTimeForReservedByDate(DateOnly date, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
