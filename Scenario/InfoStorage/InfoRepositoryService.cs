using RecordBot.Models;
using RecordBot.Scenario.InfoStorage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecordBot.Scenario.InfoStorage
{
    public class InfoRepositoryService
    {
        private readonly ConcurrentDictionary<long, InfoContext> _contexts = new();

        public async Task<InfoContext> GetOrCreateContext(long telegramUserId, CancellationToken ct)
        {
            // Проверяем наличие контекста в словаре
            if (_contexts.TryGetValue(telegramUserId, out var existingContext))
            {
                return existingContext;
            }

            // Создаем новый контекст, если его нет
            var newContext = new InfoContext();

            // Пытаемся добавить новый контекст атомарно
            var addedContext = _contexts.GetOrAdd(telegramUserId, newContext);

            // Если другой поток успел добавить контекст первым, возвращаем его
            if (addedContext != newContext)
            {
                // При необходимости освобождаем ресурсы нового контекста
                if (newContext is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (newContext is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            return addedContext;
        }

        public async Task AddProcedureIdInfo(long telegramUserId, Guid ProcedureId, CancellationToken ct)
        {
            var context = await GetOrCreateContext(telegramUserId, ct);
            context.Data["процедура"] = ProcedureId;
        }

        public async Task AddProcedureDateInfo(long telegramUserId, DateOnly date, CancellationToken ct)
        {
            var context = await GetOrCreateContext(telegramUserId, ct);
            context.Data["дата процедуры"] = date;
        }

        public async Task AddProcedureTimeInfo(long telegramUserId, TimeOnly time, CancellationToken ct)
        {
            var context = await GetOrCreateContext(telegramUserId, ct);
            context.Data["время процедуры"] = time;
        }
    }
}
