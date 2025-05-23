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
    public class JsonProcedureRepository : IProcedureRepository
    {
        private readonly string _storage;

        public JsonProcedureRepository(string storage)
        {
            _storage = storage;
            try
            {
                Directory.CreateDirectory(_storage);
            }
            catch (Exception)
            {
                Console.WriteLine("ошибка при создании репозитория Услуг");
                throw;
            }
        }
        public async Task Add(Procedure procedure, CancellationToken cancellationToken)
        {
            string filePath = Path.Combine(_storage, $"{procedure.Id}.json");
            string json = JsonSerializer.Serialize(procedure, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(filePath, json, cancellationToken);
        }
        public async Task<IReadOnlyList<Procedure>> GetProcedures(CancellationToken cancellationToken)
        {
            List<Procedure> procedures = new();
            foreach(string file in Directory.GetFiles(_storage))
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var procedure = JsonSerializer.Deserialize<Procedure>(json);
                if (procedure != null) procedures.Add(procedure);
            }
            return procedures.AsReadOnly();
        }
        public async Task<bool> Delete(Guid procedureId, CancellationToken cancellationToken)
        {
            var procedures = await GetProcedures(cancellationToken);
            var p = procedures.Any(p => p.Id == procedureId);
            if (p)
            {
                string filePath = Path.Combine(_storage, $"{procedureId.ToString()}.json");
                File.Delete(filePath);
                return true;
            }
            return false;
        }
    }
}
